using System;
using UnityEngine;

namespace YoutubePlayer.Samples.Three60Mesh
{
    // Generates a cubed sphere mesh with normals pointing inwards
    // It will be used to display the 360 video
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class CubedSphereMesh : MonoBehaviour
    {
        [Range(2, 256)]
        public int resolution = 10;

        MeshFilter m_MeshFilter;

        void OnValidate()
        {
            if (m_MeshFilter == null)
            {
                m_MeshFilter = GetComponent<MeshFilter>();
            }

            if (m_MeshFilter.sharedMesh == null)
            {
                m_MeshFilter.sharedMesh = new Mesh();
            }

            GenerateMesh(m_MeshFilter.sharedMesh);
        }

        // Inspired from https://www.youtube.com/@SebastianLague for how to generate the mesh
        void GenerateMesh(Mesh mesh)
        {
            Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

            Vector3[] vertices = new Vector3[resolution * resolution * directions.Length];
            Vector2[] uvs = new Vector2[resolution * resolution * directions.Length];
            int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6 * directions.Length];
            int triIndex = 0;

            for (int d = 0; d < directions.Length; d++)
            {
                Vector3 localUp = directions[d];

                Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
                Vector3 axisB = Vector3.Cross(localUp, axisA);

                for (int y = 0; y < resolution; y++)
                {
                    for (int x = 0; x < resolution; x++)
                    {
                        int i = x + y * resolution + d * resolution * resolution;
                        Vector2 percent = new Vector2(x, y) / (resolution - 1);
                        Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                        Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                        vertices[i] = pointOnUnitSphere;
                        uvs[i] = MapUvs(percent, localUp);

                        if (x != resolution - 1 && y != resolution - 1)
                        {
                            triangles[triIndex] = i;
                            triangles[triIndex + 1] = i + resolution;
                            triangles[triIndex + 2] = i + resolution + 1;

                            triangles[triIndex + 3] = i;
                            triangles[triIndex + 4] = i + resolution + 1;
                            triangles[triIndex + 5] = i + 1;
                            triIndex += 6;
                        }
                    }
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
        }

        Vector2 MapUvs(Vector2 uv, Vector3 localUp)
        {
            if (localUp == Vector3.forward)
            {
                return new Vector2(Mathf.Lerp(2f / 3f, 1f / 3f, uv.y), Mathf.Lerp(1f / 2f, 1f, uv.x));
            }
            else if (localUp == Vector3.left)
            {
                return new Vector2(Mathf.Lerp(1f / 3f, 0f, uv.x), Mathf.Lerp(1f, 1f / 2f, uv.y));
            }
            else if (localUp == Vector3.right)
            {
                return new Vector2(Mathf.Lerp(1f, 2f / 3f, uv.x), Mathf.Lerp(1f, 1f / 2f, uv.y));
            }
            else if (localUp == Vector3.back)
            {
                return new Vector2(Mathf.Lerp(2f / 3f, 1f / 3f, uv.x), Mathf.Lerp(1f / 2f, 0f, uv.y));
            }
            else if (localUp == Vector3.up)
            {
                return new Vector2(Mathf.Lerp(1f, 2f / 3f, uv.y), Mathf.Lerp(0f, 1f / 2f, uv.x));
            }
            else if (localUp == Vector3.down)
            {
                return new Vector2(Mathf.Lerp(0f, 1f / 3f, uv.y), Mathf.Lerp(1f / 2f, 0f, uv.x));
            }
            else
            {
                throw new ArgumentOutOfRangeException("localUp", "Invalid localUp");
            }
        }
    }
}
