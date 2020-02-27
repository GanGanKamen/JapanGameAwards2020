using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineControler : MonoBehaviour
{
    public GameObject food = null;
    Transform foodTra;

    /*-----上から視点の時-----*/
    float X_MPos, Z_MPos;
    Vector3 newTra;
    Vector2 downMousePos;
    float xPos, zPos;
    Vector3 upCameraTra;



    // Start is called before the first frame update
    void Start()
    {

        foodTra = food.GetComponent<Transform>();

        upCameraTra = foodTra.position;
    }

    // Update is called once per frame
    void Update()
    {
            if(Input.GetMouseButtonDown(0))
            {
                //カメラのポジションを格納
                newTra = Camera.main.transform.position;
                //マウスの座標を格納
                downMousePos = Input.mousePosition;

            }
            else if(Input.GetMouseButton(0))
            {
                X_MPos = downMousePos.x - Input.mousePosition.x;
                Z_MPos = downMousePos.y - Input.mousePosition.y;
                xPos = upCameraTra.x + X_MPos / 45;
                zPos = upCameraTra.z + Z_MPos / 45;
            }
           else if(Input.GetMouseButtonUp(0))
            {
                upCameraTra = this.transform.position;
            }

            transform.position = new Vector3(xPos, 20, zPos);
    }
}
