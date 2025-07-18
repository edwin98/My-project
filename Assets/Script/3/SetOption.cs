using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using LitJson;
using UnityEngine.Networking;

public class SetOption : MonoBehaviour
{
    public Dropdown dropdown;
    public string[] optionsArray;

    JsonData api_table;

    public Text type_text;
    public Text x_text;
    public Text y_text;
    public Text z_text;

    public GameObject hidden_obj;
    public GameObject dis_obj;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadTable());
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
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
        for (int i = 0; i < apitable.Count; i++)
        {

            options.Add(new Dropdown.OptionData(apitable[i]["id"].ToString()));

        
        }
        dropdown.ClearOptions();

        dropdown.AddOptions(options);
        //dropdown.value = -1;
    }
    public void changetab(string buttontext)
    {
        string currentData = PlayerPrefs.GetString("MyTableData");
        JsonData data = JsonMapper.ToObject(currentData);
        for (int i = 0; i < data.Count; i++)
        {
            if (data[i]["id"].ToString() == buttontext)
            {
                type_text.GetComponent<Text>().text = data[i]["type"].ToString();
                x_text.GetComponent<Text>().text = data[i]["x"].ToString();
                y_text.GetComponent<Text>().text = data[i]["y"].ToString();
                z_text.GetComponent<Text>().text = data[i]["z"].ToString();
            }
            //Debug.Log("Clicked Button: " + data[i]["type"]);
        }
        hidden_obj.SetActive(false);
        dis_obj.SetActive(true);
    }
    void OnDropdownValueChanged(int index)
    {
        string selectedOption = dropdown.options[index].text;

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
