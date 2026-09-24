using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Visual-only, gently tapered adhesion band. Add to a NEW child of Adhesion.
/// Its parent AdhesionController already controls position, rotation, stretch
/// and cutting; this component does not touch any of those mechanics.
/// Local X runs from -0.5 to +0.5, matching the original unit cube.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FibrousAdhesionVisual : MonoBehaviour
{
    [Header("Band Shape (relative to the Adhesion transform)")]
    [Range(0.1f, 1.5f)] public float width = 0.9f;
    [Range(0.1f, 1.5f)] public float thickness = 0.65f;
    [Range(0.1f, 1f)] public float endWidthMultiplier = 0.65f;
    [Range(6, 40)] public int lengthSegments = 20;
    [Range(6, 24)] public int radialSegments = 12;

    [System.NonSerialized] private Mesh generatedMesh;

    private void OnEnable() => GenerateVisual();

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall -= RegenerateInEditor;
        UnityEditor.EditorApplication.delayCall += RegenerateInEditor;
    }

    private void RegenerateInEditor()
    {
        UnityEditor.EditorApplication.delayCall -= RegenerateInEditor;
        if (this != null && isActiveAndEnabled && !Application.isPlaying)
            GenerateVisual();
    }
#endif

    [ContextMenu("Generate Fibrous Adhesion Visual")]
    public void GenerateVisual()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter == null) return;

        if (generatedMesh != null)
        {
            if (Application.isPlaying) Destroy(generatedMesh);
            else DestroyImmediate(generatedMesh);
            generatedMesh = null;
        }

        int along = Mathf.Max(6, lengthSegments);
        int around = Mathf.Max(6, radialSegments);
        List<Vector3> vertices = new List<Vector3>((along + 1) * around + 2);
        List<int> triangles = new List<int>(along * around * 6 + around * 6);

        for (int i = 0; i <= along; i++)
        {
            float t = (float)i / along;
            float x = t - 0.5f;

            // Broadest near the middle, gently tapered towards attachments.
            float taper = Mathf.Lerp(endWidthMultiplier, 1f,
                Mathf.Pow(Mathf.Sin(Mathf.PI * t), 0.8f));

            for (int j = 0; j < around; j++)
            {
                float angle = Mathf.PI * 2f * j / around;
                vertices.Add(new Vector3(
                    x,
                    Mathf.Cos(angle) * width * 0.5f * taper,
                    Mathf.Sin(angle) * thickness * 0.5f * taper
                ));
            }
        }

        for (int i = 0; i < along; i++)
        {
            for (int j = 0; j < around; j++)
            {
                int next = (j + 1) % around;
                int a = i * around + j;
                int b = (i + 1) * around + j;
                int c = i * around + next;
                int d = (i + 1) * around + next;
                triangles.Add(a); triangles.Add(c); triangles.Add(b);
                triangles.Add(c); triangles.Add(d); triangles.Add(b);
            }
        }

        // Close the cut ends of the visual mesh.
        int startCenter = vertices.Count;
        vertices.Add(new Vector3(-0.5f, 0f, 0f));
        int endCenter = vertices.Count;
        vertices.Add(new Vector3(0.5f, 0f, 0f));
        for (int j = 0; j < around; j++)
        {
            int next = (j + 1) % around;
            triangles.Add(startCenter); triangles.Add(next); triangles.Add(j);
            int lastRing = along * around;
            triangles.Add(endCenter);
            triangles.Add(lastRing + j);
            triangles.Add(lastRing + next);
        }

        generatedMesh = new Mesh { name = "Generated Fibrous Adhesion Visual" };
        generatedMesh.SetVertices(vertices);
        generatedMesh.SetTriangles(triangles, 0);
        generatedMesh.RecalculateNormals();
        generatedMesh.RecalculateBounds();
        filter.sharedMesh = generatedMesh;
    }
}
