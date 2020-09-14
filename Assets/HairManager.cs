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
    public GridManager gridManager;
    public RayInteraction rayInteraction;
    public Slider slider1, slider2;
    
    public int n_rendered_strand;
    List<LineRenderer> original_lines = new List<LineRenderer>();
    float length_scale = 2.0f;
    List<int> visibilities = new List<int>();
    Color original_color = Color.white;
    int hair_model_index = 0;
    string[] hair_models = {"curly", "short", "garma", "dandy", "long", "rocker"};
    float max_x=float.NegativeInfinity, max_y=float.NegativeInfinity, max_z=float.NegativeInfinity;
    float min_x=float.PositiveInfinity, min_y=float.PositiveInfinity, min_z=float.PositiveInfinity;

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
                int strand_count = 0;
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

                        int vertex_count = 0;
                        for (int j = 0; j < n_rendered_vertex; j++)
                        {
                            x = reader.ReadSingle();
                            y = reader.ReadSingle();
                            z = reader.ReadSingle();
                            if(j % skip_factor == 0 && vertex_count < n_rendered_vertex/skip_factor){
                                x = x + cx -hx;
                                y = y + cy -hy;
                                z = z + cz -hz;
                                vertices[vertex_count] = new Vector3(x,y,z);
                                vertex_count++;
                                // update max, min
                                max_x = Math.Max(max_x, x);
                                min_x = Math.Min(min_x, x);
                                max_y = Math.Max(max_y, y);
                                min_y = Math.Min(max_y, y);
                                max_z = Math.Max(max_z, z);
                                min_z = Math.Min(max_z, z);
                            }
                        }
                        for (int j = n_rendered_vertex; j < n_vertex; j++)
                        {
                            x = reader.ReadSingle();
                            y = reader.ReadSingle();
                            z = reader.ReadSingle();
                        }
                        if (i % 2 == 0) {
                            DrawAndSaveStrand(vertices, n_rendered_vertex/skip_factor, strand_count);
                            strand_count ++;
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
                Debug.Log("x " + min_x + '~' + max_x);
                Debug.Log("y " + min_y + '~' + max_y);
                Debug.Log("z " + min_z + '~' + max_z);
                gridManager.GenerateGrid( min_x,  max_x,  min_y,  max_y,  min_z,  max_z);
            }
        }
        else{
            Debug.Log("file not found");
        }
    }

    void DrawAndSaveStrand(Vector3[] vertexPositions, int n_vertex, int vertex_idx)
    {
        n_rendered_strand++;
        GameObject Hair = GameObject.Instantiate(hair);
        Hair.transform.name = "hair" + vertex_idx;
        LineRenderer lineRenderer = Hair.GetComponent<LineRenderer>();
        lineRenderer.transform.SetParent(transform);
        lineRenderer.SetWidth(0.002f, 0.003f);

        lineRenderer.positionCount = n_vertex;
        lineRenderer.SetPositions(vertexPositions);
        //add to memory
        LineRenderer original_line = Instantiate(lineRenderer);
        original_line.transform.SetParent(cache.transform);
        original_line.enabled= false;
        original_lines.Add(original_line);
        visibilities.Add(n_vertex);
        if (original_color == Color.white) {
            Color tmp = lineRenderer.material.GetColor("_BaseColor");
            original_color = new Color(tmp.r, tmp.g, tmp.b);
        };
    }
    void TrimHair(int GRANULARITY=3){
        if(rayInteraction._selected_cell== null){
            for (int i=0;i<n_rendered_strand;i++)
            {
                Transform child = this.transform.GetChild(i);
                LineRenderer line = child.gameObject.GetComponent<LineRenderer>();
                int n_rendered_vertex = visibilities[i];
                int n_vertex = original_lines[i].positionCount;
                if (n_rendered_vertex-GRANULARITY > 0){
                    for(int v_idx=n_rendered_vertex-GRANULARITY;v_idx<n_vertex;v_idx++){
                        line.SetPosition(v_idx, line.GetPosition(n_rendered_vertex-GRANULARITY-1));
                    }
                    visibilities[i] -= GRANULARITY;
                }
            }
        }
        else{
            string[] name_split = rayInteraction._selected_cell.name.Split('_');
            int i = int.Parse(name_split[1]);
            int j = int.Parse(name_split[2]);
            int k = int.Parse(name_split[3]);
            Vector3 _cell_key = new Vector3(i,j,k);
            foreach (int strand_idx in gridManager.Cell2Strand[_cell_key]){
                LineRenderer line = gameObject.transform.Find("hair" + strand_idx).GetComponent<LineRenderer>();;
                int n_rendered_vertex = visibilities[strand_idx];
                int n_vertex = original_lines[strand_idx].positionCount;
                if (n_rendered_vertex-GRANULARITY > 0){
                    for(int v_idx=n_rendered_vertex-GRANULARITY;v_idx<n_vertex;v_idx++){
                        line.SetPosition(v_idx, line.GetPosition(n_rendered_vertex-GRANULARITY-1));
                    }
                    visibilities[strand_idx] -= GRANULARITY;
                }
            }
        }
    }
    void GrowHair(int GRANULARITY=3){
        if(rayInteraction._selected_cell== null){
            for (int i=0;i<n_rendered_strand;i++)
            {
                Transform child = this.transform.GetChild(i);
                LineRenderer line = child.gameObject.GetComponent<LineRenderer>();
                int n_rendered_vertex = visibilities[i];
                int n_vertex = original_lines[i].positionCount;
                if (n_rendered_vertex+GRANULARITY <= n_vertex){
                    for(int j=n_rendered_vertex;j<n_rendered_vertex+GRANULARITY;j++){
                        line.SetPosition(j, original_lines[i].GetPosition(j));
                    }
                    for(int j=n_rendered_vertex+GRANULARITY;j<n_vertex;j++){
                        line.SetPosition(j, original_lines[i].GetPosition(n_rendered_vertex+GRANULARITY));
                    }
                    visibilities[i] += GRANULARITY;
                }
            }
        }
        else{
            string[] name_split = rayInteraction._selected_cell.name.Split('_');
            int i = int.Parse(name_split[1]);
            int j = int.Parse(name_split[2]);
            int k = int.Parse(name_split[3]);
            Vector3 _cell_key = new Vector3(i,j,k);
            foreach (int strand_idx in gridManager.Cell2Strand[_cell_key]){
                LineRenderer line = gameObject.transform.Find("hair" + strand_idx).GetComponent<LineRenderer>();;
                int n_rendered_vertex = visibilities[strand_idx];
                int n_vertex = original_lines[strand_idx].positionCount;
                if (n_rendered_vertex+GRANULARITY <= n_vertex){
                    for(int v_idx=n_rendered_vertex;v_idx<n_rendered_vertex+GRANULARITY;v_idx++){
                        line.SetPosition(v_idx, original_lines[i].GetPosition(v_idx));
                    }
                    for(int v_idx=n_rendered_vertex+GRANULARITY;v_idx<n_vertex;v_idx++){
                        line.SetPosition(v_idx, original_lines[i].GetPosition(n_rendered_vertex+GRANULARITY));
                    }
                    visibilities[i] += GRANULARITY;
                }
            }
        }
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
                int n_vertex = original_lines[i].positionCount;
                for(int j=0;j<n_vertex;j++){
                    line.SetPosition(j, line.GetPosition(0));
                }
                visibilities[i] = 1;
            }
    }
    void ResetHair(){
        for (int i=0;i<n_rendered_strand;i++)
            {
                Transform child = this.transform.GetChild(i);
                LineRenderer line = child.gameObject.GetComponent<LineRenderer>();
                int n_vertex = original_lines[i].positionCount;
                for(int j=0;j<n_vertex;j++){
                    line.SetPosition(j, original_lines[i].GetPosition(j));
                }
                line.material.SetColor("_BaseColor", original_color);
                visibilities[i] = n_vertex;
            }
            
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
        original_lines = new List<LineRenderer>();
        visibilities = new List<int>();
        original_color = Color.white;
        for (int i=0;i<n_rendered_strand;i++)
        {
            Destroy(this.transform.GetChild(i).gameObject);
            Destroy(cache.transform.GetChild(i).gameObject);
        }
        //load new model
        hair_model_index = (hair_model_index + 1) % hair_models.Length;
        string filename ="Assets/demo_files/hair_models/"+ hair_models[hair_model_index] +".data";
        GenerateModel(filename);

    }
    
    void Update(){
        if (Input.GetKey (KeyCode.H)){ //new hair
            Debug.Log("load new hair");
            NewHair();
        }
        if (Input.GetKey (KeyCode.R)){ //reset
            Debug.Log("reset hair");
            ResetHair();
        }
        if (Input.GetKey (KeyCode.C)){ //clear
            Debug.Log("clear hair");
            ClearHair();
        }
        if (Input.GetKey (KeyCode.T)){ //trimmed
            Debug.Log("trim hair");
            TrimHair();
        }
        if (Input.GetKey (KeyCode.G)){ //grow
            Debug.Log("grow hair");
            GrowHair();
        }
        if (Input.GetKey (KeyCode.Y)){ //dyed
            Debug.Log("dye hair");
            DyeHair(UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
        }
    }
}
