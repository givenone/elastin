using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RayInteraction : MonoBehaviour
{
    public LayerMask whatIsTarget;
    private Camera mainCam;
    public GridManager gridManager;
    public HairManager hairManager;
    public GameObject Group;
    public Slider selectSensitivity;
    public float distance = 10f;
    public bool rayEnable;
    public List<int> _selected_strands = new List<int>();
    private Color default_color;
    private Color selected_color;

    private ToggleGroup selectToggle;
    private int mode;
    private float threshold = 0.05f;


    public void changeSensitivity(){
        threshold = selectSensitivity.value;
    }

    public void ToggleMode()
    {
        mode = 0;
        foreach(Toggle toggle in selectToggle.ActiveToggles() )
        {
            mode = int.Parse(toggle.name);
        }
        Debug.Log(mode.ToString());
    }

    public void Toggle()
    {
        rayEnable = !rayEnable;
    }

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        selected_color = Color.white;
        default_color = Color.white;

        rayEnable = true;
        selectToggle = Group.GetComponent<ToggleGroup>();
        mode = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (rayEnable && (mode != 0) && Input.GetMouseButtonDown(0)){ // flag true and not 선택안함 mode and left click
            RaycastHit hit;
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out hit, distance, whatIsTarget)){ // cast ray from rayOrigin towards rayDir for distance
                if(default_color == Color.white){
                    Color tmp = hairManager.transform.Find("hair0").GetComponent<LineRenderer>().material.GetColor("_BaseColor");
                    default_color = new Color(tmp.r, tmp.g, tmp.b);
                }
                
                var selected_cell = hit.transform;

                string[] _name_split = selected_cell.name.Split('_');
                int _i = int.Parse(_name_split[1]);
                int _j = int.Parse(_name_split[2]);
                int _k = int.Parse(_name_split[3]);
                Vector3 _cell_key = new Vector3(_i,_j,_k);
                if (gridManager.Cell2Strand.ContainsKey(_cell_key)){
                    int first = gridManager.Cell2Strand[_cell_key][0];
                    Debug.Log(hairManager.n_rendered_strand.ToString() + " " + hairManager.strandCkpt.Count.ToString());
                    foreach (int _strand_idx in gridManager.Cell2Strand[_cell_key]){
                        // 연속성이 없어서 구현해본 부분(가까운 것 선택. 일단 선택만)                       
                                                
                        if(mode == 2){
                            // List에서 제거
                            if(_selected_strands.Contains(_strand_idx))
                            hairManager.transform.Find("hair" + _strand_idx).GetComponent<LineRenderer>().material.SetColor("_BaseColor", default_color);
                            _selected_strands.Remove(_strand_idx);
                        }
                        else if(mode == 1){ // List에 추가
                            if(Vector3.Distance(hairManager.strandCkpt[first], hairManager.strandCkpt[_strand_idx]) < threshold)
                            {
                                hairManager.transform.Find("hair" + _strand_idx).GetComponent<LineRenderer>().material.SetColor("_BaseColor", selected_color);
                                _selected_strands.Add(_strand_idx);
                            }
                        }
                    }
                }
            }
        }
    }

    public void ResetList()
    {
        _selected_strands = new List<int>();
    }
}
