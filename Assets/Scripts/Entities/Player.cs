using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class Player : BaseRollable
{
    public static Player Instance { get; private set; }

    public int MAX_STAMINA;
    protected int stamina;

    void Awake()
    {
        Instance = this;
    }

    protected override void Start()
    {
        base.Start();
        blocked = false;

        GameManager.Map.SetObstacle(Mathf.FloorToInt(transform.position.z), Mathf.FloorToInt(transform.position.x), this);
        Turn();
    }

    public bool blocked;

    void Update()
    {
        if (blocked) return;
        if (stamina > 0)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                Move(Direction.UP);
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                Move(Direction.DOWN);
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Move(Direction.LEFT);
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                Move(Direction.RIGHT);
            } else if (Input.GetKeyDown(KeyCode.Space)) {
                Ultimate();
            } else if (Input.GetKeyDown(KeyCode.Return))
            {
                GameManager.Pool.StartEnemyTurns();
            }
        } else
        {
            GameManager.Pool.StartEnemyTurns();
        }
    }

    public virtual void Ultimate() => Debug.Log("ult");

    public virtual void Attack(int damage, BaseEnemy enemy)
    {
        Debug.Log($"Dealing {damage} damage");
        StartCoroutine(PlayOneshot(AttackAction(enemy, damage)));
        enemy.TakeDamage(damage);
    }

    public IEnumerator PlayOneshot(ActionStage action)
    {
        float t = 0;
        action.Do(0);
        yield return new WaitForEndOfFrame();
        t += Time.deltaTime;
        while (t < action.t)
        {
            action.Do(t / action.t);
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        action.Do(1);
    }

    public override void Move(Direction d)
    {
        Orientation curr = this;
        Orientation pred = d * curr;

        foreach (var e in GameManager.Pool.Pool)
        {
            if (e == pred)
            {
                stamina = Mathf.Max(0, stamina - 1);
                var top = FindClosestCurrFace(Vector3.up);
                Debug.Log(string.Join(", ", OrderedFaces()));
                Attack(top, e);
                Debug.Log($"Player attacks enemy at {pred.position_GRD}");
                return;
            }
        }

        // Debug.Log($"Player: {curr.position_GRD} => {pred.position_GRD}");

        if (GameManager.Map.Legal(pred.position_GRD))
        {
            base.Move(d);
            stamina = Mathf.Max(0, stamina - 1);
        } else
        {
            Debug.Log("invalid move");
        }
    }

    public void Turn()
    {
        stamina = MAX_STAMINA;
    }

    protected override void Die()
    {
        SceneManager.LoadScene(0);
    }

    protected override IEnumerator TakeDamageAnimation(int damage)
    {
        collisionSource.GenerateImpulse();
        yield return new WaitForFixedUpdate();
        float t = 0;
        while (t < 0.25f)
        {
            var p = t / 0.25f;
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        while (t < 0.5f) {
            var p = (t - 0.25f) / 0.25f;
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
