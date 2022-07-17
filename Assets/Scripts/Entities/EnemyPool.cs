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
    }

    public void StartEnemyTurns() => StartCoroutine(TakeTurns());

    private IEnumerator TakeTurns()
    {
        Player.Instance.blocked = true;
        for (var i = 0; i < m_EnemyPool.Count; i++)
        {
            var e = m_EnemyPool[i];
            e.done = false;
            e.UpdateStages(i);
            StartCoroutine(e.Turn());
            while (!e.done)
                yield return null;
        }

        Player.Instance.blocked = false;
        Player.Instance.Turn();
    }
}
