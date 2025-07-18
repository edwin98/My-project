using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Movedrill : MonoBehaviour
{
    public float speed = 50;
    public static GameObject Head;
    public Transform SJ;
    public Transform SZ;
    public float Headspeed = 10;

    public Transform Head_rot;
    public GameObject effect;
    public static float Head_z;

    public Transform Head_bar;
    float Head_bar_sy;
    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(transform);
        //Debug.Log(SJ.position.y);
        Head = GameObject.Find("横旋转");
        Head_z = float.Parse(GetInspectorRotation(Head.transform)[2]);
        Head_bar_sy = Head_bar.localPosition.y;
    }

    
    void Update()
    {
        Move_Update();
    }
    private void Move_Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            
            if (transform.position.z< 3)
            {
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }

        }
        if (Input.GetKey(KeyCode.A))
        {
         
            if (transform.position.z > -3)
            {
                transform.Translate(Vector3.back * speed * Time.deltaTime);
            }

        }
        if (Input.GetKey(KeyCode.W))
        {
            Debug.Log(transform.position.x);
            if (transform.localPosition.x > -0.45)
            {
                transform.Translate(Vector3.left * speed * Time.deltaTime);
            }
        }
        if (Input.GetKey(KeyCode.S))
        {
 
            if (transform.localPosition.x <5)
            {
                transform.Translate(Vector3.right * speed * Time.deltaTime);
            }

        }

        if (Input.GetKey(KeyCode.F))
        {
            float Z = float.Parse(Movedrill.GetInspectorRotation(Head.transform)[2]);
            if (Z > -150)
            {
                Head.transform.Rotate(new Vector3(0, 0, -Headspeed * Time.deltaTime), Space.Self);
            }
        }
        if (Input.GetKey(KeyCode.G))
        {
            float Z = float.Parse(Movedrill.GetInspectorRotation(Head.transform)[2]);
            if (Z < 150)
            {
                Head.transform.Rotate(new Vector3(0, 0, Headspeed * Time.deltaTime), Space.Self);
            }
        }
        if (Input.GetKey(KeyCode.E))
        {
            //Debug.Log(SJ.position.y);
            if (SJ.localPosition.z < 0.9)
            {
                SJ.Translate(Vector3.forward * 0.5f * Time.deltaTime);
            }
        }
        if (Input.GetKey(KeyCode.R))
        {
            //Debug.Log(SJ.position.y);
            if (SJ.localPosition.z > 0.56)
            {
                SJ.Translate(Vector3.forward * -0.5f * Time.deltaTime);
            }
        }
        if (Input.GetKey(KeyCode.H))
        {
            
            float X = float.Parse(GetInspectorRotation(SZ)[0]);
 
            if (X > -30)
            {
                SZ.Rotate(new Vector3(-Headspeed * Time.deltaTime, 0, 0), Space.Self);
            }
        }
        if (Input.GetKey(KeyCode.J))
        {
            float X = float.Parse(GetInspectorRotation(SZ)[0]);
            
            if (X < 30)
            {
                SZ.Rotate(new Vector3(Headspeed * Time.deltaTime, 0,0 ), Space.Self);
            }
        }

        if (Input.GetKey(KeyCode.T))
        {
          
            Debug.Log(Head_bar_sy);
            if (Head_bar_sy >= 0.2f)
            {

                Head_bar.Translate(new Vector3(0, -0.2f * Time.deltaTime, 0), Space.Self);
                Head_bar_sy = Head_bar.localPosition.y;
            }
        }

        if (Input.GetKey(KeyCode.Y))
        {

            if (Head_bar_sy <= 0.6f)
            {
                Head_bar.Translate(new Vector3(0, 0.2f * Time.deltaTime, 0), Space.Self);
                Head_bar_sy = Head_bar.localPosition.y;
            }
        }
    }

    public static string[] GetInspectorRotation(Transform transform)
    {
        // 获取原生值
        System.Type transformType = transform.GetType();
        PropertyInfo m_propertyInfo_rotationOrder = transformType.GetProperty("rotationOrder", BindingFlags.Instance | BindingFlags.NonPublic);
        object m_OldRotationOrder = m_propertyInfo_rotationOrder.GetValue(transform, null);
        MethodInfo m_methodInfo_GetLocalEulerAngles = transformType.GetMethod("GetLocalEulerAngles", BindingFlags.Instance | BindingFlags.NonPublic);
        object value = m_methodInfo_GetLocalEulerAngles.Invoke(transform, new object[] { m_OldRotationOrder });
        string temp = value.ToString();
        //将字符串第一个和最后一个去掉
        temp = temp.Remove(0, 1);
        temp = temp.Remove(temp.Length - 1, 1);
        //用‘，’号分割
        string[] tempVector3;
        tempVector3 = temp.Split(',');
        //将分割好的数据传给Vector3
        Vector3 vector3 = new Vector3(float.Parse(tempVector3[0]), float.Parse(tempVector3[1]), float.Parse(tempVector3[2]));
        return tempVector3;

    }


    public static float Head_Angles()
    {
        Movedrill.Head_z = float.Parse(Movedrill.GetInspectorRotation(Movedrill.Head.transform)[2]);
        return Movedrill.Head_z;
        //Debug.Log("Here is script B");
    }

}
