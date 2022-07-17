using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Items : MonoBehaviour
{
    public enum ActivteStates {OnPickup, StartTurn, BeforeMovement, AfterMovement, BeforeAttack, AfterAttack, OnTakeDmg, EndTurn}

    public struct Item
    {
        string ItemName;
        Sprite ItemIcon;
        bool IsActivated;
    }

    public List<Item> ItemLibrary;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void UseItem(string Name, ActivteStates CurrentState) 
    {
        if (Name == "Debug Item") 
        {
            if (CurrentState == ActivteStates.BeforeAttack) 
            {
                Debug.Log("Item activated before attack");
            }
        }

        if (Name == "Charging Blow") 
        {
            if (CurrentState == ActivteStates.AfterMovement) 
            {
                //ToBeChanged
                if (1 == 1) 
                {
                    Player.Instance.NextAtkMultiplier *=  2;
                }
            }
        }

        if (Name == "Chili Pepper") 
        {
            if (CurrentState == ActivteStates.OnPickup)
            {
                Debug.Log("Picked up " + Name);
                //ToBeChanged
                //Player.Instance.NextAtkDmg += 2;
            }
            if (CurrentState == ActivteStates.EndTurn) 
            {
                Player.Instance.HP -= 2;
            }
        }
    }
}
