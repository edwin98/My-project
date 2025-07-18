using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UserRow : MonoBehaviour
{
    [System.NonSerialized]
    public string id;
    // 深度
    public Text key;
    // 抗压强度
    public Text strength;
    // 凝聚力
    public Text cohesive;
    // 摩擦角
    public Text frivtion;
    // 裂隙程度百分比
    public Text crack;
}
