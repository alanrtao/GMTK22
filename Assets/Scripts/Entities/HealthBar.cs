using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public BaseRollable MyMaster;
    public bool FollowMaster;
    public Vector3 InitialScale;
    public Image MyBar;

    void Start()
    {
        InitialScale = MyBar.rectTransform.localScale;
    }

    void Update()
    {
        // Debug.Log(MyMaster.HP / MyMaster.MAX_HP);
        MyBar.rectTransform.localScale = new Vector2((float)MyMaster.HP / (float)MyMaster.MAX_HP * InitialScale.x, InitialScale.y);
    }
}
