using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Section_detail_new : MonoBehaviour
{
    public GameObject ALL;
    public GameObject Detail;
    public GameObject SectionCamera;
    public GameObject SectionCamera_new;
    //public GameObject test;
    public GameObject center;
    GameObject Detail_one;
    public GameObject one;
    Vector3 SectionCamera_position;
    Vector3 SectionCamera_Rotation;
    int detail_nov;
    int k_nov;

    public GameObject Detail_text;
    public GameObject back_button;

    public GameObject Form;
    public Dropdown dropdown1;
    public Dropdown dropdown2;
    public Dropdown dropdown3;
    public Dropdown dropdown4;
    void Start()
    {
        SectionCamera_position = SectionCamera.transform.position;
        SectionCamera_Rotation = SectionCamera.transform.eulerAngles;
  
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if(SectionCamera.activeSelf == true)
            {
                if (Physics.Raycast(ray, out hitInfo))
                {
                    GameObject go = hitInfo.collider.gameObject;
                    string name = go.gameObject.name;
                    detail_nov = int.Parse(name.Substring(name.IndexOf("-") + 1));

                    ALL.SetActive(false);
                    Detail.SetActive(true);
                    Detail_one = Detail.transform.Find(name).gameObject;
                    back_button.SetActive(true);
                    Detail_text.SetActive(true);
                    Detail_text.transform.Find("Detail_text").GetComponent<Text>().text = name;

                    Detail_one.transform.position = one.transform.position;
                    Detail_one.transform.localScale = one.transform.localScale;
                    Detail_one.SetActive(true);
                    SectionCamera.GetComponent<Movecamera>().target = Detail_one.transform;
                    SectionCamera.transform.position = SectionCamera_position;
                    SectionCamera.transform.eulerAngles = SectionCamera_Rotation;
                    //Detail_one.transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.red;
                    //ALL.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.red;
                    ALL.SetActive(false);
                }
            }
            

        }
    }

    public void back_all()
    {
        Debug.Log(Detail_one);
        ALL.SetActive(true);
        Detail.SetActive(false);
        Detail_one.SetActive(false);
        SectionCamera.GetComponent<Movecamera>().target = center.transform;
        SectionCamera.transform.position = SectionCamera_position;
        SectionCamera.transform.eulerAngles = SectionCamera_Rotation;
        Detail_text.SetActive(false);
        back_button.SetActive(false);
        

    }

    public void color_submit()
    {
        string text1 = dropdown1.options[dropdown1.value].text;
        string text2 = dropdown2.options[dropdown2.value].text;
        string text3 = dropdown3.options[dropdown3.value].text;
        string text4 = dropdown4.options[dropdown4.value].text;
        if(text1 == "红色")
        {
            for (int i = 1; i < 25; i++)
            {
                string name = "断面-" + i;
                ALL.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.red;
                Detail.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.red;
            }
        }
        else if (text1 == "绿色")
        {
            for (int i = 1; i < 25; i++)
            {
                string name = "断面-" + i;
                ALL.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.green;
                Detail.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.green;
            }
           
        }
        else if (text1 == "黄色")
        {
            for (int i = 1; i < 25; i++)
            {
                string name = "断面-" + i;
                ALL.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.yellow;
                Detail.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.yellow;
            }
            
        }
        if (text2 == "红色")
        {
            for (int i = 25; i < 50; i++)
            {
                string name = "断面-" + i;
                ALL.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.red;
                Detail.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.red;
            }
        }
        else if (text2 == "绿色")
        {
            for (int i = 25; i < 50; i++)
            {
                string name = "断面-" + i;
                ALL.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.green;
                Detail.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.green;
            }

        }
        else if (text2 == "黄色")
        {
            for (int i = 25; i < 50; i++)
            {
                string name = "断面-" + i;
                ALL.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.yellow;
                Detail.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.yellow;
            }

        }
        if (text3 == "红色")
        {
            for (int i = 50; i < 75; i++)
            {
                string name = "断面-" + i;
                ALL.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.red;
                Detail.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.red;
            }
        }
        else if (text3 == "绿色")
        {
            for (int i = 50; i < 75; i++)
            {
                string name = "断面-" + i;
                ALL.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.green;
                Detail.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.green;
            }

        }
        else if (text3 == "黄色")
        {
            for (int i = 50; i < 75; i++)
            {
                string name = "断面-" + i;
                ALL.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.yellow;
                Detail.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.yellow;
            }

        }
        if (text4 == "红色")
        {
            for (int i = 75; i < 101; i++)
            {
                string name = "断面-" + i;
                ALL.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.red;
                Detail.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.red;
            }
        }
        else if (text4 == "绿色")
        {
            for (int i = 75; i < 101; i++)
            {
                string name = "断面-" + i;
                ALL.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.green;
                Detail.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.green;
            }

        }
        else if (text4 == "黄色")
        {
            for (int i = 75; i < 101; i++)
            {
                string name = "断面-" + i;
                ALL.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.yellow;
                Detail.transform.Find(name).transform.Find("钻孔-3").GetComponent<MeshRenderer>().material.color = Color.yellow;
            }

        }
        Debug.Log(text1);
        Debug.Log(text2);
        Debug.Log(text3);
        Debug.Log(text4);
    }

    public void close_color()
    {
        Form.SetActive(false);
        SectionCamera.SetActive(true);
        SectionCamera_new.SetActive(false);
    }
    public void choose_color()
    {
        Form.SetActive(true);
        
        SectionCamera_new.SetActive(true);
        SectionCamera.SetActive(false);
    }
}
