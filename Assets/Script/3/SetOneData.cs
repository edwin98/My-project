using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SetOneData : MonoBehaviour
{
    public Text type_text;
    public Text x_text;
    public Text y_text;
    public Text z_text;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SetData());
    }

    IEnumerator SetData()
    {

        string LineAssetsPath = Application.dataPath + "/Json/table.json";

        UnityWebRequest manifestRequest = UnityWebRequest.Get(LineAssetsPath);
        yield return manifestRequest.SendWebRequest();

        if (manifestRequest.isNetworkError || manifestRequest.isHttpError)
        {
            Debug.Log(manifestRequest.error);
        }
        else
        {
            JsonData jsonData = JsonMapper.ToObject(manifestRequest.downloadHandler.text);
            int total = jsonData.Count-1;
            Debug.Log(total);
            Debug.Log(jsonData);
            type_text.text = jsonData[total]["type"].ToString();
            x_text.text = jsonData[total]["x"].ToString();
            y_text.text = jsonData[total]["x"].ToString();
            z_text.text = jsonData[total]["x"].ToString();
            //for (int i = 0; i < jsonData.Count; i++)
            //{
            //    if(i == total){
            //        type_text.text = jsonData[i]["type"].ToString();
            //    }

            //type_text = jsonData[total].type;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
