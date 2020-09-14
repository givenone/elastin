using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayInteraction : MonoBehaviour
{
    public LayerMask whatIsTarget;
    private Camera mainCam;
    public GridManager gridManager;
    public HairManager hairManager;
    public float distance = 10f;
    public Transform _selected_cell;
    private Color default_color;
    private Color selected_color;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        selected_color = Color.white;
        default_color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)){ // left click
            RaycastHit hit;
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out hit, distance, whatIsTarget)){ // cast ray from rayOrigin towards rayDir for distance
                if(default_color == Color.white){
                    Color tmp = hairManager.transform.Find("hair0").GetComponent<LineRenderer>().material.GetColor("_BaseColor");
                    default_color = new Color(tmp.r, tmp.g, tmp.b);
                }
                
                if (_selected_cell!=null){
                    string[] _name_split = _selected_cell.name.Split('_');
                    int _i = int.Parse(_name_split[1]);
                    int _j = int.Parse(_name_split[2]);
                    int _k = int.Parse(_name_split[3]);
                    Vector3 _cell_key = new Vector3(_i,_j,_k);
                    if (gridManager.Cell2Strand.ContainsKey(_cell_key)){
                        foreach (int _strand_idx in gridManager.Cell2Strand[_cell_key]){
                            hairManager.transform.Find("hair" + _strand_idx).GetComponent<LineRenderer>().material.SetColor("_BaseColor", default_color);
                        }
                        _selected_cell=null;
                    }
                }
                
                var selected_cell = hit.transform;
                string[] name_split = selected_cell.name.Split('_');
                int i = int.Parse(name_split[1]);
                int j = int.Parse(name_split[2]);
                int k = int.Parse(name_split[3]);
                Vector3 cell_key = new Vector3(i,j,k);
                if (gridManager.Cell2Strand.ContainsKey(cell_key)){
                    foreach (int strand_idx in gridManager.Cell2Strand[cell_key]){
                        hairManager.transform.Find("hair" + strand_idx).GetComponent<LineRenderer>().material.SetColor("_BaseColor", selected_color);
                    }
                    _selected_cell = selected_cell;
                }                
            }
        }
    }
}
