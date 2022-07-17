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
        var rBuf = m_RollableRoot.localRotation;
        var vCamBuf = vCam.m_Lens.OrthographicSize;

        List<ActionStage> stages = new List<ActionStage>()
        {
            new ActionStage(0.25f, (t) =>
            {
                vCam.m_Lens.OrthographicSize = vCamBuf - (1 - (1 - t) * (1 - t)) * 0.2f;
                transform.position = pBuf + Vector3.up * (1 - (1 - t) * (1 - t));
                // m_RollableRoot.localRotation = Quaternion.AngleAxis(MathExtensions.EaseInOutCubic(t) * 360, Vector3.up) * rBuf;
            }),
            new ActionStage(0.25f, (t) =>
            {
                if (t == 1)
                {
                    vCam.m_Lens.OrthographicSize = vCamBuf;
                    transform.position = pBuf;
                    m_RollableRoot.localRotation = Quaternion.AngleAxis(180, Vector3.right) * rBuf;
                    UpdateFaces(this);

                    var upNum = FindClosestCurrFace(Vector3.up);

                    GameManager.Map
                        .Adjacent(transform.position.z, transform.position.x)
                        .Select(n => (BaseEnemy)GameManager.Map.Obstacle(n.Item1, n.Item2))
                        .Where(e => e != null)
                        .ForEach(e => Attack(upNum, e, false));

                    collisionSource.GenerateImpulse(1 + Mathf.Log(upNum));

                    return;
                }

                Debug.Log(t * t);
                RollableRoot.rotation = Quaternion.AngleAxis(t * t * 180, Vector3.right) * rBuf;
                transform.position = pBuf + Vector3.up * (1 - t * t);
                vCam.m_Lens.OrthographicSize = vCamBuf - (1 - t * t) * 0.2f;
            }),
            new ActionStage(ultimateVfx.Count * Time.maximumDeltaTime, (t) => {
                if (t == 0) ultimateVfx.gameObject.SetActive(true);
                if (t == 1) wuso = wuBuf;
            })
        };
        StartCoroutine(UltimateAnimation(stages));

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
