using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class date : MonoBehaviour
{
    public Text Time_text;
    public JsonData jsonData;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        string[] nowtime = System.DateTime.Now.ToString("yyyy:MM:dd").Split(new char[] { ':' });

        string hour = System.DateTime.Now.Hour < 10 ? "0" + System.DateTime.Now.Hour.ToString() : System.DateTime.Now.Hour.ToString();
        string minute = System.DateTime.Now.Minute < 10 ? "0" + System.DateTime.Now.Minute.ToString() : System.DateTime.Now.Minute.ToString();
        string second = System.DateTime.Now.Second < 10 ? "0" + System.DateTime.Now.Second.ToString() : System.DateTime.Now.Second.ToString();
        string riQi = nowtime[0] + "-" + nowtime[1] + "-" + nowtime[2] + " " + hour + ":" + minute + ":" + second;

        Time_text.text = riQi;
    }
}
