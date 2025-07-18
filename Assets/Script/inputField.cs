using LitJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using NPOI.XWPF.UserModel;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using TMPro;

public class inputField : MonoBehaviour
{
    public TMP_InputField inputFiel;
    public TMP_Text Placeholder_text;
    public TMP_Text input_text;
    JsonData jsonData_list;
    String guide;

    private string filePath;
    private string fileName = "guide.docx";
    private string path;
    // Start is called before the first frame update
    void Start()
    {
        //filePath = Application.dataPath + "/word";
        //path = Path.Combine(filePath, fileName);
        //if (!File.Exists(path))
        //{
        //    FileStream stream = File.Create(path);
        //    stream.Close();
        //}
        //Guide_json();
        //Debug.Log(guide);
        //inputFiel.text = "999";
        inputFiel.text = Placeholder_text.text;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Guide_json()
    {
        StreamReader streamReader = new StreamReader(Application.dataPath + "/Json/guide.json");
        string str = streamReader.ReadToEnd();
        streamReader.Close();
        jsonData_list = JsonMapper.ToObject(str);
        foreach (JsonData item in jsonData_list)
        {
            guide = item["guide"].ToString();
        }
        Debug.Log(guide);
        inputFiel.text = guide;
        input_text.text = guide;

    }
    public void ButtonPress()
    {
        //when this button is pressed it will set the value or a default 
        if (inputFiel.text == "")
        {
            //insert default information 
        }
        else
        {
            //enter the rest of the function here 
        }
    }
    public void Create()
    {
        


    }
    public void Save_Button()
    {
        string t = input_text.text;
        //input_text.text = t;
        //Debug.Log(t);
        Application.ExternalCall("GetWordData", t);
        //string[] context = new string[] {
        //    t
        //};
        //var itemToAdd = new JObject();
        //itemToAdd["guide"] = inputFiel.text;
        //foreach (JsonData item in jsonData_list)
        //{
        //    item["guide"] = t;
        //}
        ////Debug.Log(jsonData_list.ToJson());

        //string jsonString = jsonData_list.ToJson();
        ////Debug.Log(jsonString);
        //string FileUrl = Application.dataPath + "/Json/guide.json";

        //File.WriteAllText(FileUrl, jsonString);
        //XWPFDocument doc = new XWPFDocument();
        //XWPFParagraph paragraph = doc.CreateParagraph();
        //paragraph.Alignment = ParagraphAlignment.LEFT;
        //XWPFRun run = paragraph.CreateRun();
        //run.FontSize = 15;
        //run.SetColor("000000");
        //run.FontFamily = "宋体";

        //run.SetText(context[0]);

        //try
        //{
        //    FileStream fs = new FileStream(path, FileMode.Create);
        //    doc.Write(fs);
        //    fs.Close();
        //    fs.Dispose();
        //    Process.Start("explorer.exe", filePath.Replace("/", "\\"));
        //}
        //catch (Exception e)
        //{
        //    if (e.GetType() == typeof(IOException))
        //    {
        //        Debug.Log("创建失败，同名文件被打开！");
        //    }
        //}
        //Debug.Log(t);
    }
}
