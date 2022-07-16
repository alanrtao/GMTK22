using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyPool : MonoBehaviour
{
    public List<BaseEnemy> Pool => m_EnemyPool;
    private List<BaseEnemy> m_EnemyPool;

    private void Start()
    {
        var ct = transform.childCount;
        m_EnemyPool = new List<BaseEnemy>(ct * 5);
        for (int i = 0; i < ct; i++) {
            m_EnemyPool.Add(transform.GetChild(i).GetComponent<BaseEnemy>());
        }
        m_EnemyPool = m_EnemyPool.OrderBy(_ => Random.value).ToList();
    }

    public void StartEnemyTurns() => StartCoroutine(TakeTurns());

    private IEnumerator TakeTurns()
    {
        Player.Instance.blocked = true;
        for (var i = 0; i < m_EnemyPool.Count; i++)
        {
            var e = m_EnemyPool[i];
            e.UpdateStages(i);
            StartCoroutine(e.Turn());
            yield return new WaitForSeconds(e.ActionStagesTime);
        }

        if (m_EnemyPool.Count > 0)
        {
            var eLast = m_EnemyPool.Last();
            var p = GameManager.Map.Fold(Mathf.FloorToInt(eLast.transform.position.z), Mathf.FloorToInt(eLast.transform.position.x));
            GameManager.Map.EntityObstacle(p);
        }

        Player.Instance.blocked = false;
        Player.Instance.Turn();
    }
}
