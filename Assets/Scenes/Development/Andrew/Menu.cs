using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    //note here:
    //when space or mouse left click is detected, go to "placeholder" level

    void Update()
    {
        if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
        {
            //Debug.Log("enter pressed");
            LoadScene();
        }
    }

    void LoadScene()
    {
        //change the level here
        //Application.LoadLevel(""); //<----placeholder level, need to change
        SceneManager.LoadScene(1);
    }

}
