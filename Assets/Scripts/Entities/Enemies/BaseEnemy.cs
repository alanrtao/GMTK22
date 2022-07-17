using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public abstract class BaseEnemy : BaseRollable
{
    public List<ActionStage> stages;
    public float ActionStagesTime => stages.Select(a => a.t).Sum();

    public abstract void UpdateStages(int index);

    protected override void Die() {
        GameManager.Pool.Pool.Remove(this);
        GameManager.Map.SetObstacle(Mathf.RoundToInt(transform.position.z), Mathf.RoundToInt(transform.position.x), null);
        gameObject.SetActive(false);
    }

    public bool done;
    public IEnumerator Turn()
    {
        done = false;
        var tSum = 0f;
        var ts = new float[stages.Count()];
        for(int i = 0; i < stages.Count(); i++)
        {
            tSum += stages[i].t;
            ts[i] = tSum;
        }
        float t = 0;
        for (int i = 0; i < stages.Count(); i++)
        {
            float t0 = i == 0 ? 0 : ts[i - 1];
            float t1 = ts[i];
            stages[i].Do(0);

            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();

            while (t < t1)
            {
                stages[i].Do((t - t0) / t1);
                t += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            stages[i].Do(1);
            yield return new WaitForFixedUpdate();
        }
        done = true;
    }

    protected virtual float Heuristic(int ez, int ex, int z, int x) => Mathf.Sqrt((ez - z) * (ez - z) + (ex - x) * (ex - x));

    protected (List<(int, int)>, List<Direction>) AStar(int sz, int sx, int ez, int ex)
    {
        // Debug.Log($"Find path from {sz}, {sx} to {ez}, {ex}");
        var start = GameManager.Map.Fold(sz, sx);
        var goal = GameManager.Map.Fold(ez, ex);

        var open = new List<(int, int)> { start };

        var cameFrom = new (int, int)[GameManager.Map.WIDTH, GameManager.Map.HEIGHT];
        var g = new float[GameManager.Map.WIDTH, GameManager.Map.HEIGHT];
        var f = new float[GameManager.Map.WIDTH, GameManager.Map.HEIGHT];

        for (int i = 0; i < cameFrom.GetLength(0); i++) {
            for (int j = 0; j < cameFrom.GetLength(1); j++)
            {
                cameFrom[i, j] = (-1, -1);
                g[i, j] = int.MaxValue;
                f[i, j] = int.MaxValue;
            }
        }

        g[sz, sx] = 0;
        f[sz, sx] = Heuristic(ez, ex, sz, sx);

        while(open.Count > 0)
        {
            var curr = open.OrderBy(i => f[i.Item1, i.Item2]).First();
            // Debug.Log(GameManager.Map.Expand(curr));
            if (curr == goal)
            {
                // Debug.Log("hit!");
                return Reconstruct(cameFrom, curr); //.Take(max_step).ToList();
            }

            open.Remove(curr);

            var up = (curr.Item1, curr.Item2 + 1);
            var down = (curr.Item1, curr.Item2 - 1);
            var left = (curr.Item1 + 1, curr.Item2);
            var right = (curr.Item1 - 1, curr.Item2);

            var neighbors = new List<(int, int)>();
            if (GameManager.Map.Legal<BaseEnemy>(up.Item1, up.Item2)) neighbors.Add(up);
            if (GameManager.Map.Legal<BaseEnemy>(down.Item1, down.Item2)) neighbors.Add(down);
            if (GameManager.Map.Legal<BaseEnemy>(left.Item1, left.Item2)) neighbors.Add(left);
            if (GameManager.Map.Legal<BaseEnemy>(right.Item1, right.Item2)) neighbors.Add(right);

            foreach(var n in neighbors)
            {
                var gTentative = g[curr.Item1, curr.Item2] + GameManager.Map.Weight(curr.Item1, curr.Item2, n.Item1, n.Item2);
                // Debug.Log($"... neighbor {GameManager.Map.Expand(n)} has g { g[n] } vs. { gTentative }");
                if (gTentative < g[n.Item1, n.Item2])
                {
                    cameFrom[n.Item1, n.Item2] = curr;
                    g[n.Item1, n.Item2] = gTentative;
                    f[n.Item1, n.Item2] = gTentative + Heuristic(ez, ex, n.Item1, n.Item2);

                    if (!open.Contains(n)) open.Add(n);
                }
            }
        }

        return (new List<(int, int)>(), new List<Direction>());
    }

    private (List<(int, int)>, List<Direction>) Reconstruct((int, int)[,] cameFrom, (int, int) curr)
    {
        var total = new List<(int, int)> { curr };
        while (cameFrom[curr.Item1, curr.Item2].Item1 != -1)
        {
            curr = cameFrom[curr.Item1, curr.Item2];
            total.Add((curr.Item1, curr.Item2));
        }

        var ds = new List<Direction>();

        for(int i = total.Count - 1; i > 0; i--)
        {
            var deltaZ = total[i - 1].Item1 - total[i].Item1;
            if (deltaZ > 0)
            {
                ds.Add(Direction.LEFT);
                continue;
            }
            if (deltaZ < 0)
            {
                ds.Add(Direction.RIGHT);
                continue;
            }
            var deltaX = total[i - 1].Item2 - total[i].Item2;
            if (deltaX > 0)
            {
                ds.Add(Direction.UP);
                continue;
            }
            if (deltaX < 0)
            {
                ds.Add(Direction.DOWN);
                continue;
            }
        }

        total.Reverse();
        total.RemoveAt(0);
        return (total, ds);
    }


    #region prebuiltStages

    // note: the 0.5f is from cinemachine brain blending settings
    protected static ActionStage ShiftCamera(BaseRollable from, BaseRollable to, float dt = 0.5f)
        => new ActionStage(dt, (t) =>
        {
            if (t == 0 && from.vCam != null && to.vCam != null) {
                from.vCam.enabled = false;
                to.vCam.enabled = true;
            }
        });

    protected ActionStage MovesAction(List<Direction> ds)
        => new ActionStage(tMove * ds.Count, (t) =>
        {
            if (t == 0) Move(ds);
        });

    protected ActionStage MoveAction(Direction d)
        => new ActionStage(tMove, (t) =>
        {
            if (t == 0) Move(d);
        });
    #endregion

}