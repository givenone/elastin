using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Dictionary<Vector3, List<int>> Cell2Strand;
    public GameObject cell;
    public HairManager hairManager;
    int n_x, n_y, n_z;

    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    void GenerateCell(float cx, float cy, float cz, float dx, float dy, float dz, int i, int j, int k){
        GameObject Cell = GameObject.Instantiate(cell);
        Cell.transform.SetParent(gameObject.transform);
        Cell.transform.position = new Vector3(cx, cy, cz);
        Cell.transform.localScale = new Vector3(dx, dy, dz);
        Cell.transform.name = string.Format("cell_{0}_{1}_{2}",i,j,k);
    }
    public void GenerateGrid(float min_x, float max_x, float min_y, float max_y, float min_z, float max_z){
        Debug.Log(min_x + ", " + min_y + ", " + min_z);
        n_x = n_y = n_z = 10;

        float dx = (max_x - min_x)/n_x;
        float dy = (max_y - min_y)/n_y;
        float dz = (max_z - min_z)/n_z;
        for (int i=0;i<n_x; i++){
            for (int j=0;j<n_y; j++){
                for (int k=0;k<n_z; k++){
                    GenerateCell(min_x + i*dx, min_y + j*dy, min_z + k*dz, dx, dy, dz, i, j, k);
                }
            }    
        }
        Debug.Log(gameObject.transform.GetChild(0).transform.position);
        UpdateDict(min_x, min_y, min_z, dx, dy, dz);
    }
    void UpdateDict(float min_x, float min_y, float min_z, float dx, float dy, float dz){
        Cell2Strand = new Dictionary<Vector3, List<int>>();
        
        for (int i=0;i<hairManager.n_rendered_strand;i++)
        {
            LineRenderer strand = hairManager.transform.GetChild(i).gameObject.GetComponent<LineRenderer>();;
            for (int j=0;j<strand.positionCount;j++)
            {
                Vector3 vertex = strand.GetPosition(j);
                int ix = (int)((vertex[0] - min_x) / dx);
                int iy = (int)((vertex[1] - min_y) / dy);
                int iz = (int)((vertex[2] - min_z) / dz);
                Vector3 cell_idx = new Vector3(ix, iy, iz);
                if (Cell2Strand.ContainsKey(cell_idx)) Cell2Strand[cell_idx].Add(i);
                else Cell2Strand.Add(cell_idx, new List<int>(){i});
            }
        }
        for (int i=0;i<n_x; i++){
            for (int j=0;j<n_y; j++){
                for (int k=0;k<n_z; k++){
                    Vector3 cell_idx = new Vector3(i, j, k);
                    if (!Cell2Strand.ContainsKey(cell_idx)) {
                        Destroy(gameObject.transform.Find(string.Format("cell_{0}_{1}_{2}",i,j,k)).gameObject);
                    };
                }
            }    
        }
    }
}
