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
        T.text = Me.kAttack + "/" + Me.HP;
    }
}
