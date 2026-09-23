using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a mildly asymmetric visual bowel loop.
/// It does not change the parent intestine's grasping, colliders or adhesion points.
/// Keep IntestineLoopGenerator on the original XR prototype.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AnatomicalIntestineGenerator : MonoBehaviour
{
    [Header("Basic Shape")]
    [Min(0.01f)] public float tubeRadius = 0.23f;
    [Min(0.1f)] public float loopWidth = 0.9f;
    [Min(0.1f)] public float straightHeight = 2.4f;

    [Header("Natural Variation")]
    [Range(-0.25f, 0.25f)] public float leftBend = -0.06f;
    [Range(-0.25f, 0.25f)] public float rightBend = 0.08f;
    [Range(-0.4f, 0.4f)] public float rightTopHeightOffset = -0.12f;
    [Range(-0.25f, 0.25f)] public float bottomSideOffset = 0.04f;
    [Range(-0.25f, 0.25f)] public float depthVariation = 0.06f;

    [Header("Mesh Quality")]
    [Range(6, 40)] public int curveSegments = 24;
    [Range(6, 24)] public int radialSegments = 14;

    [System.NonSerialized] private Mesh generatedMesh;

    private void OnEnable() => GenerateLoop();

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
            GenerateLoop();
    }
#endif

    [ContextMenu("Generate Anatomical Loop")]
    public void GenerateLoop()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter == null) return;

        if (generatedMesh != null)
        {
            if (Application.isPlaying) Destroy(generatedMesh);
            else DestroyImmediate(generatedMesh);
        }

        int steps = Mathf.Max(6, curveSegments);
        int sides = Mathf.Max(6, radialSegments);
        float halfWidth = Mathf.Max(0.1f, loopWidth) * 0.5f;
        float top = 1.45f; // Matches the old generator's reference position.
        float bottom = top - Mathf.Max(0.1f, straightHeight);
        List<Vector3> path = new List<Vector3>(steps * 3 + 1);

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            float wave = Mathf.Pow(Mathf.Sin(Mathf.PI * t), 2f);
            path.Add(new Vector3(-halfWidth + leftBend * wave,
                Mathf.Lerp(top, bottom, t), depthVariation * wave));
        }

        for (int i = 1; i <= steps; i++)
        {
            float t = (float)i / steps;
            float angle = Mathf.PI * (1f - t);
            float wave = Mathf.Pow(Mathf.Sin(Mathf.PI * t), 2f);
            path.Add(new Vector3(halfWidth * Mathf.Cos(angle) + bottomSideOffset * wave,
                bottom - halfWidth * Mathf.Sin(angle), depthVariation * wave));
        }

        for (int i = 1; i <= steps; i++)
        {
            float t = (float)i / steps;
            float wave = Mathf.Pow(Mathf.Sin(Mathf.PI * t), 2f);
            path.Add(new Vector3(halfWidth + rightBend * wave,
                Mathf.Lerp(bottom, top + rightTopHeightOffset, t),
                depthVariation * wave));
        }

        List<Vector3> vertices = new List<Vector3>(path.Count * sides);
        List<Vector2> uvs = new List<Vector2>(path.Count * sides);
        List<int> triangles = new List<int>((path.Count - 1) * sides * 6);
        float length = 0f;

        for (int i = 0; i < path.Count; i++)
        {
            if (i > 0) length += Vector3.Distance(path[i - 1], path[i]);
            Vector3 tangent = i == 0 ? path[1] - path[0]
                : i == path.Count - 1 ? path[i] - path[i - 1]
                : path[i + 1] - path[i - 1];
            tangent.Normalize();
            Vector3 normal = new Vector3(-tangent.y, tangent.x, 0f).normalized;
            if (normal.sqrMagnitude < 0.0001f) normal = Vector3.right;
            Vector3 binormal = Vector3.Cross(tangent, normal).normalized;
            for (int j = 0; j < sides; j++)
            {
                float angle = j * Mathf.PI * 2f / sides;
                Vector3 offset = normal * Mathf.Cos(angle) + binormal * Mathf.Sin(angle);
                vertices.Add(path[i] + offset * tubeRadius);
                uvs.Add(new Vector2((float)j / sides, length));
            }
        }

        for (int i = 0; i < path.Count - 1; i++)
        {
            for (int j = 0; j < sides; j++)
            {
                int next = (j + 1) % sides;
                int a = i * sides + j;
                int b = (i + 1) * sides + j;
                int c = i * sides + next;
                int d = (i + 1) * sides + next;
                triangles.Add(a); triangles.Add(c); triangles.Add(b);
                triangles.Add(c); triangles.Add(d); triangles.Add(b);
            }
        }

        generatedMesh = new Mesh { name = "Generated Anatomical Intestine Loop" };
        generatedMesh.SetVertices(vertices);
        generatedMesh.SetUVs(0, uvs);
        generatedMesh.SetTriangles(triangles, 0);
        generatedMesh.RecalculateNormals();
        generatedMesh.RecalculateBounds();
        filter.sharedMesh = generatedMesh;
    }
}
