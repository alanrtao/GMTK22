using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SideStatus : MonoBehaviour
{
    public List<Image> Images;
    [SerializeField]
    private List<Sprite> DieSprites_Warrior;
    [SerializeField]
    private List<Sprite> DieSprites_Rogue;

    void Start()
    {
        Refresh();
    }

    void Update()
    {
        Refresh();
    }

    void Refresh() 
    {
        for (int i = 0; i < Images.Count; i++)
        {
            if (Player.Instance is Warrior)
            {
                Images[i].sprite = DieSprites_Warrior[i];
            }
            else if (Player.Instance is Rogue)
            {
                Images[i].sprite = DieSprites_Rogue[i];
            }
            else
            {
                Debug.Log("Unknown Character");
            }
        }
    }
}
