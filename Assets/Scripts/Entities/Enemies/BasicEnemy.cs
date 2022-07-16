using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : BaseEnemy
{
    bool right = true;

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
        var moves = AStar(sz, sx, ez, ex, 3);
        // stages.Add(MovesAction(moves));

        if (i == GameManager.Pool.Pool.Count - 1)
            stages.Add(ShiftCamera(this, Player.Instance));
    }
}
