using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerprefsTest : MonoBehaviour
{
    private string set_NAME = string.Empty;

    private string get_NAME = string.Empty;

    void OnGUI()

    {

        GUILayout.BeginHorizontal("box");

        GUILayout.Label("姓名：");

        set_NAME = GUILayout.TextArea(set_NAME, 200, GUILayout.Width(50));

        if (GUILayout.Button("存储数据"))

        {

            //将我们输入的姓名保存到本地，命名为_NAME ；

            PlayerPrefs.SetString("_NAME", set_NAME);

        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("box");

        if (GUILayout.Button("读取数据"))

        {

            //读取本地数据中名称为_NAME 的数据；

            get_NAME = PlayerPrefs.GetString("_NAME");

        }

        GUILayout.Label("你输入的姓名：" + get_NAME);

        GUILayout.EndHorizontal();

    }
}
