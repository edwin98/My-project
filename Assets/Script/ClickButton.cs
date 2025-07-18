using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickButton : MonoBehaviour
{
    // Start is called before the first frame update
    public Button clickbtn;
    public GameObject controlobj;
    void Start()
    {
        
    }
    public void TogglePanel()
    {
        controlobj.SetActive(!controlobj.activeSelf); // 切换面板的激活状态（显示或隐藏）
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
