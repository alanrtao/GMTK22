using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Render : MonoBehaviour
{
    public List<SpriteRenderer> mesh;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            for (int i =0; i < mesh.Count; i++)
            {
                if(Vector3.Dot(mesh[i].transform.forward, Camera.main.transform.forward) > 0)
                {
                    mesh[i].enabled = false;
                    //will not enable render after key is released

                }
                else
                {
                    mesh[i].enabled = true;

                }
            }
        }
        else
        {
            for (int i = 0; i < mesh.Count; i++)
            {
                mesh[i].enabled = true;
            }
        }
    }
}
