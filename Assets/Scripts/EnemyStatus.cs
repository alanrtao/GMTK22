using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemyStatus : MonoBehaviour
{
    public BasicEnemy Me;
    public TextMeshPro T;

    void Update()
    {
        T.text = Me.HP + "/" + Me.MAX_HP;
    }
}
