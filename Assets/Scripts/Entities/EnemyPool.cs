using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class EnemyPool : MonoBehaviour
{
    public List<BaseEnemy> Pool => m_EnemyPool;
    private List<BaseEnemy> m_EnemyPool;

    [SerializeField] GameObject BasicEnemyPrototype, RangedEnemyPrototype;

    public int EnemyCount = 8;
    public int SpawnPerTurn;
    public int TurnsPerWave = 8;
    public int AlreadySpawned = 0;

    private IEnumerator Start()
    {
        m_EnemyPool = new List<BaseEnemy>(EnemyCount);

        while (!GameManager.Map.done) yield return null;

        AlreadySpawned = 0;
        Spawn();

        InEnemyTurn = false;
        Player.Instance.blocked = false;
    }

    private void Spawn()
    {
        int toSpawn = Mathf.Min(SpawnPerTurn, EnemyCount - AlreadySpawned);
        AlreadySpawned += toSpawn;

        var sz = Mathf.RoundToInt(Player.Instance.transform.position.z);
        var sx = Mathf.RoundToInt(Player.Instance.transform.position.x);

        var minDist = 4;
        var maxDist = Mathf.Max(GameManager.Map.WIDTH, GameManager.Map.HEIGHT);
        var r = minDist;

        int tries = 0;
        while (toSpawn > 0 && tries < 1000)
        {
            var candidates = GameManager.Map.GetEmptyAtDistance(sz, sx, r);
            if (candidates.Count > 0)
            {
                var pick = candidates[Mathf.FloorToInt(Random.value * candidates.Count)];
                GameObject enemy = Instantiate(BasicEnemyPrototype, transform);
                //if (Random.value < 0.3f)
                //{
                //    enemy = Instantiate(RangedEnemyPrototype, transform);
                //} else
                //{
                //    enemy = Instantiate(BasicEnemyPrototype, transform);
                //}
                enemy.transform.position = new Vector3(pick.Item2, 0, pick.Item1);
                var bEnemy = enemy.GetComponent<BaseEnemy>();

                bEnemy.MAX_HP += SpawnPerTurn - 1;
                bEnemy.wave = SpawnPerTurn - 1;
                bEnemy.SetHP(bEnemy.MAX_HP);

                m_EnemyPool.Add(bEnemy);

                GameManager.Map.SetObstacle(bEnemy);
                toSpawn -= 1;
                Debug.Log($"Spawned {enemy.name}");
            }

            r += Mathf.FloorToInt(Random.value * 4);
            r %= maxDist;
            tries += 1;
            if (r < minDist) r = minDist;
        }
    }

    private void Update()
    {
        if (AlreadySpawned == EnemyCount && m_EnemyPool.Count == 0)
        {
            SceneManager.LoadScene(3);
        }
    }

    public void StartEnemyTurns() => StartCoroutine(TakeTurns());


    public bool InEnemyTurn = false;
    private int tCount = 0;
    private IEnumerator TakeTurns()
    {
        Debug.Log($"Entering Turn {tCount++}");
        Player.Instance.blocked = true;
        SpawnPerTurn = tCount / TurnsPerWave + 1;
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
        Spawn();
        InEnemyTurn = false;
        Player.Instance.blocked = false;
    }
}
