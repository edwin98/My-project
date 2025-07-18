using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class changescene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void Scene_Home()
    {
        SceneManager.LoadScene("首页");
    }
    public void Scene_Project()
    {
        SceneManager.LoadScene("工程管理");
    }
    public void Scene_rill()
    {
        SceneManager.LoadScene("钻机概况");
    }
    public void Scene_coal()
    {
        SceneManager.LoadScene("随钻数据统计");
    }

    public void Scene_coaldata(int valueToPass)
    {
        PlayerPrefs.SetInt("Selecttunnel", valueToPass);
        SceneManager.LoadScene("随钻数据统计");
    }
    public void Scene_Statistics()
    {
        SceneManager.LoadScene("煤岩力学信息");
    }
    public void Scene_report()
    {
        SceneManager.LoadScene("隐蔽灾害预测");
    }
    // Update is called once per frame
    public void Quit_()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif

    }
}
