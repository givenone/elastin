using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    public GameObject linePrefab;
     
    private LineRenderer lineRenderer;
    private GameObject line;
    private List<Vector3> dotPositions;
    private float maxLength = 10f;
    private float linelength;
    private bool onCanvas = false;
    private int maxcount = 100;
    // Start is called before the first frame update
    void Start()
    {
        linelength = 0f;
        dotPositions = new List<Vector3>();
    }

    void Awake()
    {
        linelength = 0f;
        dotPositions = new List<Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        // if mode on.
        if(onCanvas){

            Vector3 mousePosition = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z);
            Vector3 mouse_world_position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.0f));

            if(onCanvas && Input.GetMouseButtonDown(0))
            {// Line 시작
                if(line != null) Destroy(line); // 전에 남아있던 데이터 지움.
                dotPositions.RemoveRange(0, dotPositions.Count);

                dotPositions.Add(mouse_world_position);
                createLine();
            }
            else if(onCanvas && Input.GetMouseButton(0))
            {
                if(Vector3.Distance(mouse_world_position, dotPositions[dotPositions.Count - 1]) > 0.01f){ // 너무 라인 많이 생기지 않도록.
                    // Line 이어주기.
                    dotPositions.Add(mouse_world_position);
                    updateLine();
                }
            }
        }
    }

    void createLine()
    {
        Debug.Log("created");
        line = GameObject.Instantiate(linePrefab);     
        lineRenderer = line.GetComponent<LineRenderer>();
        lineRenderer.SetWidth(0.02f, 0.02f);
        lineRenderer.SetPosition(0, dotPositions[0]);
        lineRenderer.positionCount = 1;
    }

    void updateLine()
    {
        if(lineRenderer.positionCount < maxcount){

            lineRenderer.positionCount++;
            lineRenderer.SetPosition(dotPositions.Count -1,  dotPositions[dotPositions.Count - 1]);
            linelength += Vector3.Distance(dotPositions[dotPositions.Count - 1], dotPositions[dotPositions.Count - 2]);
        }
    }

    public void onEnter(){
        onCanvas = true;
    }

    public void onExit(){
        onCanvas = false;
    }
}
