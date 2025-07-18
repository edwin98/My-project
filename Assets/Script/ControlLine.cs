using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class ControlLine : MonoBehaviour
{
    public GameObject lineobj;

    private string[] keylist;
    private float[] xlist;
    private float[] zlist;

    private string[] colorlist;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("888");
        Transform parentTransform = lineobj.transform;

        //if (parentTransform != null)
        //{
        //    Debug.Log("找到父物体：" + parentTransform.name);

        //    // 使用递归查找所有子物体
        //    FindAndLogAllChildren(parentTransform);
        //}
        //else
        //{
        //    Debug.LogError("未找到名为 'ParentObject' 的父物体！");
        //}
        StartCoroutine(Loadline());

    }
    IEnumerator Loadline()
    {
        Debug.Log("999");

        string LineAssetsPath = Application.streamingAssetsPath + "/Json/line.json";

        UnityWebRequest manifestRequest = UnityWebRequest.Get(LineAssetsPath);
        yield return manifestRequest.SendWebRequest();

        if (manifestRequest.isNetworkError || manifestRequest.isHttpError)
        {
            Debug.Log(manifestRequest.error);
        }
        else
        {
            JsonData jsonData = JsonMapper.ToObject(manifestRequest.downloadHandler.text);
            xlist = new float[jsonData.Count];
            zlist = new float[jsonData.Count];
            colorlist = new string[jsonData.Count];
            Debug.Log(jsonData.Count);
            string num = "";
            for (int i = 0; i < jsonData.Count; i++)
            {
                //Debug.Log(jsonData[i]["color"]);
                xlist[i] = float.Parse(jsonData[i]["x"].ToString());
                zlist[i] = float.Parse(jsonData[i]["z"].ToString());
                colorlist[i] = jsonData[i]["color"].ToString();
                //Debug.Log(xlist);
                //Debug.Log(i + "--"+colorlist[i]);
                if ((int)jsonData[i]["key"] >= 10)
                {
                    if ((int)jsonData[i]["key"] < 100)
                    {
                        num = "0" + jsonData[i]["key"];
                    }
                    else if ((int)jsonData[i]["key"] >= 100)
                    {
                        num = jsonData[i]["key"].ToString();
                    }
                }
                else
                {
                    num = "00" + jsonData[i]["key"];
                }
                string linename = "ZK_" + num;
                //Debug.Log(linename);
                //Debug.Log(jsonData[i]["value"]);

                


            }
            Transform parentTransform = lineobj.transform;

            if (parentTransform != null)
            {
                //Debug.Log("找到父物体：" + parentTransform.name);

                // 使用递归查找所有子物体
                FindAndLogAllChildren(parentTransform);
            }
            else
            {
                Debug.LogError("未找到名为 'ParentObject' 的父物体！");
            }
        }
    }

    void FindAndLogAllChildren(Transform parent)
    {
        //foreach (Transform child in parent)
        //{
        //    // 打印当前子物体的信息
        //    Debug.Log("子物体：" + child.name);
        //    Debug.Log("子物体：" + child.transform.rotation);
        //    child.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
        //    // 递归查找子物体的子物体
        //    FindAndLogAllChildren(child);
        //}
        foreach (Transform child in parent)
        {
            //Debug.Log("color1：" + colorlist[2]);
            // 打印当前子物体的信息
            //Debug.Log("子物体：" + child.name);
            string childname = child.name;
            string pattern = @"ZK_(\d+)"; // 正则表达式，匹配以 ZK_ 开头，后面跟着数字的部分

            Match match = Regex.Match(childname, pattern);
            if (match.Success)
            {
                string numberPart = match.Groups[1].Value; // 获取第一个捕获组的值，即数字部分
                //Debug.Log("提取的数字部分：" + numberPart);
                int number = int.Parse(numberPart);
                number--;
                // Debug.Log("number：" + number);
                //Debug.Log("valuelist：" + valuelist);
                // Debug.Log("xlist：" + xlist[number]);
                // Debug.Log("color：" + colorlist[number]);
                child.localEulerAngles = new Vector3(xlist[number], 0.0f, zlist[number]);
                Renderer renderer = child.GetComponent<Renderer>();
                // Debug.Log("renderer：" + renderer);
                if (renderer)
                {
                    renderer.material.color = HexToColor(colorlist[number]);
                }
                
            }

            // 修改子物体的旋转值
            //child.rotation = Quaternion.Euler(10f, 0f, 0f);
            
            Debug.Log("子物体旋转：" + child.localEulerAngles);

            // 递归查找子物体的子物体
            FindAndLogAllChildren(child);
        }

    }
    Color HexToColor(string hex)
    {
        Debug.Log("---");
        Debug.Log(hex);
        // 处理颜色字符串
        //hex = hex.Replace("#", "");

        // 确保字符串长度正确
        if (hex.Length != 6)
        {
            Debug.LogError("Invalid hex color length.");
            return Color.black;
        }

        // 解析十六进制值
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        // 创建 Color 对象（RGBA）
        return new Color(r / 255f, g / 255f, b / 255f);


    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
