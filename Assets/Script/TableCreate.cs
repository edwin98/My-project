using UnityEngine;
using UnityEngine.UI;
using System.Collections;
//using System.IO;
using LitJson;
using MySql.Data.MySqlClient;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using TMPro;

[System.Serializable]
public class MyData
{
    public int id;
    public string type;
    public float x;
    public float y;
    public float z;
}
public class TableCreate : MonoBehaviour
{
    public GameObject table_parent;
    public GameObject Row_Prefab;//表头预设

    public GameObject add_table;
    public Text text_x;
    public Text text_y;
    public Text text_z;
    public Text text_type;
    public Text text_data3;
    public Text text_data4;
    public Text text_data5;
    //public Text text_data6;
    JsonData api_table;
    JObject api_json;
    string FileUrl;
    string history_data;

    private int total_id;

    JsonData apiData;

    public Text type_text;
    public Text x_text;
    public Text y_text;
    public Text z_text;

    public GameObject hidden_obj;
    public GameObject dis_obj;

    public Dropdown dropdown;
    //public Dropdown dropdown; // 在 Inspector 中关联 Dropdown
    public string[] optionsArray; // 在 Inspector 中配置选项数组

    private int tunnel;
    public TMP_Text tunnel_text;

    [System.Obsolete]
    void Start()
    {
        StartCoroutine(LoadTable());
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        
        if(PlayerPrefs.HasKey("Selecttunnel"))
        {
            tunnel = PlayerPrefs.GetInt("Selecttunnel");
            Debug.Log(tunnel);
            tunnel_text.text = tunnel.ToString();
        }
        
    }

    public void Add_btn()
    {
        add_table.SetActive(true);
    }

    public void Close_add_btn()
    {
        add_table.SetActive(false);
    }

    
    public void Tijiao_btn0()
    {
        total_id++;
        //var opts = new Dictionary<string, string>();
        //opts.Add("id", total_id.ToString());
        //opts.Add("x", text_x.GetComponent<Text>().text);
        //opts.Add("y", text_y.GetComponent<Text>().text);
        //opts.Add("z", text_z.GetComponent<Text>().text);
        //opts.Add("type", text_type.GetComponent<Text>().text);
        //opts.Add("data3", text_data3.GetComponent<Text>().text);
        //opts.Add("data4", text_data4.GetComponent<Text>().text);
        //opts.Add("data5", text_data5.GetComponent<Text>().text);
        //Debug.Log(text_type.GetComponent<Text>().text);
        //var list1 = api_table.ToJson();
        //Debug.Log(list1);

        

        var itemToAdd = new JObject();
        itemToAdd["id"] = total_id.ToString();
        itemToAdd["x"] = text_x.GetComponent<Text>().text;
        itemToAdd["y"] = text_y.GetComponent<Text>().text;
        itemToAdd["z"] = text_z.GetComponent<Text>().text;
        itemToAdd["type"] = text_type.GetComponent<Text>().text;

        //array.Add(itemToAdd);
        Debug.Log("--itemToAdd--");
        Debug.Log(itemToAdd);
        string jsonString = "{" +
        $"\"id\": \"{itemToAdd["id"]}\"," +
        $"\"x\": \"{itemToAdd["x"]}\"," +
        $"\"y\": \"{itemToAdd["y"]}\"," +
        $"\"z\": \"{itemToAdd["z"]}\"," +
        $"\"type\": \"{itemToAdd["type"]}\"" +
        "}";
        Debug.Log(jsonString);
        add_table.SetActive(false);
        int childCount = table_parent.transform.childCount;
        Debug.Log(childCount);
        for (int i = 0; i < childCount; i++)
        {
            Destroy(table_parent.transform.GetChild(i).gameObject);
        }
        //PlayerPrefs.SetString("MyTableData", jsonString);
        //PlayerPrefs.Save();
        Debug.Log(childCount);

        string currentData = PlayerPrefs.GetString("MyTableData", "");

        // 如果这是第一次存储数据，currentData 为 ""
        if (currentData == "")
        {
            // 直接存储新数据
            PlayerPrefs.SetString("MyTableData", jsonString);
        }
        else
        {
            // 拼接新数据
            currentData += "," + jsonString;

            // 存储拼接后的数据
            PlayerPrefs.SetString("MyTableData", currentData);
        }
        string loadedJson = PlayerPrefs.GetString("MyTableData");
        Debug.Log(loadedJson);
        api_table = JsonMapper.ToObject(loadedJson);
        apiData.Add(api_table);
        Debug.Log(apiData);
        Debug.Log(api_table);
        Debug.Log(apiData.Count);
    
        Deal_table(apiData, loadedJson);
        //Table_json();

    }

    public void Tijiao_btn()
    {
        total_id++;
        // 创建一个新的JSON对象
        var itemToAdd = new JObject();
        itemToAdd["id"] = total_id.ToString();
        itemToAdd["x"] = text_x.text;
        itemToAdd["y"] = text_y.text;
        itemToAdd["z"] = text_z.text;
        itemToAdd["type"] = text_type.text;

        // 读取现有的数据
        string currentData = PlayerPrefs.GetString("MyTableData", "[]");
        // 解析现有的JSON数据为数组
        JArray jsonArray = JArray.Parse(currentData);
        // 将新的数据项添加到数组中
        jsonArray.Add(itemToAdd);
        // 将更新后的数组转换回字符串
        string updatedData = jsonArray.ToString();
        // 存储更新后的数据
        PlayerPrefs.SetString("MyTableData", updatedData);

        add_table.SetActive(false);

        // 删除旧的表格行
        int childCount = table_parent.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(table_parent.transform.GetChild(i).gameObject);
        }

        Deal_table(JsonMapper.ToObject(updatedData), updatedData);
    }
    public void Clear_btn()
    {

        //PlayerPrefs.SetString("MyTableData", jsonString);
        PlayerPrefs.DeleteKey("MyTableData");
        PlayerPrefs.Save();
        int childCount = table_parent.transform.childCount;
        //Debug.Log(childCount);
        for (int i = 0; i < childCount; i++)
        {
            //Debug.Log(table_parent.transform.GetChild(i));
            Destroy(table_parent.transform.GetChild(i).gameObject);
        }
        total_id = 0;
        apiData.Clear();
        //apiData = null;
        //string jsonString = "[]";

        //FileUrl = Application.dataPath + "/Json/table.json";
        //File.WriteAllText(FileUrl, jsonString);
        //add_table.SetActive(false);
        //int childCount = table_parent.transform.childCount;
        ////Debug.Log(childCount);
        //for (int i = 0; i < childCount; i++)
        //{
        //    Debug.Log(table_parent.transform.GetChild(i));
        //    Destroy(table_parent.transform.GetChild(i).gameObject);
        //}
        //Table_json();

    }

    IEnumerator LoadTable()
    {
        
            
            string LineAssetsPath = Application.streamingAssetsPath + "/Json/table.json";
        Debug.Log(LineAssetsPath);
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
            apiData=api_table;
            //Debug.Log(api_table);
            Deal_table(api_table,jsonData);
            //Debug.Log(api_table.Count);
            //total_id = api_table.Count;
            //for (int i = 0; i < api_table.Count; i++)
            //{
            //    Debug.Log(api_table[i]["type"]);
            //    GameObject table = GameObject.Find("钻孔信息/Tab/Panel/Table");
            //    GameObject row = GameObject.Instantiate(Row_Prefab, table.transform.position, table.transform.rotation) as GameObject;
            //    row.name = "row" + (i + 1);
            //    row.transform.SetParent(table.transform);
            //    row.transform.localScale = Vector3.one;
            //    if (i % 2 == 0)
            //    {
            //        row.transform.Find("Cell0").GetComponent<Image>().color = Color.white;
            //        row.transform.Find("Cell1").GetComponent<Image>().color = Color.white;
            //        row.transform.Find("Cell2").GetComponent<Image>().color = Color.white;
            //        row.transform.Find("Cell3").GetComponent<Image>().color = Color.white;
            //        row.transform.Find("Cell4").GetComponent<Image>().color = Color.white;
            //        //row.transform.Find("Cell5").GetComponent<Image>().color = Color.white;
            //        //row.transform.Find("Cell6").GetComponent<Image>().color = new Color(255, 255, 255); 
            //    }
            //    row.transform.Find("Cell0").transform.Find("Text").gameObject.SetActive(false);
            //    row.transform.Find("Cell0").transform.Find("Button").gameObject.SetActive(true);

            //    row.transform.Find("Cell0").transform.Find("Button").transform.Find("Text").GetComponent<Text>().text = api_table[i]["id"].ToString();
            //    row.transform.Find("Cell1").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["type"];
            //    row.transform.Find("Cell2").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["x"];
            //    row.transform.Find("Cell3").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["y"];
            //    row.transform.Find("Cell4").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["z"];
            //    //row.transform.Find("Cell5").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["data4"];
            //    //row.transform.Find("Cell6").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["data5"];
            //}
        }
            //yield return new WaitForSeconds(5);
        
    }

    public void Deal_table(JsonData apitable,string stringtable)
    {
        Debug.Log(apitable.Count);
        total_id = apitable.Count;
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        for (int i = 0; i < apitable.Count; i++)
        {
            
            options.Add(new Dropdown.OptionData(apitable[i]["id"].ToString()));
            //Debug.Log("---230---");
            //Debug.Log(apitable[i]["id"]);
            //Debug.Log(apitable[i]["type"]);
            GameObject table = GameObject.Find("钻孔信息/Tab/Panel/Table");
            GameObject row = GameObject.Instantiate(Row_Prefab, table.transform.position, table.transform.rotation) as GameObject;
            row.name = "row" + (i + 1);
            row.transform.SetParent(table.transform);
            row.transform.localScale = Vector3.one;
            if (i % 2 == 0)
            {
                row.transform.Find("Cell0").GetComponent<Image>().color = Color.white;
                row.transform.Find("Cell1").GetComponent<Image>().color = Color.white;
                row.transform.Find("Cell2").GetComponent<Image>().color = Color.white;
                row.transform.Find("Cell3").GetComponent<Image>().color = Color.white;
                row.transform.Find("Cell4").GetComponent<Image>().color = Color.white;
                //row.transform.Find("Cell5").GetComponent<Image>().color = Color.white;
                //row.transform.Find("Cell6").GetComponent<Image>().color = new Color(255, 255, 255); 
            }
            row.transform.Find("Cell0").transform.Find("Text").gameObject.SetActive(false);
            row.transform.Find("Cell0").transform.Find("Button").gameObject.SetActive(true);

            row.transform.Find("Cell0").transform.Find("Button").transform.Find("Text").GetComponent<Text>().text = apitable[i]["id"].ToString();
            row.transform.Find("Cell1").transform.Find("Text").GetComponent<Text>().text = (string)apitable[i]["type"];
            row.transform.Find("Cell2").transform.Find("Text").GetComponent<Text>().text = (string)apitable[i]["x"];
            row.transform.Find("Cell3").transform.Find("Text").GetComponent<Text>().text = (string)apitable[i]["y"];
            row.transform.Find("Cell4").transform.Find("Text").GetComponent<Text>().text = (string)apitable[i]["z"];
            //row.transform.Find("Cell5").transform.Find("Text").GetComponent<Text>().text = (string)apitable[i]["data4"];
            //row.transform.Find("Cell6").transform.Find("Text").GetComponent<Text>().text = (string)apitable[i]["data5"];
        }
        dropdown.ClearOptions();

        // 添加新选项
        dropdown.AddOptions(options);
    }
    //public void Table_json()
    //{

    //    StreamReader streamReader = new StreamReader(Application.dataPath + "/Json/table.json");
    //    string str = streamReader.ReadToEnd();
    //    streamReader.Close();

    //    api_table = JsonMapper.ToObject(str);
    //    Debug.Log(api_table.Count);
    //    total_id = api_table.Count;
    //    for (int i = 0; i < api_table.Count; i++)
    //    {
    //        Debug.Log(api_table[i]["type"]);
    //        GameObject table = GameObject.Find("钻孔信息/Tab/Panel/Table");
    //        GameObject row = GameObject.Instantiate(Row_Prefab, table.transform.position, table.transform.rotation) as GameObject;
    //        row.name = "row" + (i + 1);
    //        row.transform.SetParent(table.transform);
    //        row.transform.localScale = Vector3.one;
    //        if (i%2 == 0)
    //        {
    //            row.transform.Find("Cell0").GetComponent<Image>().color = Color.white;
    //            row.transform.Find("Cell1").GetComponent<Image>().color = Color.white;
    //            row.transform.Find("Cell2").GetComponent<Image>().color = Color.white;
    //            row.transform.Find("Cell3").GetComponent<Image>().color = Color.white;
    //            row.transform.Find("Cell4").GetComponent<Image>().color = Color.white;
    //            //row.transform.Find("Cell5").GetComponent<Image>().color = Color.white;
    //            //row.transform.Find("Cell6").GetComponent<Image>().color = new Color(255, 255, 255); 
    //        }
    //        row.transform.Find("Cell0").transform.Find("Text").gameObject.SetActive(false);
    //        row.transform.Find("Cell0").transform.Find("Button").gameObject.SetActive(true);

    //        row.transform.Find("Cell0").transform.Find("Button").transform.Find("Text").GetComponent<Text>().text = api_table[i]["id"].ToString();
    //        row.transform.Find("Cell1").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["type"];
    //        row.transform.Find("Cell2").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["x"];
    //        row.transform.Find("Cell3").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["y"];
    //        row.transform.Find("Cell4").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["z"];
    //        //row.transform.Find("Cell5").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["data4"];
    //        //row.transform.Find("Cell6").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["data5"];
    //    }
    //}


    [System.Obsolete]
    public void Sumbit_btn()
    {
        Debug.Log(text_x.GetComponent<Text>().text);
        Debug.Log(text_z.GetComponent<Text>().text);
        string x = text_x.GetComponent<Text>().text;
        string y = text_y.GetComponent<Text>().text;
        string z = text_z.GetComponent<Text>().text;
        string type = text_type.GetComponent<Text>().text;
        string data3 = text_data3.GetComponent<Text>().text;
        string data4 = text_data4.GetComponent<Text>().text;
        string data5 = text_data5.GetComponent<Text>().text;

        string sqlSer = "server=192.168.28.229;port=3306;user=han;password=han123;database=test;";
        MySqlConnection conn = new MySqlConnection(sqlSer);
        try
        {
            conn.Open();
            Debug.Log("-----连接成功！------");
            //string sqlinsert = "INSERT INTO test SET x = 4, y = 6 , data1 = 6 , data2 = 6 , data3 = 6 , data4 = 6 , data5 = 6 ";
            string sqlinsert = "insert into test(x,y,z,type,data3,data4,data5) values('" + x + "','" + y + "','" + z + "','" + type + "','" + data3 + "','" + data4 + "','" + data5 + "')";
            MySqlCommand comd = new MySqlCommand(sqlinsert, conn);
            comd.ExecuteNonQuery();
            //Debug.Log(comd.ExecuteNonQuery() + "\n" + sqlinsert);
            string sqlQuary = "select * from test";


            DataSet ds = new DataSet();
            MySqlDataAdapter da = new MySqlDataAdapter(sqlQuary, conn);
            da.Fill(ds);
            GetDatas(ds);

            for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
            {
                Debug.Log(ds.Tables[0].Rows[0][i]);
            }

        }
        catch (System.Exception e)
        {

            Debug.Log(e.Message);
        }
        finally
        {
            conn.Close();
        }
    }

    [System.Obsolete]
    public void Click_register()
    {
        string sqlSer = "server=192.168.28.229;port=3306;user=han;password=han123;database=test;";
        MySqlConnection conn = new MySqlConnection(sqlSer);
        try
        {
            conn.Open();
            Debug.Log("-----连接成功！------");
            string sqlQuary = "select * from test";

            MySqlCommand comd = new MySqlCommand(sqlQuary, conn);
            DataSet ds = new DataSet();

            MySqlDataAdapter da = new MySqlDataAdapter(sqlQuary, conn);
            da.Fill(ds);
            Debug.Log(ds);
            GetDatas(ds);
            //List<DataRow> list_Data=GetDatas(ds);
            //Debug.Log(list_Data);
            //Debug.Log(da);
            //for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
            //{
            //    Debug.Log(ds.Tables[0].Rows[0][i]);
            //}

        }
        catch (System.Exception e)
        {

            Debug.Log(e.Message);
        }
        finally
        {
            conn.Close();
        }
    }

    [System.Obsolete]
    public List<DataRow> GetDatas(DataSet ds)
    {
        List<DataRow> conList = new List<DataRow>();
        //Debug.Log(string.Format("行：{0},列：{1}", ds.Tables[0].Rows.Count, ds.Tables[0].Columns.Count));
        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
        {
            var api_table = ds.Tables[0].Rows;
            Debug.Log(ds.Tables[0].Rows[i]["x"]);
            DataRow row1 = ds.Tables[0].Rows[i];
            GameObject table = GameObject.Find("钻孔信息/Tab/Panel/Table");
            GameObject row = GameObject.Instantiate(Row_Prefab, table.transform.position, table.transform.rotation) as GameObject;
            row.name = "row" + (i + 1);
            row.transform.SetParent(table.transform);
            row.transform.localScale = Vector3.one;
            row.transform.Find("Cell0").transform.Find("Button").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["id"];
            row.transform.Find("Cell1").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["type"];
            row.transform.Find("Cell2").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["x"];
            row.transform.Find("Cell3").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["y"];
            row.transform.Find("Cell4").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["z"];
            row.transform.Find("Cell5").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["data4"];
            row.transform.Find("Cell6").transform.Find("Text").GetComponent<Text>().text = (string)api_table[i]["data5"];
            conList.Add(row1);
        }
        return conList;
    }

    public void clicktable()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            pointerId = -1
        };

        // 获取当前点击的对象
        GameObject clickedObject = EventSystem.current.currentSelectedGameObject;
        
        //Debug.Log("clickedObject: " + clickedObject);
        if (clickedObject != null)
        {
            Button clickedButton = clickedObject.GetComponent<Button>();
            if (clickedButton != null)
            {
                string buttontext = clickedButton.transform.Find("Text").GetComponent<Text>().text;
                Debug.Log("Clicked Button: " + buttontext);
                changetab(buttontext);
                //string currentData = PlayerPrefs.GetString("MyTableData");
                //JsonData data = JsonMapper.ToObject(currentData);
                //for (int i = 0; i < data.Count; i++)
                //{
                //    if (data[i]["id"].ToString() == buttontext)
                //    {
                //        type_text.GetComponent<Text>().text = data[i]["type"].ToString();
                //        x_text.GetComponent<Text>().text = data[i]["x"].ToString();
                //        y_text.GetComponent<Text>().text = data[i]["y"].ToString();
                //        z_text.GetComponent<Text>().text = data[i]["z"].ToString();
                //    }
                //    Debug.Log("Clicked Button: " + data[i]["type"]);
                //}
                //hidden_obj.SetActive(false);
                //dis_obj.SetActive(true);
            }
        }
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

    public void export()
    {
        string ExportData = PlayerPrefs.GetString("MyTableData");
        Application.ExternalCall("GetExportData", ExportData);
    }


    void OnDropdownValueChanged(int index)
    {
        // 获取Dropdown选中的选项
        string selectedOption = dropdown.options[index].text;

        // 执行特定的操作
        HandleSelectedOption(selectedOption);
    }

    void HandleSelectedOption(string option)
    {
        changetab(option);
        //Debug.Log(option);
        // 根据选中的选项执行操作
        //switch (option)
        //{
        //    case "Option1":
        //        Debug.Log("Option 1 selected");
        //        // 执行Option1对应的操作
        //        break;
        //    case "Option2":
        //        Debug.Log("Option 2 selected");
        //        // 执行Option2对应的操作
        //        break;
        //    // 添加更多选项处理
        //    default:
        //        Debug.Log("Other option selected");
        //        break;
        //}
    }
}