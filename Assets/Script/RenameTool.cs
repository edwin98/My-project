﻿#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CustomEditorWindow
{
    public class RenameTool : EditorWindow
    {
        #region members
        private Vector2 scrollPos;
        private int charSize;
        private string targetName = string.Empty;
        private string startNum_str = "0";
        private int startNum_int = 0;
        #endregion
        #region get window
        [MenuItem("Tools/Rename Tool")]
        private static void OpenWindow()
        {
            GetWindow<RenameTool>("Rename Tool").Show();
        }
        #endregion
        #region render the window
        private void OnGUI()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            {
                #region tool title
                charSize = GUI.skin.label.fontSize;
                GUI.color = Color.yellow;
                GUILayout.Space(10);
                GUI.skin.label.fontSize = 24;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label("Rename Tool");
                GUI.skin.label.fontSize = charSize;
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                GUI.color = Color.white;
                GUILayout.Space(20);
                #endregion

                GUILayout.BeginVertical();
                {
                    #region text input
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Name Style: ");
                        GUILayout.FlexibleSpace();
                        targetName = GUILayout.TextField(targetName, GUILayout.Width(140));
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Start Number: ");
                        GUILayout.FlexibleSpace();
                        startNum_str = GUILayout.TextField(startNum_str, GUILayout.Width(60));
                        try
                        {
                            startNum_int = int.Parse(startNum_str);
                        }
                        catch
                        {

                        }
                    }
                    GUILayout.EndHorizontal();
                    #endregion
                    #region button
                    GUILayout.Space(30);
                    bool hasObject = (Selection.objects.Length > 0);
                    GUI.enabled = hasObject;

                    GUILayout.FlexibleSpace();
                    if (!hasObject)
                    {
                        GUI.color = Color.red;
                        GUILayout.Button("No Selected Objects!");
                        GUI.color = Color.white;

                    }
                    else
                    {
                        if (GUILayout.Button("Rename"))
                        {
                            Rename(targetName, startNum_int);
                        }
                    }
                    GUILayout.Space(20);
                    #endregion
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
        }
        #endregion
        #region Rename Function
        private void Rename(string t_name, int t_index)
        {
            string name = t_name.Trim();//去除头尾空白字符串
            int index = t_index;
            if ((name + index) != string.Empty)//若名字不为空
            {
                bool isAssetsObject = false;//flag, 是否是assets文件夹的资源

                foreach (Object o in Selection.objects)
                {
                    string path_g = AssetDatabase.GetAssetPath(o);//获得选中物的路径
                    //查看路径后缀
                    if (Path.GetExtension(path_g) != "")//若后缀不为空, 则为assets文件夹物体
                    {
                        if (name.Length >= 2 && name.Substring(0, 2) == "m_")// m_ 开头会被吞
                        {
                            //用 M_ 修正
                            string temp_name = name.Remove(0, 1);
                            name = temp_name.Insert(0, "M");
                        }
                        AssetDatabase.RenameAsset(path_g, name + index);//改名API
                        if (!isAssetsObject)
                        {
                            isAssetsObject = true;//修改flag
                        }
                    }
                    else//后缀为空, 是场景物体
                    {
                        o.name = name + index;
                    }
                    index++;
                }
                if (isAssetsObject)//若是assets文件夹资源, 则刷新assets
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }
        #endregion
    }
}
#endif
