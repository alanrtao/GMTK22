using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItems : MonoBehaviour
{

    public List<Items.Item> MyItemList;

    private void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PickUpItem(3);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) 
        {
            ActivateSingleItem(Items.ActivateStates.Manual, 1);
        }
    }

    public void ActivateAllItems(Items.ActivateStates CurrentState) 
    {
        //Skip Item 0
        for (int i = 1; i < MyItemList.Count; i++) 
        {
            Items.instance.UseItem(MyItemList[i].ItemName, CurrentState);
        }
    }

    public void ActivateSingleItem(Items.ActivateStates CurrentState, int Index)
    {
        Items.instance.UseItem(MyItemList[Index].ItemName, CurrentState);
    }

    public void PickUpItem(int LibraryIndex) 
    {
        if (MyItemList.Count < 11)
        {
            MyItemList.Add(Items.instance.ItemLibrary[LibraryIndex]);
            ActivateSingleItem(Items.ActivateStates.OnPickup, MyItemList.Count - 1);
        }
        else
        {
            Debug.Log("Failed to add items, there are already 10!");
        }
    }
}
