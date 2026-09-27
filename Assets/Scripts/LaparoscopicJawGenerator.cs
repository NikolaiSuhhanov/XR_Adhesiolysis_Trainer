using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
public class LaparoscopicJawGenerator : MonoBehaviour
{
    public enum JawSide
    {
        Left,
        Right
    }

    [Header("Jaw")]
    public JawSide side = JawSide.Left;

    [Tooltip("Width at the hinge end, in normalized local mesh units.")]
    [Range(0.4f, 1.4f)]
    public float baseWidth = 1.0f;

    [Tooltip("Width at the distal tip, in normalized local mesh units.")]
    [Range(0.15f, 1.0f)]
    public float tipWidth = 0.48f;

    [Tooltip("Thickness of the jaw, in normalized local mesh units.")]
    [Range(0.2f, 1.2f)]
    public float thickness = 0.75f;

    [Tooltip("How much the distal half bends toward the opposing jaw.")]
    [Range(0f, 0.35f)]
    public float inwardCurve = 0.12f;

    [Tooltip("Softens the very distal end by shrinking the last cross-sections.")]
    [Range(0f, 0.45f)]
    public float tipRoundness = 0.18f;

    [Header("Mesh Quality")]
    [Range(4, 24)]
    public int lengthSegments = 10;

    [Range(6, 20)]
    public int radialSegments = 10;

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

    [ContextMenu("Generate Jaw")]
    public void Generate()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter == null)
            return;

        lengthSegments = Mathf.Max(2, lengthSegments);
        radialSegments = Mathf.Max(6, radialSegments);

        int rings = lengthSegments + 1;
        int vertsPerRing = radialSegments;
        int vertexCount = rings * vertsPerRing + 2;
        int triangleCount = lengthSegments * radialSegments * 2 + radialSegments * 2;

        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[triangleCount * 3];

        float inwardSign = side == JawSide.Left ? 1f : -1f;

        for (int i = 0; i < rings; i++)
        {
            float t = (float)i / lengthSegments;

            // Jaw runs from -0.5 (hinge) to +0.5 (tip), matching the old cube bounds.
            float z = Mathf.Lerp(-0.5f, 0.5f, t);

            float smooth = t * t * (3f - 2f * t);
            float width = Mathf.Lerp(baseWidth, tipWidth, smooth);
            float localThickness = thickness * Mathf.Lerp(1f, 0.82f, smooth);

            // Round off only the last part of the distal tip.
            if (tipRoundness > 0f)
            {
                float roundStart = 1f - tipRoundness;
                if (t > roundStart)
                {
                    float rt = Mathf.InverseLerp(roundStart, 1f, t);
                    float shrink = Mathf.Lerp(1f, 0.38f, rt * rt);
                    width *= shrink;
                    localThickness *= shrink;
                }
            }

            // Distal bending toward the opposing jaw.
            float xCenter = inwardSign * inwardCurve * smooth * smooth;

            for (int j = 0; j < radialSegments; j++)
            {
                float angle = (float)j / radialSegments * Mathf.PI * 2f;

                // Flattened elliptical cross-section: wider than it is thick.
                float x = Mathf.Cos(angle) * width * 0.5f;
                float y = Mathf.Sin(angle) * localThickness * 0.5f;

                int index = i * vertsPerRing + j;
                vertices[index] = new Vector3(xCenter + x, y, z);

                Vector3 n = new Vector3(
                    Mathf.Cos(angle) / Mathf.Max(0.0001f, width),
                    Mathf.Sin(angle) / Mathf.Max(0.0001f, localThickness),
                    0f
                ).normalized;

                normals[index] = n;
                uvs[index] = new Vector2((float)j / radialSegments, t);
            }
        }

        int tri = 0;

        // Side surface.
        for (int i = 0; i < lengthSegments; i++)
        {
            int ringA = i * vertsPerRing;
            int ringB = (i + 1) * vertsPerRing;

            for (int j = 0; j < radialSegments; j++)
            {
                int next = (j + 1) % radialSegments;

                int a = ringA + j;
                int b = ringA + next;
                int c = ringB + j;
                int d = ringB + next;

                triangles[tri++] = a;
                triangles[tri++] = c;
                triangles[tri++] = b;

                triangles[tri++] = b;
                triangles[tri++] = c;
                triangles[tri++] = d;
            }
        }

        // End caps.
        int baseCenter = rings * vertsPerRing;
        int tipCenter = baseCenter + 1;

        float baseX = side == JawSide.Left ? 0f : 0f;
        float tipSmooth = 1f;
        float tipX = inwardSign * inwardCurve * tipSmooth * tipSmooth;

        vertices[baseCenter] = new Vector3(baseX, 0f, -0.5f);
        normals[baseCenter] = Vector3.back;
        uvs[baseCenter] = new Vector2(0.5f, 0f);

        vertices[tipCenter] = new Vector3(tipX, 0f, 0.5f);
        normals[tipCenter] = Vector3.forward;
        uvs[tipCenter] = new Vector2(0.5f, 1f);

        int lastRing = lengthSegments * vertsPerRing;

        for (int j = 0; j < radialSegments; j++)
        {
            int next = (j + 1) % radialSegments;

            triangles[tri++] = baseCenter;
            triangles[tri++] = next;
            triangles[tri++] = j;

            triangles[tri++] = tipCenter;
            triangles[tri++] = lastRing + j;
            triangles[tri++] = lastRing + next;
        }

        if (generatedMesh != null)
        {
            if (Application.isPlaying)
                Destroy(generatedMesh);
            else
                DestroyImmediate(generatedMesh);
        }

        generatedMesh = new Mesh
        {
            name = "Generated Laparoscopic Jaw"
        };

        generatedMesh.vertices = vertices;
        generatedMesh.normals = normals;
        generatedMesh.uv = uvs;
        generatedMesh.triangles = triangles;
        generatedMesh.RecalculateBounds();
        generatedMesh.RecalculateTangents();

        filter.sharedMesh = generatedMesh;
    }
}
