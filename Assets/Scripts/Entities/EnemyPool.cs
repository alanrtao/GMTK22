using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyPool : MonoBehaviour
{
    public List<BaseEnemy> Pool => m_EnemyPool;
    private List<BaseEnemy> m_EnemyPool;

    private IEnumerator Start()
    {
        var ct = transform.childCount;
        m_EnemyPool = new List<BaseEnemy>(ct * 5);
        for (int i = 0; i < ct; i++) {
            m_EnemyPool.Add(transform.GetChild(i).GetComponent<BaseEnemy>());
        }
        m_EnemyPool = m_EnemyPool.OrderBy(_ => Random.value).ToList();

        while (!GameManager.Map.done) yield return null;

        m_EnemyPool.ForEach(e => GameManager.Map.SetObstacle(e));

        InEnemyTurn = false;
    }

    public void StartEnemyTurns() => StartCoroutine(TakeTurns());

    public bool InEnemyTurn = false;
    private int tCount = 0;
    private IEnumerator TakeTurns()
    {
        Debug.Log($"Entering Turn {tCount++}");
        Player.Instance.blocked = true;
        InEnemyTurn = true;
        for (var i = 0; i < m_EnemyPool.Count; i++)
        {
            var e = m_EnemyPool[i];
            e.done = false;
            e.UpdateStages(i);
            StartCoroutine(e.Turn());
            while (!e.done || e.IsMoving)
                yield return null;
        }
        Player.Instance.Turn();
        InEnemyTurn = false;
        Player.Instance.blocked = false;
    }
}
