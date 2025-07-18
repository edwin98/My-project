using LitJson;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SetDropdown : MonoBehaviour
{
    public Dropdown dropdown;
    public string[] optionsArray; 

    JsonData api_table;

    public TMP_Text id_text;
    public TMP_Text type_text;
    public TMP_Text x_text;
    public TMP_Text y_text;
    public TMP_Text z_text;

    public GameObject[] hidden_obj;
    public GameObject dis_obj;

    public TMP_Text[] hidden_btns;
    public TMP_Text dis_btns;

    JsonData apiData;
    // Start is called before the first frame update
    void Start()
    {
        string currentData = PlayerPrefs.GetString("MyTableData", "");

        if (currentData == "")
        {
            StartCoroutine(LoadTable());
        }
        else
        {

            JArray jsonArray = JArray.Parse(currentData);

            string updatedData = jsonArray.ToString();


            Deal_table(JsonMapper.ToObject(updatedData), updatedData);

        }

        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        //dropdown.value = -1;
    }

    IEnumerator LoadTable()
    {

        string LineAssetsPath = Application.streamingAssetsPath + "/Json/table.json";

        UnityWebRequest manifestRequest = UnityWebRequest.Get(LineAssetsPath);
        yield return manifestRequest.SendWebRequest();

        if (manifestRequest.isNetworkError || manifestRequest.isHttpError)
        {
            Debug.Log(manifestRequest.error);
        }
        else
        {
            string jsonData = manifestRequest.downloadHandler.text;
            Debug.Log(jsonData);
            PlayerPrefs.SetString("MyTableData", jsonData);
            PlayerPrefs.Save();
            string loadedJson = PlayerPrefs.GetString("MyTableData");
            api_table = JsonMapper.ToObject(loadedJson);

            //Debug.Log(api_table);
            Deal_table(api_table, jsonData);

        }
        //yield return new WaitForSeconds(5);

    }

    public void Deal_table(JsonData apitable, string stringtable)
    {

        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        Debug.Log(apitable);
        Debug.Log(apitable.Count);
        for (int i = 0; i < apitable.Count; i++)
        {

            options.Add(new Dropdown.OptionData(apitable[i]["id"].ToString()));


        }
        dropdown.ClearOptions();

        dropdown.AddOptions(options);
        dropdown.value = -1;
    }
    public void changetab(string buttontext)
    {
        string currentData = PlayerPrefs.GetString("MyTableData");
        JsonData data = JsonMapper.ToObject(currentData);
        Debug.Log(buttontext);
        Debug.Log(data);
        for (int i = 0; i < data.Count; i++)
        {
            Debug.Log(data[i]["id"]);
            Debug.Log(data[i]["type"]);
            Debug.Log(data[i]["x"]);
            if (data[i]["id"].ToString() == buttontext)
            {
                id_text.text = data[i]["id"].ToString();
                type_text.text = data[i]["type"].ToString();
                x_text.text = data[i]["x"].ToString();
                y_text.text = data[i]["y"].ToString();
                z_text.text = data[i]["z"].ToString();
            }
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

        dis_obj.SetActive(true);
    }
    void OnDropdownValueChanged(int index)
    {
        // ��ȡDropdownѡ�е�ѡ��
        string selectedOption = dropdown.options[index].text;

        // ִ���ض��Ĳ���
        HandleSelectedOption(selectedOption);
    }

    void HandleSelectedOption(string option)
    {
        changetab(option);

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
