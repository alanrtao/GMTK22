using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Items : MonoBehaviour
{
    public static Items instance;
    public enum ActivateStates {OnPickup, StartTurn, BeforeMovement, AfterMovement, BeforeAttack, AfterAttack, OnTakeDmg, EndTurn, Manual}
    public enum Character {Public, Warrior, Rogue}

    [System.Serializable]
    public class Item
    {
        public string ItemName;
        public Sprite ItemIcon;
        public string ItemIntro;
        public bool IsActivated;
        public Character CardCharacter;
    }

    public List<Item> ItemLibrary;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else 
        {
            instance = this;
        }
    }

    void Update()
    {
        
    }

    public void UseItem(string Name, ActivateStates CurrentState) 
    {
        if (Name == "Debug") 
        {
            if (CurrentState == ActivateStates.OnPickup) 
            {
                Debug.Log("Debug picked up!");
            }
            if (CurrentState == ActivateStates.BeforeAttack) 
            {
                Debug.Log("Debug activated before attack");
            }
            if (CurrentState == ActivateStates.Manual) 
            {
                Debug.Log("Debug activated manually");
            }

        }

        if (Name == "Charge Up") 
        {
            if (CurrentState == ActivateStates.AfterMovement) 
            {
                if (Player.Instance.FindClosestIdxFace(Vector3.up) == Player.OriginalNumberToIndex(1)) 
                {
                    Player.Instance.NextAtkMultiplier *=  2;
                }
            }
        }

        if (Name == "Chili Pepper") 
        {
            if (CurrentState == ActivateStates.OnPickup)
            {
                Debug.Log("Picked up " + Name);
                Player.Instance.AddToAllFaces(2);
            }
            if (CurrentState == ActivateStates.EndTurn) 
            {
                Player.Instance.HP -= 2;
            }
        }
    }
}
