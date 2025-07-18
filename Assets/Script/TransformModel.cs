using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformModel : MonoBehaviour
{
    //上下旋转最大角度限制
    public int yMinLimit = -20;
    public int yMaxLimit = 80;
    //旋转速度
    public float xSpeed = 250.0f;//左右旋转速度
    public float ySpeed = 120.0f;//上下旋转速度
    //旋转角度
    private float x = 0.0f;
    //private float y = 0.0f;
    private float z = 0.0f;

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            //Input.GetAxis("MouseX")获取鼠标移动的X轴的距离
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            z -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            z = ClampAngle(z, yMinLimit, yMaxLimit);
            //欧拉角转化为四元数
            Quaternion rotation = Quaternion.Euler(z, x, 0);
            transform.rotation = rotation;
        }
    }

    //角度范围值限定
    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

}
