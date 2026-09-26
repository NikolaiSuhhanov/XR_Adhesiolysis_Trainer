using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MesenterySheetGenerator : MonoBehaviour
{
    [Header("Point lines (same count, same order)")]
    public Transform[] bowelEdgePoints;   // Points along bowel attachment
    public Transform[] rootEdgePoints;    // Points along mesenteric root

    [Header("Shape")]
    [Range(1, 20)] public int crossSubdivisions = 6;
    [Range(0f, 0.2f)] public float billow = 0.03f;
    public Vector3 localBillowAxis = new Vector3(0f, 0f, -1f);

    [Header("Behaviour")]
    public bool updateEveryFrame = true;

    private Mesh generatedMesh;

    private void OnEnable()
    {
        Generate();
    }

    private void LateUpdate()
    {
        if (Application.isPlaying && updateEveryFrame)
            Generate();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            Generate();
    }
#endif

    [ContextMenu("Generate Mesentery")]
    public void Generate()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter == null)
            return;

        if (bowelEdgePoints == null || rootEdgePoints == null)
            return;

        if (bowelEdgePoints.Length < 2 || rootEdgePoints.Length < 2)
            return;

        if (bowelEdgePoints.Length != rootEdgePoints.Length)
        {
            Debug.LogWarning("MesenterySheetGenerator: bowelEdgePoints and rootEdgePoints must have the same length.");
            return;
        }

        int rows = bowelEdgePoints.Length;
        int cols = Mathf.Max(1, crossSubdivisions) + 1;

        List<Vector3> vertices = new List<Vector3>(rows * cols);
        List<Vector2> uvs = new List<Vector2>(rows * cols);
        List<int> triangles = new List<int>();

        Vector3 bulgeDir = localBillowAxis.normalized;

        for (int i = 0; i < rows; i++)
        {
            Vector3 rootLocal = transform.InverseTransformPoint(rootEdgePoints[i].position);
            Vector3 bowelLocal = transform.InverseTransformPoint(bowelEdgePoints[i].position);

            for (int j = 0; j < cols; j++)
            {
                float t = (float)j / (cols - 1);
                Vector3 p = Vector3.Lerp(rootLocal, bowelLocal, t);

                float curve = Mathf.Sin(t * Mathf.PI);
                p += bulgeDir * (billow * curve);

                vertices.Add(p);
                uvs.Add(new Vector2((float)i / (rows - 1), t));
            }
        }

        for (int i = 0; i < rows - 1; i++)
        {
            for (int j = 0; j < cols - 1; j++)
            {
                int a = i * cols + j;
                int b = a + 1;
                int c = (i + 1) * cols + j;
                int d = c + 1;

                // Front side
                triangles.Add(a);
                triangles.Add(c);
                triangles.Add(b);

                triangles.Add(b);
                triangles.Add(c);
                triangles.Add(d);

                // Back side
                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(c);

                triangles.Add(b);
                triangles.Add(d);
                triangles.Add(c);
            }
        }

        if (generatedMesh != null)
        {
            if (Application.isPlaying)
                Destroy(generatedMesh);
            else
                DestroyImmediate(generatedMesh);
        }

        generatedMesh = new Mesh();
        generatedMesh.name = "Generated Mesentery";

        generatedMesh.SetVertices(vertices);
        generatedMesh.SetUVs(0, uvs);
        generatedMesh.SetTriangles(triangles, 0);
        generatedMesh.RecalculateNormals();
        generatedMesh.RecalculateBounds();

        filter.sharedMesh = generatedMesh;
    }
}