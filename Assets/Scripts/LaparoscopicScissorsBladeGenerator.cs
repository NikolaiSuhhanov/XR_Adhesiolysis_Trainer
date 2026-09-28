using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
public class LaparoscopicScissorsBladeGenerator : MonoBehaviour
{
    public enum BladeSide
    {
        Left,
        Right
    }

    [Header("Blade")]
    public BladeSide side = BladeSide.Left;

    [Tooltip("Width at the hinge end, in normalized local mesh units.")]
    [Range(0.2f, 1.2f)]
    public float baseWidth = 0.75f;

    [Tooltip("Width near the distal tip.")]
    [Range(0.05f, 0.8f)]
    public float tipWidth = 0.16f;

    [Tooltip("Blade thickness.")]
    [Range(0.08f, 0.6f)]
    public float thickness = 0.22f;

    [Tooltip("Gentle lateral curvature of the distal blade.")]
    [Range(-0.35f, 0.35f)]
    public float lateralCurve = 0.10f;

    [Tooltip("How strongly the distal tip narrows and rounds off.")]
    [Range(0f, 0.45f)]
    public float tipRoundness = 0.22f;

    [Tooltip("Makes the inner cutting edge slightly more pronounced.")]
    [Range(0f, 0.25f)]
    public float cuttingEdgeBias = 0.08f;

    [Header("Mesh Quality")]
    [Range(6, 40)]
    public int lengthSegments = 24;

    private Mesh generatedMesh;

    private void OnEnable()
    {
        Generate();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            Generate();
    }
#endif

    [ContextMenu("Generate Scissors Blade")]
    public void Generate()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter == null)
            return;

        lengthSegments = Mathf.Max(4, lengthSegments);

        // Four vertices per longitudinal section:
        // outer/top, inner/top, outer/bottom, inner/bottom.
        int sections = lengthSegments + 1;
        int verticesPerSection = 4;
        int vertexCount = sections * verticesPerSection;
        int quadCountPerStrip = lengthSegments;
        int sideQuads = quadCountPerStrip * 4;
        int capQuads = 2;
        int triangleCount = (sideQuads + capQuads) * 2;

        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[triangleCount * 3];

        float innerSign = side == BladeSide.Left ? 1f : -1f;
        float curveSign = side == BladeSide.Left ? 1f : -1f;

        for (int i = 0; i < sections; i++)
        {
            float t = (float)i / lengthSegments;
            float z = Mathf.Lerp(-0.5f, 0.5f, t);

            float smooth = t * t * (3f - 2f * t);
            float width = Mathf.Lerp(baseWidth, tipWidth, smooth);
            float localThickness = thickness * Mathf.Lerp(1f, 0.7f, smooth);

            if (tipRoundness > 0f)
            {
                float roundStart = 1f - tipRoundness;
                if (t > roundStart)
                {
                    float rt = Mathf.InverseLerp(roundStart, 1f, t);
                    float shrink = Mathf.Lerp(1f, 0.20f, rt * rt);
                    width *= shrink;
                    localThickness *= Mathf.Lerp(1f, 0.45f, rt * rt);
                }
            }

            // Gentle curved-shear profile.
            float centerX = curveSign * lateralCurve * smooth * smooth;

            float outerX = centerX - innerSign * width * 0.5f;
            float innerX = centerX + innerSign * width * 0.5f;

            // A subtle bevel toward the inner cutting edge.
            float edgeLift = cuttingEdgeBias * width;

            int v = i * verticesPerSection;

            vertices[v + 0] = new Vector3(outerX,  localThickness * 0.5f, z);
            vertices[v + 1] = new Vector3(innerX,  localThickness * 0.5f - edgeLift, z);
            vertices[v + 2] = new Vector3(outerX, -localThickness * 0.5f, z);
            vertices[v + 3] = new Vector3(innerX, -localThickness * 0.5f + edgeLift, z);

            uvs[v + 0] = new Vector2(0f, t);
            uvs[v + 1] = new Vector2(1f, t);
            uvs[v + 2] = new Vector2(0f, t);
            uvs[v + 3] = new Vector2(1f, t);
        }

        int tri = 0;

        for (int i = 0; i < lengthSegments; i++)
        {
            int a = i * verticesPerSection;
            int b = (i + 1) * verticesPerSection;

            // Top
            AddQuad(triangles, ref tri, a + 0, b + 0, a + 1, b + 1);
            // Bottom
            AddQuad(triangles, ref tri, a + 3, b + 3, a + 2, b + 2);
            // Outer edge
            AddQuad(triangles, ref tri, a + 2, b + 2, a + 0, b + 0);
            // Inner cutting edge
            AddQuad(triangles, ref tri, a + 1, b + 1, a + 3, b + 3);
        }

        // Base cap
        AddQuad(triangles, ref tri, 2, 0, 3, 1);

        // Tip cap
        int last = lengthSegments * verticesPerSection;
        AddQuad(triangles, ref tri, last + 0, last + 2, last + 1, last + 3);

        if (generatedMesh != null)
        {
            if (Application.isPlaying)
                Destroy(generatedMesh);
            else
                DestroyImmediate(generatedMesh);
        }

        generatedMesh = new Mesh
        {
            name = "Generated Laparoscopic Scissors Blade"
        };

        generatedMesh.vertices = vertices;
        generatedMesh.uv = uvs;
        generatedMesh.triangles = triangles;
        generatedMesh.RecalculateNormals();
        generatedMesh.RecalculateBounds();
        generatedMesh.RecalculateTangents();

        filter.sharedMesh = generatedMesh;
    }

    private static void AddQuad(
        int[] triangles,
        ref int index,
        int a,
        int b,
        int c,
        int d)
    {
        triangles[index++] = a;
        triangles[index++] = b;
        triangles[index++] = c;

        triangles[index++] = c;
        triangles[index++] = b;
        triangles[index++] = d;
    }
}
