﻿using System.Collections.Generic;
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
    public DrawLine drawline; // freecurl 함수 구현.
    public RayInteraction rayInteraction;
    public Slider slider1, slider2;
    public Dropdown curl; // Dropdown for curl style.
    public GameObject sCurlOption;
    public GameObject cCurlOption;
    public GameObject DrawCanvas;
    
    public int n_rendered_strand;
    List<LineRenderer> original_lines = new List<LineRenderer>();
    public List<Vector3> strandCkpt = new List<Vector3>(); // Grid 선택을 위한 머리카락 별 체크포인트 좌표
    float length_scale = 50.0f; // 정수화 함. (Slider onValueChange 너무 자주 실행돼서.)
    List<int> visibilities = new List<int>(); // # of vertices in a strand
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
                        
                        strandCkpt.Add(new Vector3(0, 0, 0)); // 두피 좌표 (첫 점)
                        
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

                                // strand별 ckpt(grid 선택을 위한) 설정 
                                if(j == 0){
                                    strandCkpt[strandCkpt.Count-1] = new Vector3(x,y,z);
                                }
                                if(j == 20){
                                    strandCkpt[strandCkpt.Count-1] = new Vector3(x,y,z);
                                }
                                if(j == 35){
                                    strandCkpt[strandCkpt.Count-1] = new Vector3(x,y,z);
                                }                                
     
                            }
                        }
                        for (int j = n_rendered_vertex; j < n_vertex; j++)
                        {
                            x = reader.ReadSingle();
                            y = reader.ReadSingle();
                            z = reader.ReadSingle();
                        }

                        // 수정함 : 모든 Strand 렌더링.

                        DrawAndSaveStrand(vertices, n_rendered_vertex/skip_factor, strand_count);
                        strand_count ++;
                        
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

                float dx = (max_x - min_x) / 2;
                float dy = (max_y - min_y) / 2;
                float dz = (max_z - min_z) / 2;
                gridManager.GenerateGrid(min_x - dx,  max_x + dx,  min_y - dy,  max_y + dy,  min_z - dz,  max_z + dz);
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
        if(true/*rayInteraction._selected_strands.Count == 0*/){
            // 모든 머리카락에 대해
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
            // 선택된 머리카락만
            foreach(int strand_idx in rayInteraction._selected_strands)
            {
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

        //gridManager.ChangeGrid();
    }
    void GrowHair(int GRANULARITY=3){
        if(rayInteraction._selected_strands.Count == 0){
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
            foreach(int strand_idx in rayInteraction._selected_strands)
            {                
                LineRenderer line = gameObject.transform.Find("hair" + strand_idx).GetComponent<LineRenderer>();;
                int n_rendered_vertex = visibilities[strand_idx];
                int n_vertex = original_lines[strand_idx].positionCount;
                if (n_rendered_vertex+GRANULARITY <= n_vertex){
                    for(int v_idx=n_rendered_vertex;v_idx<n_rendered_vertex+GRANULARITY;v_idx++){
                        line.SetPosition(v_idx, original_lines[strand_idx].GetPosition(v_idx));
                    }
                    for(int v_idx=n_rendered_vertex+GRANULARITY;v_idx<n_vertex;v_idx++){
                        line.SetPosition(v_idx, original_lines[strand_idx].GetPosition(n_rendered_vertex+GRANULARITY));
                    }
                    visibilities[strand_idx] += GRANULARITY;
                }
            }
        }

        gridManager.ChangeGrid();

    }
    public void ChangeLength(){ // from slider 1
        float granularity = (slider1.value - length_scale);
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
    public void ResetHair(){
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

        gridManager.ChangeGrid();
        rayInteraction.ResetList();
            
    }
    public void DyeHair(Color new_BaseColor){
        if(rayInteraction._selected_strands.Count == 0){
            for (int i=0;i<n_rendered_strand;i++)
                {
                    Transform child = this.transform.GetChild(i);
                    child.gameObject.GetComponent<LineRenderer>().material.SetColor("_BaseColor", new_BaseColor);
                }
        }
    }

    // Hair Curliness Control starts
    // by givenone

    public void cCurl(){
        float curliness = cCurlOption.transform.Find("curliness").GetComponent<Slider>().value;
        curliness = curliness / (5 + curliness);
        int no_change_index = (int)cCurlOption.transform.Find("start").GetComponent<Slider>().value;
        int up_index = (int)cCurlOption.transform.Find("cStart").GetComponent<Slider>().value;
        float C_strongness = cCurlOption.transform.Find("cStrong").GetComponent<Slider>().value;
        
        
        if(true){ // 일괄 적용 테스트 (선택 X)
            
            for (int i=0;i<n_rendered_strand;i++)
            {
                Transform child = this.transform.GetChild(i);
                LineRenderer line = child.gameObject.GetComponent<LineRenderer>();
                int n_rendered_vertex = visibilities[i];
                int n_vertex = original_lines[i].positionCount;

                if (n_vertex <= no_change_index) continue;
                
                for(int v_idx= no_change_index ; v_idx < n_rendered_vertex ; v_idx++){

                    Vector3 point = line.GetPosition(v_idx);
                    Vector3 prev_point = line.GetPosition(v_idx-1);
                    Vector3 center = new Vector3(-point.x, 0, -point.z);
                    center = center.normalized;
                    float length = (original_lines[i].GetPosition(v_idx) - original_lines[i].GetPosition(v_idx - 1)).magnitude;
                    float mag = ((float)(v_idx - no_change_index ) / (float)(n_rendered_vertex + 10));
                    mag = mag * mag; // 2차원 증가. (컬 세기 증가 속도를 끝으로 갈수록 더 증가하기 위함.)

                    Vector3 direction = ((point + center * curliness * mag) - prev_point).normalized; // 방향 벡터.
                    // 길이 조정 (원래 길이 유지)

                    point = prev_point + direction * length;
                    
                    line.SetPosition(v_idx, point);
                }

                // 끝이 더 말리는 효과 ( up 방향 (+y & center) 로 말리는 효과)
                for(int v_idx = up_index; v_idx < n_rendered_vertex; v_idx++)
                {
                    Vector3 point = line.GetPosition(v_idx);
                    Vector3 prev_point = line.GetPosition(v_idx - 1);
                    //Vector3 up = new Vector3(0, 1, 0); // TODO :: C curl의 방향을 꼭 위쪽이 아닌 유저 인풋(in 2차원 plane!)으로 지정해줄 수 있도록 설정하면 좋을 듯 ! (그리는 인터페이스 !)
                    Vector3 up = new Vector3(-point.x, 0, -point.z);
                    up = up.normalized;
                    up.y = 0.8f;
                    up = up.normalized;
                    float length = (original_lines[i].GetPosition(v_idx) - original_lines[i].GetPosition(v_idx - 1)).magnitude;
                    float mag = ((float)(v_idx - up_index) / (float)(n_rendered_vertex - up_index));
                    mag = mag * mag;

                    Vector3 direction = (point + up * curliness * mag * C_strongness ) - prev_point;

                    point = prev_point + direction.normalized * length;

                    line.SetPosition(v_idx, point);
                }
            }
            
        }
        gridManager.ChangeGrid();
    }

    public void sCurl()
    {
        float curliness = sCurlOption.transform.Find("curliness").GetComponent<Slider>().value;
        curliness = curliness / (25 + curliness);
        curliness /= 2;
        int no_change_index = (int)sCurlOption.transform.Find("start").GetComponent<Slider>().value;
        int stop_change_index = (int)sCurlOption.transform.Find("stop").GetComponent<Slider>().value;
        int change_cycle = (int)sCurlOption.transform.Find("cycle").GetComponent<Slider>().value;

        if(true){ // 일괄 적용
            
            for (int i=0;i<n_rendered_strand;i++)
            {
                Transform child = this.transform.GetChild(i);
                LineRenderer line = child.gameObject.GetComponent<LineRenderer>();
                int n_rendered_vertex = visibilities[i];
                int n_vertex = original_lines[i].positionCount;



                for(int v_idx= no_change_index ; v_idx < (stop_change_index > n_rendered_vertex ? n_rendered_vertex : stop_change_index) ; v_idx++){

                    Vector3 point = line.GetPosition(v_idx);
                    Vector3 prev_point = line.GetPosition(v_idx-1);
                    float length = (original_lines[i].GetPosition(v_idx) - original_lines[i].GetPosition(v_idx - 1)).magnitude;
                    Vector3 center = new Vector3(-point.x, 0, -point.z);
                    center = center.normalized;

                    if( (v_idx / change_cycle) % 2 == 0) center *= -1; // change direction

                    //포물선을 모델링
                    int mag_index = Math.Abs(v_idx % change_cycle - change_cycle / 2); // 2 1 0 1 2
                    float mag = (float)mag_index /  (change_cycle / 2); // 1 0.5 0 0.5 1
                    mag = 1 - (mag * mag); // 0 0.25 1 0.25 1 

                    //mag *= ((float)(v_idx + 10) / (float)n_rendered_vertex) * ((float)(v_idx + 10) / (float)n_rendered_vertex);
                    //float coeff = (float)(v_idx - no_change_index + 5) / (float)(n_rendered_vertex - no_change_index);
                    //mag *= (coeff * coeff * 5 + 0.3f);

                    Vector3 direction = (point + center * curliness * mag) - prev_point;

                    // 길이 조정 (원래 길이 유지)
                    point = prev_point + direction.normalized * length; 
                    line.SetPosition(v_idx, point);
                }
            }
            
        }
        gridManager.ChangeGrid();
    }    

    // 머리카락을 특정 vertex (ex 50) 번째부터 쭉 펴는 함수.
    public void Straight()
    {
        int n_vertex_threshold = 20; // 펴기 시작하는 vertex 번호.
        Vector3 down = new Vector3(0, 0, -1);
        if(true){
            for (int i=0;i<n_rendered_strand;i++)
            {
                Transform child = this.transform.GetChild(i);
                LineRenderer line = child.gameObject.GetComponent<LineRenderer>();
                int n_rendered_vertex = visibilities[i];
                int n_vertex = original_lines[i].positionCount;
                //Debug.Log(n_rendered_vertex);
                if(n_rendered_vertex < n_vertex_threshold) continue;
                
                Vector3 tangent = (original_lines[i].GetPosition(n_vertex_threshold-1) - original_lines[i].GetPosition(n_vertex_threshold-2)).normalized;
                for(int j=n_vertex_threshold ; j<n_rendered_vertex ; j++){
                    float delta = (original_lines[i].GetPosition(j) - original_lines[i].GetPosition(j-1)).magnitude;  // 길이
                    Vector3 dir = down * j + tangent * (n_rendered_vertex - j); 
                    line.SetPosition(j, line.GetPosition(j-1) + dir.normalized * delta);
                }                
            }
        }

        gridManager.ChangeGrid();
    }

    // Drawing 내용을 기반으로 Curl 조정.
    // 선택된 영역이 있어야만 작동하도록 설정.
    public void FreeCurl(){

        int normalize_factor = 1;
        List<Vector3> tangent = new List<Vector3>();
        int lineLength = drawline.dotPositions.Count;

        if(rayInteraction._selected_strands.Count == 0 || lineLength == 0) return;        
        Vector3 prev = drawline.dotPositions[0];

        for(int i= normalize_factor; i<lineLength; i+=normalize_factor){
            tangent.Add ( (drawline.dotPositions[i] - prev).normalized );
            prev = drawline.dotPositions[i];
        } // tangent를 계산 (world 좌표 기준임.)

        foreach(int strand_idx in rayInteraction._selected_strands)
        {                
            LineRenderer line = gameObject.transform.Find("hair" + strand_idx).GetComponent<LineRenderer>();;
            int n_rendered_vertex = visibilities[strand_idx];
            for(int v_idx=1; v_idx<n_rendered_vertex; v_idx++)
            {
                Vector3 point = line.GetPosition(v_idx);
                Vector3 prev_point = line.GetPosition(v_idx-1);
                float length = (original_lines[strand_idx].GetPosition(v_idx) - original_lines[strand_idx].GetPosition(v_idx - 1)).magnitude;
                Vector3 direction = tangent[(int)(((float)v_idx / n_rendered_vertex) * (lineLength-1) / normalize_factor)];
                point = prev_point + direction * length; 
                line.SetPosition(v_idx, point);
            }
        }
        gridManager.ChangeGrid();
    }

    public void Curly(){ 
        int style = curl.value;

        if (style == 0){
            sCurlOption.SetActive(false);
            cCurlOption.SetActive(false);
            DrawCanvas.SetActive(false);
        }  
        else if (style == 1){ // reset
            Straight();
            sCurlOption.SetActive(false);
            cCurlOption.SetActive(false);
            DrawCanvas.SetActive(false);
        }
        else if(style == 2){
            sCurlOption.SetActive(false);
            cCurlOption.SetActive(true);
            DrawCanvas.SetActive(false);
        }

        else if(style == 3){
            sCurlOption.SetActive(true);
            cCurlOption.SetActive(false);
            DrawCanvas.SetActive(false);
        }
        else if(style == 4){
            sCurlOption.SetActive(false);
            cCurlOption.SetActive(false);
            DrawCanvas.SetActive(true);
        }        
    }
    // Hair Curliness Control ends

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
