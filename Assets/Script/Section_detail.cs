using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Section_detail : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject ALL;
    public GameObject Detail;
    public GameObject SectionCamera;
    //public GameObject test;
    public GameObject center;
    GameObject Detail_one;
    public GameObject one;
    Vector3 SectionCamera_position;
    Vector3 SectionCamera_Rotation;
    int detail_nov;

    public GameObject[] hole;
    Vector3[] hole_position;
    int k_nov;

    public GameObject Detail_text;
    public GameObject back_button;
    void Start()
    {
        SectionCamera_position = SectionCamera.transform.position;
        SectionCamera_Rotation = SectionCamera.transform.eulerAngles;
        Debug.Log(hole.Length);
        for (int i = 1; i < hole.Length+1; i++)
        {
            hole_position[i] = hole[i].transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            //Debug.Log(ray);

            if (Physics.Raycast(ray, out hitInfo))
            {
                GameObject go = hitInfo.collider.gameObject;

                string name = go.gameObject.name;
                //string name = hitInfo.transform.name;
                Debug.Log(name);
                //name.Substring(0, name.LastIndexOf("-"));
                detail_nov = int.Parse(name.Substring(name.IndexOf("-") + 1));
          
                ALL.SetActive(false);
                Detail.SetActive(true);
                Detail_one = Detail.transform.Find(name).gameObject;
                back_button.SetActive(true);
                Detail_text.SetActive(true);
                Detail_text.transform.Find("Detail_text").GetComponent<Text>().text = name;
                //Detail_one.transform.DOMove(one.transform.position, 0.1f);
                //Detail_one.transform.DOScale(new Vector3(11.84514f, 11.84514f, 11.84514f), 0.1f);
                Detail_one.transform.position = one.transform.position;
                Detail_one.transform.localScale = one.transform.localScale;
                Detail_one.SetActive(true);
                SectionCamera.GetComponent<Movecamera>().target = Detail_one.transform;
                SectionCamera.transform.position = SectionCamera_position;
                SectionCamera.transform.eulerAngles = SectionCamera_Rotation;
                for (int i = 1; i < hole.Length+1; i++)
                {
                    Debug.Log(detail_nov);
                    detail_nov = 101 - detail_nov;
                    Debug.Log(detail_nov);
                    k_nov = detail_nov * 7 -8 + i;
                    //Debug.Log(hole[i]);
                    string hole_name = "钻孔-" + k_nov;
                    //Debug.Log(hole_name);

                    Detail.transform.Find(hole_name).transform.position= hole[i].transform.position;
                    Detail.transform.Find(hole_name).transform.eulerAngles = hole[i].transform.eulerAngles;
                    Detail.transform.Find(hole_name).transform.localScale = hole[i].transform.localScale;
                    //Detail.transform.Find(hole_name).transform.DOMove(hole[i].transform.position, 0.1f);
                    //Detail.transform.Find(hole_name).transform.DOScale(new Vector3(11.83598f, 11.83598f, 11.83598f), 0.1f);
                    Detail.transform.Find(hole_name).gameObject.SetActive(true);
                }
                
                
                ALL.SetActive(false);

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
        for (int i = 1; i < hole.Length+1; i++)
        {
            int k_nov = detail_nov * 7 - 8 + i;
            string hole_name = "钻孔-" + k_nov;

            Detail.transform.Find(hole_name).gameObject.SetActive(false);
        }
        
    }
}
