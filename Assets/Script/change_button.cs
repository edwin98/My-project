using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class change_button : MonoBehaviour
{
    public GameObject[] display_obj;
    public GameObject[] hidden_obj;


    public Button[] menubtns;
    public Button menubtn;


    public TMP_Text[] hidden_btns;
    public TMP_Text dis_btns;
    //string status = "on";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void change()
    {
        //if (status=="on")
        //{
        menubtn.transform.Find("Text").GetComponent<Text>().color = new Color32(14, 226, 218, 255);
        for (int i = 0; i < menubtns.Length; i++)
        {
            menubtns[i].transform.Find("Text").GetComponent<Text>().color = new Color32(255, 255, 255, 255);

        }
        for (int i = 0; i < display_obj.Length; i++)
        {
            display_obj[i].SetActive(true);

        }
        for (int i = 0; i < hidden_obj.Length; i++)
        {
            hidden_obj[i].SetActive(false);

        }

        //status = "off";
        //}
        //else if (status == "off")
        //{
        //    display_obj.SetActive(false);
        //    hidden_obj.SetActive(true);
        //    status = "on";
        //}

    }

    public void change4()
    {
        //if (status=="on")
        //{
        //menubtn.transform.Find("Text").GetComponent<Text>().color = new Color32(14, 226, 218, 255);
        //for (int i = 0; i < menubtns.Length; i++)
        //{
        //    menubtns[i].transform.Find("Text").GetComponent<Text>().color = new Color32(255, 255, 255, 255);

        //}
        for (int i = 0; i < display_obj.Length; i++)
        {
            display_obj[i].SetActive(true);

        }
        for (int i = 0; i < hidden_obj.Length; i++)
        {
            hidden_obj[i].SetActive(false);

        }
        for (int i = 0; i < hidden_btns.Length; i++)
        {
            hidden_btns[i].color = new Color32(255, 255, 255, 255);

        }
        dis_btns.color = new Color32(14, 226, 218, 255);

    }

    public void change2()
    {
        //display_obj.SetActive(false);

        for (int i = 0; i < display_obj.Length; i++)
        {
            display_obj[i].SetActive(false);

        }

    }

    public void changedetail()
    {
        //if (status=="on")
        //{
        menubtn.transform.Find("Text").GetComponent<Text>().color = new Color32(14, 226, 218, 255);
        for (int i = 0; i < menubtns.Length; i++)
        {
            menubtns[i].transform.Find("Text").GetComponent<Text>().color = new Color32(255, 255, 255, 255);

        }
        for (int i = 0; i < display_obj.Length; i++)
        {
            display_obj[i].SetActive(true);

        }
        for (int i = 0; i < hidden_obj.Length; i++)
        {
            hidden_obj[i].SetActive(false);

        }


    }
}
