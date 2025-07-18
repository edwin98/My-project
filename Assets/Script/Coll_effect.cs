using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coll_effect : MonoBehaviour
{
    public Transform Head_rot;
    public GameObject effect;
    public GameObject Wall;
    float t;
    float hitPos_y;
    float hitPos_z;
    Material material;

    public Material s_Material;
    //public Movedrill Mdrill;
    //private GameInfo gameInfo;
    void Start()
    {
        //Mdrill = new Movedrill();
        //Mdrill.DoSomething();
        //GameObject gameInfo = GameObject.Find("钻机").GetComponent<Movedrill>();
        //Mdrill scriptB = b.GetComponent<Movedrill>();

        material = new Material(Shader.Find("Transparent/Diffuse"));
        material.color = Color.black;
    }
    void Update()
    {
        

    }

    void OnTriggerEnter(Collider other)
    {
        t = Movedrill.Head_Angles();
        Debug.Log(transform.name);
        //Debug.Log(other);
        effect.SetActive(true);
        Vector3 hitPos = other.bounds.ClosestPoint(transform.position);

        //Debug.Log(hitPos);
        
        // 设置大小
        if(transform.name == "钻头 1")
        {
            GameObject Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(Sphere.GetComponent<SphereCollider>());
            Sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            if (1.76f < hitPos.y && hitPos.y < 1.77f)
            {
                hitPos_y = hitPos.y;
            }

            else
            {
                hitPos_y = hitPos.y - 4 * (hitPos.y - 1.7679f);
            }
            //Debug.Log("hitPos.z: " + hitPos.z);
            if (-95f < t && t < -85f)
            {
                hitPos_z = hitPos.z;
            }

            else if (-95f > t)
            {
                hitPos_z = (0.35f / hitPos.z) + hitPos.z;
            }
            else if (-85f < t)
            {
                hitPos_z = hitPos.z - (1.45f / hitPos.z);
            }
            //Sphere.transform.position = new Vector3(hitPos.x, hitPos_y, hitPos_z);
            //Debug.Log("Head_z: " + Mdrill.Head_z);
            Sphere.transform.position = hitPos;
            //MeshRenderer mesh_renderer = Sphere.GetComponent<MeshRenderer>();
            //mesh_renderer.material = Resources.Load<Material>("Black");
            //Sphere.GetComponent<Renderer>().material = material;
            Sphere.GetComponent<Renderer>().material = s_Material;
        }
        else
        {

        }
        
        //Head_rot.Rotate(new Vector3(0, 20 * Time.deltaTime, 0), Space.Self);

    }
    //触发器（无物理效果）持续碰撞检测
    void OnTriggerStay(Collider other)
    {
        Debug.Log("22");
        Head_rot.Rotate(new Vector3(0, 1800 * Time.deltaTime, 0), Space.Self);

    }
    //触发器（无物理效果）离开碰撞检测
    void OnTriggerExit(Collider other)
    {
        //Debug.Log("33");
        effect.SetActive(false);
    }

    //void OnCollisionEnter(Collision collision) {
    //    Debug.Log("22888888");
    //}
    //void OnCollisionStay(Collision collision) {
    //    Debug.Log("229998");
    //}
    //void OnCollisionExit(Collision collision) {
    //    Debug.Log("6666");
    //}




}
 
