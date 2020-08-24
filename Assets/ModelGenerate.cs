using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine;
using System;
using System.Collections.Specialized;

public class ModelGenerate : MonoBehaviour
{
    const string filename = "Assets/demo_files/demo.data";
    public GameObject lineDrawPrefabs;
    public GameObject lineDrawPrefab;
    Color c1 = Color.black;
    Color c2 = Color.black;

    void start()
    {
        lineDrawPrefabs = gameObject;
    }

    // Start is called before the first frame update
    public void GenerateModel()
    {
        if (File.Exists(filename))
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                int c = 0;
                int n_strand = reader.ReadInt32();
                //Vector3[][] strands = new Vector3[][n_strand];
                c++;
                
                int n_vertex = 0; // 1024
                float x=0.0f, y=0.0f, z=0.0f;
                float cx = 1.689f, cy = 0.719f, cz = 0.099f;
                for (int i = 0; i < n_strand; i++)
                {
                    n_vertex = reader.ReadInt32();
                    Vector3[] vertices = new Vector3[n_vertex];

                    c++;
                    for (int j = 0; j < n_vertex; j++)
                    {
                        x = reader.ReadSingle();
                        y = reader.ReadSingle();
                        z = reader.ReadSingle();
                        vertices[j] = new Vector3(x + cx, y +cy, z + cz);
                        c += 3;
                    }
                    DrawStrand(vertices, n_vertex);
                }
            }
        }
    }

    void DrawStrand(Vector3[] vertexPositions, int n_vertex)
    {
        lineDrawPrefab = GameObject.Instantiate(lineDrawPrefabs) as GameObject;
        LineRenderer lineRenderer = lineDrawPrefab.GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.SetColors(c1, c2);

        lineRenderer.positionCount = n_vertex;
        lineRenderer.SetPositions(vertexPositions);


    }


    // Update is called once per frame
    void Update()
    {
        
    }
};
