using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public bool isCriticalBar;

    public BaseRollable MyMaster;
    public bool FollowMaster;
    public Vector3 InitialScale;
    public Image MyBar;
    public Text Number;

    public Text BlinkTimer;

    void Start()
    {

            InitialScale = MyBar.rectTransform.localScale;
    }

    void Update()
    {
        if (isCriticalBar && MyMaster is Player)
        {
            Color Glow;
            Glow = new Color(1, 1, 1, 0) * Mathf.Sin(Time.realtimeSinceStartup * 20) * 0.2f;
            if ((MyMaster as Player).Wuso >= 20)
            {
                Number.text = "Ultimate Rdy!";
                MyBar.color = Color.red + Glow;
            }
            else 
            {
                Number.text = (MyMaster as Player).Wuso + " / " + 20;
                MyBar.color = Color.blue;
            }
            MyBar.rectTransform.localScale = new Vector2((float)(MyMaster as Player).Wuso / 20f * InitialScale.x, InitialScale.y);

        }
        else 
        {
            Number.text = MyMaster.HP + " / " + MyMaster.MAX_HP;
            MyBar.rectTransform.localScale = new Vector2((float)MyMaster.HP / (float)MyMaster.MAX_HP * InitialScale.x, InitialScale.y);
        }
    }
}
