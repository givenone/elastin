using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Specialized;

public class HairManager : MonoBehaviour
{
    public GameObject hair;
    public GameObject cache;
    public LineRenderer lineRenderer;
    int n_rendered_strand;
    List<LineRenderer> lines = new List<LineRenderer>();
    public Slider slider1, slider2;
    float length_scale = 2.0f;
    List<int> visibilities = new List<int>();
    Color color = Color.white;
    int hair_model_index = 0;
    string[] hair_models = {"curly", "short", "garma", "dandy", "long", "rocker"};
    

    // Start is called before the first frame update
    public void GenerateModel(string filename)
    {
        if (File.Exists(filename))
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                int n_strand = reader.ReadInt32();
                n_rendered_strand = 0;
                //Vector3[][] strands = new Vector3[][n_strand];
                int n_exclude_vertex = 0;
                int n_vertex = 0; // 1024
                int n_rendered_vertex = 0;
                int skip_factor=2;
                float x = 0.0f, y = 0.0f, z = 0.0f;
                float cx = 0f, cy = 0.0f, cz = 0.00f; // default camera center 
                float hx = 0.005f, hy = 1.75f, hz = 0.01f; // default scalp center
                for (int i = 0; i < n_strand; i++)
                {
                    n_vertex = reader.ReadInt32();
                    n_rendered_vertex = n_vertex-n_exclude_vertex;
                    if (n_rendered_vertex >= 0){
                        Vector3[] vertices = new Vector3[n_rendered_vertex/skip_factor];

                        int c = 0;
                        for (int j = 0; j < n_rendered_vertex; j++)
                        {
                            x = reader.ReadSingle();
                            y = reader.ReadSingle();
                            z = reader.ReadSingle();
                            if(j % skip_factor == 0 && c < n_rendered_vertex/skip_factor){
                                vertices[c] = new Vector3(x + cx -hx, y + cy -hy, z  + cz -hz);
                                c++;
                            }
                        }
                        for (int j = n_rendered_vertex; j < n_vertex; j++)
                        {
                            x = reader.ReadSingle();
                            y = reader.ReadSingle();
                            z = reader.ReadSingle();
                        }
                        if (i % 2 == 0) {
                            DrawAndSaveStrand(vertices, n_rendered_vertex/skip_factor);
                        }
                    }
                    else{
                        for (int j = 0; j < n_vertex; j++)
                        {
                            x = reader.ReadSingle();
                            y = reader.ReadSingle();
                            z = reader.ReadSingle();
                        }
                    }
                }
                Debug.Log("strands drawn");
            }
        }
        else{
            Debug.Log("file not found");
        }
    }

    void DrawAndSaveStrand(Vector3[] vertexPositions, int n_vertex)
    {
        n_rendered_strand++;
        GameObject Hair = GameObject.Instantiate(hair);
        lineRenderer = Hair.GetComponent<LineRenderer>();
        lineRenderer.transform.SetParent(transform);
        lineRenderer.SetWidth(0.002f, 0.003f);

        lineRenderer.positionCount = n_vertex;
        lineRenderer.SetPositions(vertexPositions);
        //add to memory
        LineRenderer copy = Instantiate(lineRenderer);
        copy.transform.SetParent(cache.transform);
        copy.enabled= false;
        lines.Add(copy);
        visibilities.Add(n_vertex);
        if (color == Color.white) {
            Color tmp = lineRenderer.material.GetColor("_BaseColor");
            color = new Color(tmp.r, tmp.g, tmp.b);
            Debug.Log("Color saved");
        };
    }
    void TrimHair(int GRANULARITY=3){
        for (int i=0;i<n_rendered_strand;i++)
            {
                Transform child = this.transform.GetChild(i);
                LineRenderer line = child.gameObject.GetComponent<LineRenderer>();
                int n_rendered_vertex = visibilities[i];
                int n_vertex = lines[i].positionCount;
                if (n_rendered_vertex-GRANULARITY > 0){
                    for(int j=n_rendered_vertex-GRANULARITY;j<n_vertex;j++){
                        line.SetPosition(j, line.GetPosition(n_rendered_vertex-GRANULARITY-1));
                    }
                    visibilities[i] -= GRANULARITY;
                }
            }
            Debug.Log("trim hair");
    }
    void GrowHair(int GRANULARITY=3){
        for (int i=0;i<n_rendered_strand;i++)
            {
                Transform child = this.transform.GetChild(i);
                LineRenderer line = child.gameObject.GetComponent<LineRenderer>();
                int n_rendered_vertex = visibilities[i];
                int n_vertex = lines[i].positionCount;
                if (n_rendered_vertex+GRANULARITY <= n_vertex){
                    for(int j=n_rendered_vertex;j<n_rendered_vertex+GRANULARITY;j++){
                        line.SetPosition(j, lines[i].GetPosition(j));
                    }
                    for(int j=n_rendered_vertex+GRANULARITY;j<n_vertex;j++){
                        line.SetPosition(j, lines[i].GetPosition(n_rendered_vertex+GRANULARITY));
                    }
                    visibilities[i] += GRANULARITY;
                }
            }
            Debug.Log("grow hair");
    }
    public void ChangeLength(){ // from slider 1
        float granularity = (slider1.value - length_scale)*25;
        if(granularity<=-1 || granularity>=1){

            if (slider1.value < length_scale){
                TrimHair(-(int)granularity);
            }
            else if (slider1.value > length_scale){
                GrowHair((int)granularity);
            }
            length_scale = slider1.value;
        }
    }
    void ClearHair(){
        for (int i=0;i<n_rendered_strand;i++)
            {
                Transform child = this.transform.GetChild(i);
                LineRenderer line = child.gameObject.GetComponent<LineRenderer>();
                int n_rendered_vertex = visibilities[i];
                int n_vertex = lines[i].positionCount;
                for(int j=0;j<n_vertex;j++){
                    line.SetPosition(j, line.GetPosition(0));
                }
                visibilities[i] = 1;
            }
            Debug.Log("clear hair");
    }
    void ResetHair(){
        for (int i=0;i<n_rendered_strand;i++)
            {
                Transform child = this.transform.GetChild(i);
                LineRenderer line = child.gameObject.GetComponent<LineRenderer>();
                int n_vertex = lines[i].positionCount;
                for(int j=0;j<n_vertex;j++){
                    line.SetPosition(j, lines[i].GetPosition(j));
                }
                line.material.SetColor("_BaseColor", color);
                visibilities[i] = n_vertex;
            }
            Debug.Log("reset hair");
    }
    public void DyeHair(Color new_BaseColor){
        for (int i=0;i<n_rendered_strand;i++)
            {
                Transform child = this.transform.GetChild(i);
                child.gameObject.GetComponent<LineRenderer>().material.SetColor("_BaseColor", new_BaseColor);
            }
    }
    void Start(){
        string filename ="Assets/demo_files/hair_models/"+ hair_models[hair_model_index] +".data";
        GenerateModel(filename);
    }
    void NewHair(){
        //reset saved object
        lines = new List<LineRenderer>();
        visibilities = new List<int>();
        color = Color.white;
        for (int i=0;i<n_rendered_strand;i++)
        {
            Destroy(this.transform.GetChild(i).gameObject);
            Destroy(cache.transform.GetChild(i).gameObject);
        }
        //load new model
        hair_model_index = (hair_model_index + 1) % hair_models.Length;
        string filename ="Assets/demo_files/hair_models/"+ hair_models[hair_model_index] +".data";
        GenerateModel(filename);
        Debug.Log("load model");
    }
    void Update(){
        if (Input.GetKey (KeyCode.H)){ //new hair
            NewHair();
        }
        if (Input.GetKey (KeyCode.R)){ //reset
            ResetHair();
        }
        if (Input.GetKey (KeyCode.C)){ //clear
            ClearHair();
        }
        if (Input.GetKey (KeyCode.T)){ //trimmed
            TrimHair();
        }
        if (Input.GetKey (KeyCode.G)){ //grow
            GrowHair();
        }
        if (Input.GetKey (KeyCode.Y)){ //dyed
            DyeHair(UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
        }
    }
}
