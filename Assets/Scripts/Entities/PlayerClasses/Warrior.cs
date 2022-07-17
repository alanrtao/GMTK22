using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Warrior : Player
{

    public override void Ultimate()
    {
        if (wuso >= 24) wuso -= 24;
        else return;

        blocked = true;

        var wuBuf = wuso;

        var pBuf = transform.position;
        var rBuf = transform.rotation;

        List<ActionStage> stages = new List<ActionStage>()
        {
            new ActionStage(0.5f, (t) =>
            {
                transform.position = pBuf + Vector3.up * (1 - (1 - t) * (1 - t));
                RollableRoot.rotation = Quaternion.AngleAxis(MathExtensions.EaseInOutCubic(t) * 360, Vector3.up) * rBuf;
            }),
            new ActionStage(0.5f, (t) =>
            {
                if (t == 1)
                {
                    transform.position = pBuf;
                    RollableRoot.rotation = Quaternion.AngleAxis(180, Vector3.right) * rBuf;
                    UpdateFaces(this);

                    var upNum = FindClosestCurrFace(Vector3.up);

                    GameManager.Map
                        .Adjacent(transform.position.z, transform.position.x)
                        .Select(n => (BaseEnemy)GameManager.Map.Obstacle(n))
                        .Where(e => e != null)
                        .ForEach(e => Attack(upNum, e));

                    return;
                }
                RollableRoot.rotation = Quaternion.AngleAxis(t * t * 180, Vector3.right) * rBuf;
                transform.position = pBuf + Vector3.up * (1 - t * t);
            }),
            new ActionStage(ultimateVfx.Count * Time.maximumDeltaTime, (t) => {
                if (t == 0) ultimateVfx.gameObject.SetActive(true);
            })
        };
        StartCoroutine(UltimateAnimation(stages));

        wuso = wuBuf;
    }

    public IEnumerator UltimateAnimation(List<ActionStage> stages)
    {
        blocked = true;

        foreach (var s in stages)
        {
            float t = 0;
            while (t < s.t)
            {
                s.Do(t);
                t += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            s.Do(1);
            yield return new WaitForEndOfFrame();
        }

        blocked = false;
    }
}
