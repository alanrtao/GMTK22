using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Warrior : Player
{
    public override void TurnEnd()
    {
        // first shield then -hp
        Debug.Log("Turn end as warrior");
        shield = FindClosestCurrFace(Vector3.down);
        base.TurnEnd();
    }

    public override void Ultimate()
    {
        if (Wuso >= 20 && stamina > 0) Wuso -= 20;
        else return;

        blocked = true;

        var wuBuf = Wuso;

        var pBuf = transform.position;
        var rBuf = m_RollableRoot.localRotation;
        var vCamBuf = vCam.m_Lens.OrthographicSize;

        List<ActionStage> stages = new List<ActionStage>()
        {
            new ActionStage(0.5f, (t) =>
            {
                vCam.m_Lens.OrthographicSize = vCamBuf - (1 - (1 - t) * (1 - t)) * 0.2f;
                transform.position = pBuf + Vector3.up * (1 - (1 - t) * (1 - t));
                // m_RollableRoot.localRotation = Quaternion.AngleAxis(MathExtensions.EaseInOutCubic(t) * 360, Vector3.up) * rBuf;
            }),
            new ActionStage(0.4f, (t) =>
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
                        .ForEach(e => Attack(upNum, e, false, false));

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
                if (t == 1) {
                    if (stamina > 0) stamina -= 1;
                }
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
