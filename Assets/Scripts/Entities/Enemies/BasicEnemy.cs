using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BasicEnemy : BaseEnemy
{
    bool right = true;

    [SerializeField] int kAttack = 4;
    [SerializeField] int kSteps = 2;

    protected bool alertBuffer = false;

    public override void UpdateStages(int i)
    {
        right = !right;

        stages = new List<ActionStage>();
        if (i == 0)
        {
            stages.Add(ShiftCamera(Player.Instance, this));
        } else
        {
            stages.Add(ShiftCamera(GameManager.Pool.Pool[i - 1], this));
        }

        var ex = Mathf.FloorToInt(Player.Instance.transform.position.x);
        var ez = Mathf.FloorToInt(Player.Instance.transform.position.z);
        var sx = Mathf.FloorToInt(transform.position.x);
        var sz = Mathf.FloorToInt(transform.position.z);
        var moves = AStar(sz, sx, ez, ex);

        moves.Item1 = moves.Item1.Take(kSteps + 1).ToList();
        moves.Item2 = moves.Item2.Take(kSteps).ToList();

        // after how many steps, the enemy is adjacent to the player
        var idx = moves.Item1.FindIndex(p => p.Item1 == ez && p.Item2 == ex);

        if (idx == 0)
        {
            // stay and attack
            if (!alertBuffer)
            {
                alertBuffer = true;
                ShowAlert(true);
            } else
            {
                stages.Add(AttackAction(Player.Instance, kAttack));
                ShowAlert(false);
            }
        } else if (idx > 0)
        {
            // walk up to the player
            stages.Add(MovesAction(moves.Item2.Take(Mathf.Min(idx, kSteps)).ToList()));
            alertBuffer = true;
            ShowAlert(true);
            // stages.Add(AttackAction(Player.Instance, kAttack));
        } else
        {
            // walk the full path
            alertBuffer = false;
            ShowAlert(false);
            stages.Add(MovesAction(moves.Item2));
        }

        if (i == GameManager.Pool.Pool.Count - 1)
            stages.Add(ShiftCamera(this, Player.Instance));
    }

    [SerializeField] TMPro.TextMeshPro AlertUI;
    protected void ShowAlert(bool b) => AlertUI.enabled = b;

    protected override IEnumerator TakeDamageAnimation(int damage)
    {
        collisionSource.GenerateImpulse(1 + Mathf.Log(damage));
        yield return new WaitForFixedUpdate();
    }
}
