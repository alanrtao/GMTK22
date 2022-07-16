using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static Map Map => Instance.m_Map;
    public static EnemyPool Pool => Instance.m_Pool;

    public static GameManager Instance { get; private set; }

    [SerializeField] Map m_Map;
    [SerializeField] EnemyPool m_Pool;

    private void Awake()
    {
        Instance = this;
    }
}
