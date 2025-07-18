using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class change_silder : MonoBehaviour
{
    public GameObject[] slider_objs;
    int now_slider;
    // Start is called before the first frame update
    void Start()
    {
        now_slider = 0;
    }

    public void right()
    {
        int next_slider = now_slider + 1;
        Debug.Log(next_slider);
        
        Debug.Log(slider_objs.Length);
        if(now_slider == slider_objs.Length - 1)
        {
            Debug.Log("22222");
            slider_objs[now_slider].SetActive(false);
            slider_objs[0].SetActive(true);
            now_slider = 0;
        }
        else
        {
            Debug.Log("000000");
            slider_objs[now_slider].SetActive(false);
            slider_objs[next_slider].SetActive(true);
            now_slider = now_slider + 1;
        }

    }

    public void left()
    {
        int next_slider = now_slider - 1;
        Debug.Log(next_slider);

        if (next_slider<0)
        {
            Debug.Log("-1-1-1");
            Debug.Log(slider_objs.Length - 1);
            slider_objs[slider_objs.Length-1].SetActive(true);
            slider_objs[0].SetActive(false);
            now_slider = slider_objs.Length - 1;
        }
        else
        {
            Debug.Log("000000");
            Debug.Log(now_slider);
            Debug.Log(next_slider);
            slider_objs[now_slider].SetActive(false);
            slider_objs[next_slider].SetActive(true);
            now_slider = now_slider - 1;
        }

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
