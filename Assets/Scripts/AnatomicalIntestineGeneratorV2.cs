using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Anatomy-stage V2: visual-only, mildly irregular loops of small bowel.
/// Does not modify parent colliders, adhesion points, grasping or XR controls.
/// The original IntestineLoopGenerator and AnatomicalIntestineGenerator stay intact.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AnatomicalIntestineGeneratorV2 : MonoBehaviour
{
    public enum LoopVariant { Left, Right }

    [Header("Loop")]
    public LoopVariant variant = LoopVariant.Left;
    [Min(0.01f)] public float tubeRadius = 0.23f;
    [Min(0.1f)] public float loopWidth = 0.9f;
    [Min(0.1f)] public float straightHeight = 2.4f;

    [Header("Anatomical Variation")]
    [Range(0f, 1.5f)] public float bendStrength = 1f;
    [Range(0f, 1.5f)] public float depthStrength = 1f;
    [Range(0f, 0.08f)] public float radiusVariation = 0.035f;

    [Header("Mesh Quality")]
    [Range(10, 48)] public int curveSegments = 32;
    [Range(8, 24)] public int radialSegments = 16;

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

    [ContextMenu("Generate Anatomical Loop V2")]
    public void GenerateLoop()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter == null) return;

        if (generatedMesh != null)
        {
            if (Application.isPlaying) Destroy(generatedMesh);
            else DestroyImmediate(generatedMesh);
            generatedMesh = null;
        }

        int steps = Mathf.Max(10, curveSegments);
        int sides = Mathf.Max(8, radialSegments);
        float half = Mathf.Max(0.1f, loopWidth) * 0.5f;
        float top = 1.45f; // Retain original local reference height.
        float bottom = top - Mathf.Max(0.1f, straightHeight);
        float bend = bendStrength;
        float depth = depthStrength;
        bool left = variant == LoopVariant.Left;

        // Preset differences remain small enough to keep the existing
        // attachment and grasping layout recognizable during the first test.
        float leftBend = (left ? -0.19f : 0.13f) * bend;
        float rightBend = (left ? -0.11f : 0.20f) * bend;
        float upperRightOffset = (left ? -0.20f : 0.12f) * bend;
        float turnSkew = (left ? 0.13f : -0.12f) * bend;
        float leftDepth = (left ? 0.12f : -0.08f) * depth;
        float rightDepth = (left ? -0.08f : 0.12f) * depth;
        float turnDepth = (left ? 0.16f : -0.14f) * depth;

        List<Vector3> path = new List<Vector3>(steps * 3 + 1);

        // Descending limb: curved in both the frontal plane and depth.
        // sin^2 is zero with zero slope at both ends, helping the pieces join.
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            float wave = Mathf.Pow(Mathf.Sin(Mathf.PI * t), 2f);
            float detail = Mathf.Pow(Mathf.Sin(2f * Mathf.PI * t), 2f);
            path.Add(new Vector3(
                -half + leftBend * wave + (left ? 0.035f : -0.025f) * bend * detail,
                Mathf.Lerp(top, bottom, t),
                leftDepth * wave
            ));
        }

        // Rounded turn: asymmetry and modest fore/aft displacement.
        for (int i = 1; i <= steps; i++)
        {
            float t = (float)i / steps;
            float angle = Mathf.PI * (1f - t);
            float wave = Mathf.Pow(Mathf.Sin(Mathf.PI * t), 2f);
            path.Add(new Vector3(
                half * Mathf.Cos(angle) + turnSkew * wave,
                bottom - half * Mathf.Sin(angle) - 0.04f * bend * wave,
                turnDepth * wave
            ));
        }

        // Ascending limb with a different upper height and depth curvature.
        for (int i = 1; i <= steps; i++)
        {
            float t = (float)i / steps;
            float wave = Mathf.Pow(Mathf.Sin(Mathf.PI * t), 2f);
            float detail = Mathf.Pow(Mathf.Sin(2f * Mathf.PI * t), 2f);
            path.Add(new Vector3(
                half + rightBend * wave + (left ? -0.025f : 0.03f) * bend * detail,
                Mathf.Lerp(bottom, top + upperRightOffset, t),
                rightDepth * wave
            ));
        }

        List<Vector3> vertices = new List<Vector3>(path.Count * sides);
        List<Vector2> uvs = new List<Vector2>(path.Count * sides);
        List<int> triangles = new List<int>((path.Count - 1) * sides * 6);
        float traveled = 0f;

        for (int i = 0; i < path.Count; i++)
        {
            if (i > 0) traveled += Vector3.Distance(path[i - 1], path[i]);

            Vector3 tangent = i == 0 ? path[1] - path[0]
                : i == path.Count - 1 ? path[i] - path[i - 1]
                : path[i + 1] - path[i - 1];
            tangent.Normalize();
            Vector3 normal = new Vector3(-tangent.y, tangent.x, 0f).normalized;
            if (normal.sqrMagnitude < 0.0001f) normal = Vector3.right;
            Vector3 binormal = Vector3.Cross(tangent, normal).normalized;

            float tPath = (float)i / (path.Count - 1);
            float localRadius = Mathf.Max(0.01f,
                tubeRadius * (1f + radiusVariation * Mathf.Sin(tPath * 5f * Mathf.PI)));

            for (int j = 0; j < sides; j++)
            {
                float fraction = (float)j / sides;
                float angle = fraction * Mathf.PI * 2f;
                Vector3 offset = normal * Mathf.Cos(angle)
                    + binormal * Mathf.Sin(angle);
                vertices.Add(path[i] + offset * localRadius);
                uvs.Add(new Vector2(fraction, traveled));
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

        generatedMesh = new Mesh { name = "Generated Anatomical Intestine Loop V2" };
        generatedMesh.SetVertices(vertices);
        generatedMesh.SetUVs(0, uvs);
        generatedMesh.SetTriangles(triangles, 0);
        generatedMesh.RecalculateNormals();
        generatedMesh.RecalculateBounds();
        filter.sharedMesh = generatedMesh;
    }
}
