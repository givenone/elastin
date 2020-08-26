using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine;
using System;
using System.Collections.Specialized;

public class HairManager : MonoBehaviour
{
    public GameObject hair;
    public LineRenderer lineRenderer;
    const string filename = "Assets/demo_files/demo.data";
    Color c1 = Color.black;
    Color c2 = Color.black;


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
                float x = 0.0f, y = 0.0f, z = 0.0f;
                float cx = 0f, cy = 0.0f, cz = 0.00f; // default camera center 
                float hx = 0.005f, hy = 1.75f, hz = 0.01f; // default scalp center
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
                        vertices[j] = new Vector3(x + cx -hx, y + cy -hy, z  + cz -hz);
                        c += 3;
                    }
                    DrawStrand(vertices, n_vertex);
                }
            }
        }
        else{
            Debug.Log("file not found");
        }
    }

    void DrawStrand(Vector3[] vertexPositions, int n_vertex)
    {
        GameObject Hair = GameObject.Instantiate(hair);
        lineRenderer = Hair.GetComponent<LineRenderer>();
        lineRenderer.SetWidth(0.003f, 0.003f);

        lineRenderer.positionCount = n_vertex;
        lineRenderer.SetPositions(vertexPositions);
    }

}
