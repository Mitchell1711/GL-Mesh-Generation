using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GenerateMesh : MonoBehaviour
{
    [Range(1, 127)]
    public int chunkSize;
    public int blockSize;
    private Mesh mesh;
    private Vector3 vertices;

    public float sampleRate;
    public int heightVariation;
    private readonly int offsetDepth = 100000;

    void Start()
    {
        BuildChunk();
    }

    private void BuildChunk()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Voxel Grid";

        List<Vector3> vertices = new List<Vector3>();

        //random offsets for the perlin noise function to get a new result every time
        float randOffsetX = Random.Range(0, offsetDepth);
        float randOffsetY = Random.Range(0, offsetDepth);

        //go over each position in the chunk
        for (int xPos = 0; xPos < chunkSize; xPos++)
        {
            for(int yPos = 0; yPos < chunkSize; yPos++)
            {
                //fill up the heightmap
                int height = (int)(Mathf.PerlinNoise((xPos + randOffsetX) * sampleRate, (yPos + randOffsetY) * sampleRate) 
                    * heightVariation - (heightVariation / 2));

                //create vertices for each voxel
                for (int i = 0; i < 4; i++)
                {
                    vertices.Add(BuildVertices(new Vector2Int(xPos * blockSize, yPos * blockSize), height)[i] * blockSize);
                }
            }
        }

        List<int> triangles = new List<int>();

        //im not sure why exactly this is needed i just figured the formula out by bruteforcing it
        int sideMultiplier = 4 - chunkSize;

        //generate tris
        for (int vi = 0; vi < vertices.Count - 2; vi += 2)
        {
            //top tris
            if (vi % 4 == 0)
            {
                triangles.Add(vi);
                triangles.Add(vi + 2);
                triangles.Add(vi + 1);
                triangles.Add(vi);
                triangles.Add(vi + 3);
                triangles.Add(vi + 2);

                //side tris
                if (vi + chunkSize * chunkSize + 3 < vertices.Count - (chunkSize * sideMultiplier))
                {
                    triangles.Add(vi + 1);
                    triangles.Add(vi + 2);
                    triangles.Add(vi + chunkSize * chunkSize + (chunkSize * sideMultiplier));
                    triangles.Add(vi + 2);
                    triangles.Add(vi + chunkSize * chunkSize + (chunkSize * sideMultiplier) + 3);
                    triangles.Add(vi + chunkSize * chunkSize + (chunkSize * sideMultiplier));
                }
            }
            //front/back tris
            else if(Mathf.Abs(vertices[vi].z - vertices[vi + 2].z) == 0)
            {
                triangles.Add(vi);
                triangles.Add(vi + 1);
                triangles.Add(vi + 2);
                triangles.Add(vi);
                triangles.Add(vi + 2);
                triangles.Add(vi + 3);
            }
        }

        //load voxels vertices and tris
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        print("Triangle count: "+triangles.Count+", "+mesh.triangles.Length);
    }

    private Vector3[] BuildVertices(Vector2Int pos, int height)
    {
        return new Vector3[]
        {
        new Vector3Int(pos.x, height, pos.y),
        new Vector3Int(pos.x + blockSize, height, pos.y),
        new Vector3Int(pos.x + blockSize, height, pos.y + blockSize),
        new Vector3Int(pos.x, height, pos.y + blockSize),
        };
    }

    //private void OnDrawGizmos()
    //{
    //    if (mesh == null)
    //    {
    //        return;
    //    }
    //    Gizmos.color = Color.black;
    //    for (int i = 0; i < mesh.vertices.Length; i++)
    //    {
    //        if (i != 0)
    //        {
    //            if (mesh.vertices[i] != mesh.vertices[i - 1])
    //            {
    //                Gizmos.DrawSphere(mesh.vertices[i], 0.1f);
    //            }
    //        }
    //        else
    //        {
    //            Gizmos.DrawSphere(mesh.vertices[i], 0.1f);
    //        }
    //    }
    //}
}