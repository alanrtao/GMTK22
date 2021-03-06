using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BasicEnemy : BaseEnemy
{
    public int kAttack = 4;
    [SerializeField] int kSteps = 2;

    protected bool alertBuffer = false;

    public override void UpdateStages(int i)
    {
        stages = new List<ActionStage>();
        if (i == 0)
        {
            stages.Add(ShiftCamera(Player.Instance, this));
        } else
        {
            stages.Add(ShiftCamera(GameManager.Pool.Pool[i - 1], this));
        }

        var ex = Mathf.RoundToInt(Player.Instance.transform.position.x);
        var ez = Mathf.RoundToInt(Player.Instance.transform.position.z);
        var sx = Mathf.RoundToInt(transform.position.x);
        var sz = Mathf.RoundToInt(transform.position.z);
        var moves = AStar(sz, sx, ez, ex);

        moves.Item1 = moves.Item1.Take(kSteps).ToList();
        moves.Item2 = moves.Item2.Take(kSteps).ToList();

        // after how many steps, the enemy is adjacent to the player
        var idx = moves.Item1.FindIndex(p => p.Item1 == ez && p.Item2 == ex);

        Debug.Log($"{gameObject.name} action: {string.Join(", ", moves.Item1)}, {string.Join(", ", moves.Item2)}, attack at step {idx}");

        if (idx == 0)
        {
            // stay and attack
            //if (!alertBuffer)
            //{
            //    alertBuffer = true;
            //    ShowAlert(true);
            //} else
            //{
                Debug.Log($"Enter attack action for { gameObject.name }");
                stages.Add(AttackAction(Player.Instance, kAttack + wave));
                ShowAlert(false);
            //}
        } else if (idx > 0)
        {
            // walk up to the player
            stages.Add(MovesAction(moves.Item2.Take(Mathf.Min(idx, kSteps)).ToList()));
            //alertBuffer = true;
            //ShowAlert(true);
            // stages.Add(AttackAction(Player.Instance, kAttack));
            stages.Add(AttackAction(Player.Instance, kAttack));
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
    
}
