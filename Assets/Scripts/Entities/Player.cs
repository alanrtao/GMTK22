using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : BaseRollable
{
    public static Player Instance { get; private set; }

    public int MAX_STAMINA;
    protected int stamina;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        blocked = false;
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
            } else if (Input.GetKeyDown(KeyCode.Return))
            {
                GameManager.Pool.StartEnemyTurns();
            }
        } else
        {
            GameManager.Pool.StartEnemyTurns();
        }
    }

    public override void Move(Direction d)
    {
        base.Move(d);
        stamina = Mathf.Max(0, stamina - 1);
    }

    public void Turn()
    {
        stamina = MAX_STAMINA;
    }
}
