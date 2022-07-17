using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RangedEnemy : BaseEnemy
{
    [SerializeField] int kAttack = 3;
    [SerializeField] int kSteps = 2;

    public override void UpdateStages(int i)
    {
        stages = new List<ActionStage>();
        if (i == 0)
        {
            stages.Add(ShiftCamera(Player.Instance, this));
        }
        else
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
        var idx = moves.Item1.FindIndex(p => p.Item1 == ez || p.Item2 == ex);
        if (idx > 0) idx += 1;

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
            stages.Add(AttackAction(Player.Instance, kAttack));
            // ShowAlert(false);
            //}
        }
        else if (idx > 0)
        {
            // walk up to the player
            stages.Add(MovesAction(moves.Item2.Take(Mathf.Min(idx, kSteps)).ToList()));
            //alertBuffer = true;
            //ShowAlert(true);
            // stages.Add(AttackAction(Player.Instance, kAttack));
            stages.Add(AttackAction(Player.Instance, kAttack));
        }
        else
        {
            // walk the full path
            // alertBuffer = false;
            // ShowAlert(false);
            stages.Add(MovesAction(moves.Item2));
        }

        if (i == GameManager.Pool.Pool.Count - 1)
            stages.Add(ShiftCamera(this, Player.Instance));
    }


    [SerializeField] GameObject projectile;
    GameObject bullet;
    protected override ActionStage AttackAction(BaseRollable target, int attack, bool renderAnimation = true) => new ActionStage(0.5f, t =>
    {
        if (t == 0)
        {
            bullet = Instantiate(projectile);
            bullet.transform.position = RollableRoot.position + Vector3.up * 0.5f;
        }
        if (t == 1)
        {
            Destroy(transform.GetChild(0).gameObject);
            target.ChangeHPBy(kAttack);
        }

        float parabolic = 5 * (1 - (1 - 2 * t) * (1 - 2 * t));

        bullet.transform.position = new Vector3(
            Mathf.Lerp(transform.position.x, target.transform.position.x, t),
            Mathf.Lerp(transform.position.y, target.transform.position.y, t) + parabolic,
            Mathf.Lerp(transform.position.z, target.transform.position.z, t));
    });

    protected override float Heuristic(int ez, int ex, int z, int x)
    {
        float temp = base.Heuristic(ez, ex, z, x);

        // prefer diagonal
        if (ez == z || ex == x) temp *= 0.5f;
        return temp;
    }

    protected override (List<(int, int)>, List<Direction>) Fallback((int, int)[,] cameFrom, float[,] f, int sz, int sx)
    {
        var tiers = new (int, int)[3]
        {
            (-1, -1), // tier 1: can shoot
            (-1, -1), // tier 2: can shoot next turn if player stay on same axis
            (-1, -1), // tier 3: other
        };

        var tiersf = new float[3]
        {
            float.MaxValue,
            float.MaxValue,
            float.MaxValue
        };

        for(int i = 0; i < GameManager.Map.WIDTH; i++)
        {
            for(int j = 0; j < GameManager.Map.HEIGHT; j++)
            {
                if (cameFrom[i, j].Item1 != -1)
                {
                    if (sz == i || sx == i)
                    {
                        // if major axis can be reached within this turn, do it
                        var path = Reconstruct(cameFrom, (i, j));
                        if (path.Item2.Count < kSteps)
                        {
                            if (path.Item2.Count < tiersf[0])
                            {
                                tiersf[0] = path.Item2.Count + 0.1f;
                                tiers[0] = (i, j);
                            }

                        } else if (f[i, j] < tiersf[1])
                        {
                            tiersf[1] = f[i, j];
                            tiers[1] = (i, j);
                        }
                    } else if (f[i, j] < tiersf[2])
                    {
                        tiersf[2] = f[i, j];
                        tiers[2] = (i, j);
                    }
                }
            }
        }

        for(int i = 0; i < tiers.Length; i++)
        {
            if (tiers[i].Item1 != -1) return Reconstruct(cameFrom, tiers[i]);
        }

        return base.Fallback(cameFrom, f, sz, sx);
    }
}
