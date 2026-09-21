using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class IntestineLoopGenerator : MonoBehaviour
{
    [Header("Intestine Shape")]

    public float tubeRadius = 0.23f;

    public float loopWidth = 0.9f;

    public float straightHeight = 2.4f;


    [Header("Mesh Quality")]

    [Range(4, 40)]
    public int curveSegments = 20;

    [Range(4, 24)]
    public int radialSegments = 12;


    private Mesh generatedMesh;


    private void OnEnable()
    {
        GenerateLoop();
    }


    [ContextMenu("Generate Loop")]
    public void GenerateLoop()
    {
        MeshFilter meshFilter =
            GetComponent<MeshFilter>();

        if (generatedMesh != null)
        {
            if (Application.isPlaying)
                Destroy(generatedMesh);
            else
                DestroyImmediate(generatedMesh);
        }

        generatedMesh = new Mesh();

        generatedMesh.name = "Generated Intestine Loop";


        List<Vector3> path = new List<Vector3>();

        float halfWidth = loopWidth / 2f;

        float topY = 1.45f;

        float bottomY = topY - straightHeight;


        // LEFT SIDE

        for (int i = 0; i <= curveSegments; i++)
        {
            float t = (float)i / curveSegments;

            Vector3 point = new Vector3(
                -halfWidth,
                Mathf.Lerp(topY, bottomY, t),
                0
            );

            path.Add(point);
        }


        // BOTTOM CURVE

        for (int i = 1; i <= curveSegments; i++)
        {
            float t = (float)i / curveSegments;

            float angle =
                Mathf.PI - t * Mathf.PI;

            float x =
                Mathf.Cos(angle) * halfWidth;

            float y =
                bottomY -
                Mathf.Sin(angle) * halfWidth;

            path.Add(new Vector3(x, y, 0));
        }


        // RIGHT SIDE

        for (int i = 1; i <= curveSegments; i++)
        {
            float t = (float)i / curveSegments;

            Vector3 point = new Vector3(
                halfWidth,
                Mathf.Lerp(bottomY, topY, t),
                0
            );

            path.Add(point);
        }


        List<Vector3> vertices =
            new List<Vector3>();

        List<int> triangles =
            new List<int>();


        // CREATE TUBE

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 tangent;

            if (i == 0)
            {
                tangent =
                    path[i + 1] - path[i];
            }
            else if (i == path.Count - 1)
            {
                tangent =
                    path[i] - path[i - 1];
            }
            else
            {
                tangent =
                    path[i + 1] - path[i - 1];
            }

            tangent.Normalize();


            Vector3 normal =
                new Vector3(
                    -tangent.y,
                    tangent.x,
                    0
                );


            for (int j = 0; j < radialSegments; j++)
            {
                float angle =
                    (float)j / radialSegments *
                    Mathf.PI * 2f;

                Vector3 offset =
                    normal * Mathf.Cos(angle) +
                    Vector3.forward * Mathf.Sin(angle);

                vertices.Add(
                    path[i] + offset * tubeRadius
                );
            }
        }


        // CREATE TRIANGLES

        for (int i = 0; i < path.Count - 1; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int nextJ =
                    (j + 1) % radialSegments;

                int a =
                    i * radialSegments + j;

                int b =
                    (i + 1) * radialSegments + j;

                int c =
                    i * radialSegments + nextJ;

                int d =
                    (i + 1) * radialSegments + nextJ;


                triangles.Add(a);
                triangles.Add(c);
                triangles.Add(b);

                triangles.Add(c);
                triangles.Add(d);
                triangles.Add(b);
            }
        }


        generatedMesh.SetVertices(vertices);

        generatedMesh.SetTriangles(
            triangles, 0
        );

        generatedMesh.RecalculateNormals();

        generatedMesh.RecalculateBounds();

        meshFilter.sharedMesh = generatedMesh;
    }
}