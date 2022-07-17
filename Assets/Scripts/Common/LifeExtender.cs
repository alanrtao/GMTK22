using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LifeExtender : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += (prev, next) => DontDestroyOnLoad(gameObject);
    }
}
