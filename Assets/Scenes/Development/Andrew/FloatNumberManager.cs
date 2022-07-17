using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatNumberManager : MonoBehaviour
{
    
    //public GameObject floatingNumber;

    //display the type of the damage in this order, (1)heal, (2)armor, (3)damage with block, (4)damage without block
    public List<GameObject> numberType;

    //temp object, delete itself after create
    private GameObject clone;


    //the target object that the number will display at
    public GameObject targetObject;


    
    //public int damageType;

    public int inputValue;

    public int damageType;

    void Start()
    {
        inputValue = 114514;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            //Debug.Log("r shift pressed");

            //Debug.Log(numberType[3]);
            displayNumber(targetObject, damageType, inputValue);
            /*
            clone = Instantiate(floatingNumber, transform.position+new Vector3(-8, 8,-5), Quaternion.Euler(new Vector3(35,45,0)));
            Debug.Log("first "+clone.transform.GetChild(0).GetComponent<TextMeshPro>().text);
            clone.transform.GetChild(0).GetComponent<TextMeshPro>().text = inputValue.ToString();
            Debug.Log("after " + clone.transform.GetChild(0).GetComponent<TextMeshPro>().text);
            Destroy(clone, 1f);
            */
        } 
    }


    public void displayNumber(GameObject target, int type, int input)
    {
        //(1)heal, (2)armor, (3)damage with block, (4)damage without block
        switch(type)
        {
            case 1:
                //Debug.Log("1");
                clone = Instantiate(numberType[0], target.transform.position + new Vector3(0, 5, 0), Quaternion.identity);
            
                clone.transform.GetChild(0).GetComponent<TextMeshPro>().text = input.ToString();

                Destroy(clone, 1f);
                break;
            case 2:
                //Debug.Log("2");
                clone = Instantiate(numberType[1], target.transform.position + new Vector3(0,5,0), Quaternion.identity);

                clone.transform.GetChild(0).GetComponent<TextMeshPro>().text = input.ToString();

                Destroy(clone, 1f);
                break;
            case 3:
                //Debug.Log("3");
                clone = Instantiate(numberType[2], target.transform.position + new Vector3(0, 5, 0), Quaternion.identity);

                clone.transform.GetChild(0).GetComponent<TextMeshPro>().text = input.ToString();

                Destroy(clone, 1f);
                break;
            case 4:
                //Debug.Log("4");
                clone = Instantiate(numberType[3], target.transform.position + new Vector3(0, 5, 0), Quaternion.identity);

                clone.transform.GetChild(0).GetComponent<TextMeshPro>().text = input.ToString();

                Destroy(clone, 1f);
                break;
        }

    }
}
