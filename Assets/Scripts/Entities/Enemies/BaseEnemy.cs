using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public abstract class BaseEnemy : BaseRollable
{
    public int HP => m_HP;
    [SerializeField] protected int m_HP;

    public List<ActionStage> stages;
    public float ActionStagesTime => stages.Select(a => a.t).Sum();

    public abstract void UpdateStages(int index);

    public IEnumerator Turn()
    {
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
    }

    protected virtual float Heuristic(int sz, int sx, int z, int x) => (sz - z) * (sz - z) + (sx - x) * (sx - x);

    protected List<Direction> AStar(int sz, int sx, int ez, int ex, int max_step)
    {
        var start = GameManager.Map.Fold(sz, sx);
        var goal = GameManager.Map.Fold(ez, ex);
        var open = new List<int> { start };
        var cameFrom = new int[GameManager.Map.Count];
        var g = new float[GameManager.Map.Count];
        var f = new float[GameManager.Map.Count];
        for (int i = 0; i < cameFrom.Length; i++) {
            cameFrom[i] = -1;
            g[i] = int.MaxValue;
            f[i] = int.MaxValue;
        }

        g[start] = 0;
        f[start] = Heuristic(sz, sx, sz, sx);

        int step = 0;
        while(open.Count > 0)
        {
            step++;
            var curr = open.OrderBy(i => f[i]).First();
            if (curr == goal)
            {
                return Reconstruct(cameFrom, curr).Take(max_step).ToList();
            }

            open.Remove(curr);

            var expCurr = GameManager.Map.Expand(curr);
            var up = (expCurr.Item1, expCurr.Item2 + 1);
            var down = (expCurr.Item1, expCurr.Item2 - 1);
            var left = (expCurr.Item1 + 1, expCurr.Item2);
            var right = (expCurr.Item1 - 1, expCurr.Item2);

            var neighbors = new List<int>();
            if (GameManager.Map.Legal(up.Item1, up.Item2)) neighbors.Add(GameManager.Map.Fold(up));
            if (GameManager.Map.Legal(down.Item1, down.Item2)) neighbors.Add(GameManager.Map.Fold(down));
            if (GameManager.Map.Legal(left.Item1, left.Item2)) neighbors.Add(GameManager.Map.Fold(left));
            if (GameManager.Map.Legal(right.Item1, right.Item2)) neighbors.Add(GameManager.Map.Fold(right));

            foreach(var n in neighbors)
            {
                var gTentative = g[curr] + GameManager.Map.Weight(curr, n);
                if (gTentative < g[n])
                {
                    cameFrom[n] = curr;
                    g[n] = gTentative;
                    f[n] = gTentative + f[n];
                    if (!open.Contains(n)) open.Add(n);
                }
            }
        }

        return new List<Direction>();
    }

    private List<Direction> Reconstruct(int[] cameFrom, int curr)
    {
        var total = new List<(int, int)> { GameManager.Map.Expand(curr) };
        while (cameFrom[curr] != -1)
        {
            curr = cameFrom[curr];
            total.Add(GameManager.Map.Expand(curr));
        }

        var ds = new List<Direction>();

        for(int i = total.Count - 1; i > 0; i--)
        {
            var deltaZ = total[i].Item1 - total[i - 1].Item1;
            if (deltaZ > 0)
            {
                ds.Add(Direction.LEFT);
                break;
            }
            if (deltaZ < 0)
            {
                ds.Add(Direction.RIGHT);
            }
            var deltaX = total[i].Item2 - total[i - 1].Item2;
            if (deltaX > 0)
            {
                ds.Add(Direction.UP);
                break;
            }
            if (deltaX < 0)
            {
                ds.Add(Direction.DOWN);
                break;
            }
        }
        return ds;
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

    protected ActionStage MoveAction(Direction d)
        => new ActionStage(tMove, (t) =>
        {
            if (t == 0) Move(d);
        });
    #endregion

    public class ActionStage
    {
        public float t;
        public Action<float> Do;

        public ActionStage(float t, Action<float> Do) { this.t = t; this.Do = Do; }
    }
}