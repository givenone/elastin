using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Dictionary<Vector3, List<int>> Cell2Strand;
    public GameObject cell;
    public HairManager hairManager;
    int n_x, n_y, n_z;

    float mx, my, mz, Mx, My, Mz, dx, dy, dz;

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
        n_x = n_y = n_z = 20;
        mx = min_x;
        my = min_y;
        mz = min_z;
        Mx = max_x;
        My = max_y;
        Mz = max_z;

        dx = (max_x - min_x)/n_x;
        dy = (max_y - min_y)/n_y;
        dz = (max_z - min_z)/n_z;
        for (int i=0;i<n_x; i++){
            for (int j=0;j<n_y; j++){
                for (int k=0;k<n_z; k++){
                    GenerateCell(min_x + i*dx, min_y + j*dy, min_z + k*dz, dx, dy, dz, i, j, k);
                }
            }    
        }
        UpdateDict();
    }

    // 처음부터 여유있게 cell을 만들어 둔 후
    // Style Change 시마다 cell을 update. (새로운 Dict 생성)
    int Clamp(int n, int min, int max ) => (n >= min) ? (n <= max) ? n : max : min;

    void UpdateDict(){
        Cell2Strand = new Dictionary<Vector3, List<int>>();
        
        for (int i=0;i<hairManager.n_rendered_strand;i++)
        {
            LineRenderer strand = hairManager.transform.GetChild(i).gameObject.GetComponent<LineRenderer>();;
            for (int j=0;j<strand.positionCount;j++)
            {
                Vector3 vertex = strand.GetPosition(j);
                int ix = (int)((vertex[0] - mx) / dx);
                int iy = (int)((vertex[1] - my) / dy);
                int iz = (int)((vertex[2] - mz) / dz);
                ix = Clamp(ix, 0, n_x-1);
                iy = Clamp(iy, 0, n_y-1);
                iz = Clamp(iz, 0, n_z-1);
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

    void DestroyChild()
    {
        foreach (Transform child in transform) {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void ChangeGrid()
    {
        DestroyChild();

        dx = (Mx - mx)/n_x;
        dy = (My - my)/n_y;
        dz = (Mz - mz)/n_z;
        for (int i=0;i<n_x; i++){
            for (int j=0;j<n_y; j++){
                for (int k=0;k<n_z; k++){
                    GenerateCell(mx + i*dx, my + j*dy, mz + k*dz, dx, dy, dz, i, j, k);
                }
            }    
        }
        UpdateDict();

    }

}
