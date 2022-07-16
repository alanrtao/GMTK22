using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Render : MonoBehaviour
{


    //private GameObject cube;

    public MeshRenderer cube;

    public List<MeshRenderer> mesh;


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Debug.Log("shift press"); //debug delete later

            

            for (int i =0; i < mesh.Count; i++)
            {
                if(Vector3.Dot(mesh[i].transform.forward, Camera.main.transform.forward) > 0)
                {
                    mesh[i].enabled = false;
                    Debug.Log("render diabled" + i);
                    //will not enable render after key is released
                }
                else
                {
                    mesh[i].enabled = true;
                    Debug.Log("render enable " + i);
                }
            }

            //GameObject.GetComponent(MeshRenderer).enabled = false;
            //gameObject.GetComponentInChildren<Renderer>().enabled = false;
            cube.enabled = false;
        }
        else
        {
            Debug.Log("shift released");
            for (int i = 0; i < mesh.Count; i++)
            {
                    mesh[i].enabled = true;
                    Debug.Log("render enable " + i);
            }
        }
    }
}
