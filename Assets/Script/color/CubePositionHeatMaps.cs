using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using LitJson;

public class CubePositionHeatMaps : MonoBehaviour
{
    const int scale = 50;
    Dictionary<Transform, GameObject> PainterDics = new Dictionary<Transform, GameObject>();
    float offset = 10;

    public GameObject heatmapContainer;
    public int[] counts;
    public int[] counts0;
    public int[] counts1;
    public int[] counts2;
    public float[] pos_x0;
    public float[] pos_x1;
    public float[] pos_y0;
    public float[] pos_y1;
    public float[] contribute;

    public float[] one_data;

    public Color[] colors;
    public Color[] colors1;

    private int lastSecond = -1;
    private int startSecond;
    string jsonUrl = Application.streamingAssetsPath + "/Color.json";
    private string lastJsonHash = "";
    
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
    }
    IEnumerator LoadJsonData0()
    {
        string jsonUrl = Application.streamingAssetsPath + "/Colors.json";
        //yield return LoadJsonData();
        while (true)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(jsonUrl);
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                JsonData jsonData = JsonMapper.ToObject(webRequest.downloadHandler.text);


                JsonData count0Array = jsonData["data0"][0]["counts0"];
                JsonData count1Array = jsonData["data0"][0]["counts1"];
                JsonData count2Array = jsonData["data0"][0]["counts2"];
                JsonData pos_x0Array = jsonData["data0"][0]["pos_x0"];
                JsonData pos_x1Array = jsonData["data0"][0]["pos_x1"];
                JsonData pos_y0Array = jsonData["data0"][0]["pos_y0"];
                JsonData pos_y1Array = jsonData["data0"][0]["pos_y1"];

                List<Position> posList = new List<Position>();

                for (int i = 0; i < count0Array.Count; i++)
                {
                    Debug.Log(pos_x1Array[i]);
                    Debug.Log((int)count0Array[i]);
                    for (int j = 0; j < (int)count0Array[i]; j++)
                    {
                        //Debug.Log(pos_x1Array[i]);
                        posList.Add(new Position(GetVal((float)(double)pos_x0Array[i], (float)(double)pos_x1Array[i]), GetVal((float)(double)pos_y0Array[0], (float)(double)pos_y1Array[0]), 0, 10));
                        //posList.Add(new Position(GetVal((float)(double)pos_x0Array[i], (float)(double)pos_x1Array[i]), GetVal((float)(double)pos_y0Array[0], (float)(double)pos_y1Array[0]), 0, 10)); 
                    }
                }
                for (int i = 0; i < count1Array.Count; i++)
                {
                    for (int j = 0; j < (int)count1Array[i]; j++)
                    {
                        posList.Add(new Position(GetVal((float)(double)pos_x0Array[i], (float)(double)pos_x1Array[i]), GetVal((float)(double)pos_y0Array[1], (float)(double)pos_y1Array[1]), 0, 10)); 
                    }
                }
                for (int i = 0; i < count2Array.Count; i++)
                {
                    for (int j = 0; j < (int)count2Array[i]; j++)
                    {
                        posList.Add(new Position(GetVal((float)(double)pos_x0Array[i], (float)(double)pos_x1Array[i]), GetVal((float)(double)pos_y0Array[2], (float)(double)pos_y1Array[2]), 0, 10));
                    }
                }

                DrawPositionHeatmap(heatmapContainer, posList);
            }

            yield return new WaitForSeconds(5f);
        }
    }
    IEnumerator LoadJsonData1()
    {
        string jsonUrl = Application.streamingAssetsPath + "/Colors.json";
        //yield return LoadJsonData();
        while (true)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(jsonUrl);
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                JsonData jsonData = JsonMapper.ToObject(webRequest.downloadHandler.text);


                JsonData count0Array = jsonData["data0"][1]["counts0"];
                JsonData count1Array = jsonData["data0"][1]["counts1"];
                JsonData count2Array = jsonData["data0"][1]["counts2"];
                JsonData pos_x0Array = jsonData["data0"][1]["pos_x0"];
                JsonData pos_x1Array = jsonData["data0"][1]["pos_x1"];
                JsonData pos_y0Array = jsonData["data0"][1]["pos_y0"];
                JsonData pos_y1Array = jsonData["data0"][1]["pos_y1"];

                List<Position> posList = new List<Position>();

                for (int i = 0; i < count0Array.Count; i++)
                {
                    for (int j = 0; j < (int)count0Array[i]; j++)
                    {
                        posList.Add(new Position(GetVal((float)(double)pos_x0Array[i], (float)(double)pos_x1Array[i]), GetVal((float)(double)pos_y0Array[0], (float)(double)pos_y1Array[0]), 0, 10));
                        //posList.Add(new Position(GetVal((float)(double)pos_x0Array[i], (float)(double)pos_x1Array[i]), GetVal((float)(double)pos_y0Array[0], (float)(double)pos_y1Array[0]), 0, 10)); // ���ӹ���ֵ
                    }
                }
                for (int i = 0; i < count1Array.Count; i++)
                {
                    for (int j = 0; j < (int)count1Array[i]; j++)
                    {
                        posList.Add(new Position(GetVal((float)(double)pos_x0Array[i], (float)(double)pos_x1Array[i]), GetVal((float)(double)pos_y0Array[1], (float)(double)pos_y1Array[1]), 0, 10)); // ���ӹ���ֵ
                    }
                }
                for (int i = 0; i < count2Array.Count; i++)
                {
                    for (int j = 0; j < (int)count2Array[i]; j++)
                    {
                        posList.Add(new Position(GetVal((float)(double)pos_x0Array[i], (float)(double)pos_x1Array[i]), GetVal((float)(double)pos_y0Array[2], (float)(double)pos_y1Array[2]), 0, 10)); // ���ӹ���ֵ
                    }
                }

                DrawPositionHeatmap(heatmapContainer, posList);
            }

            yield return new WaitForSeconds(5f);
        }
    }

    IEnumerator LoadJsonData2()
    {
        string jsonUrl = Application.streamingAssetsPath + "/Colors.json";
        //yield return LoadJsonData();
        while (true)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(jsonUrl);
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                JsonData jsonData = JsonMapper.ToObject(webRequest.downloadHandler.text);

                JsonData count0Array = jsonData["data0"][2]["counts0"];
                JsonData count1Array = jsonData["data0"][2]["counts1"];
                JsonData count2Array = jsonData["data0"][2]["counts2"];
                JsonData pos_x0Array = jsonData["data0"][2]["pos_x0"];
                JsonData pos_x1Array = jsonData["data0"][2]["pos_x1"];
                JsonData pos_y0Array = jsonData["data0"][2]["pos_y0"];
                JsonData pos_y1Array = jsonData["data0"][2]["pos_y1"];

                List<Position> posList = new List<Position>();

                for (int i = 0; i < count0Array.Count; i++)
                {
                    for (int j = 0; j < (int)count0Array[i]; j++)
                    {
                        posList.Add(new Position(GetVal((float)(double)pos_x0Array[i], (float)(double)pos_x1Array[i]), GetVal((float)(double)pos_y0Array[0], (float)(double)pos_y1Array[0]), 0, 10));
                        //posList.Add(new Position(GetVal((float)(double)pos_x0Array[i], (float)(double)pos_x1Array[i]), GetVal((float)(double)pos_y0Array[0], (float)(double)pos_y1Array[0]), 0, 10));
                    }
                }
                for (int i = 0; i < count1Array.Count; i++)
                {
                    for (int j = 0; j < (int)count1Array[i]; j++)
                    {
                        posList.Add(new Position(GetVal((float)(double)pos_x0Array[i], (float)(double)pos_x1Array[i]), GetVal((float)(double)pos_y0Array[1], (float)(double)pos_y1Array[1]), 0, 10));
                    }
                }
                for (int i = 0; i < count2Array.Count; i++)
                {
                    for (int j = 0; j < (int)count2Array[i]; j++)
                    {
                        posList.Add(new Position(GetVal((float)(double)pos_x0Array[i], (float)(double)pos_x1Array[i]), GetVal((float)(double)pos_y0Array[2], (float)(double)pos_y1Array[2]), 0, 10));
                    }
                }

                DrawPositionHeatmap(heatmapContainer, posList);
            }

            yield return new WaitForSeconds(5f);
        }
    }

    double GetVal(float min, float max)
    {
        return Random.Range(min, max);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void GetPoint()
    {
        MeshRenderer meshRenderer = heatmapContainer.GetComponent<MeshRenderer>();
        Material material = meshRenderer.material;
        Vector2 offset = material.mainTextureOffset;
        Vector2 scale = material.mainTextureScale;

        UnityEngine.Bounds bounds = meshRenderer.bounds;
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(center.x - extents.x, center.y + extents.y, center.z);
        vertices[1] = new Vector3(center.x + extents.x, center.y + extents.y, center.z); 
        vertices[2] = new Vector3(center.x - extents.x, center.y - extents.y, center.z); 
        vertices[3] = new Vector3(center.x + extents.x, center.y - extents.y, center.z);

        for (int i = 0; i < 4; i++)
        {
            vertices[i].x = Mathf.Lerp(vertices[i].x, vertices[i].x + extents.x * 2, scale.x); 
            vertices[i].y = Mathf.Lerp(vertices[i].y, vertices[i].y + extents.y * 2, scale.y);
            vertices[i].x += offset.x; 
            vertices[i].y += offset.y;
        }

        foreach (Vector3 vertex in vertices)
        {
            Debug.Log("Vertex position: " + vertex);
        }
    }


    public void addone()
    {
        List<Position> posList = new List<Position>();
        for (int i = 0; i < one_data[0]; i++)
        {
            posList.Add(new Position(GetVal(one_data[1], one_data[2]), GetVal(one_data[3], one_data[4]), 0, one_data[0])); 
        }

        DrawPositionHeatmap(heatmapContainer, posList);
    }




    public void adds_test_cube0()
    {
        StartCoroutine(LoadJsonData0());
    }
    public void adds_test_cube1()
    {
        StartCoroutine(LoadJsonData1());
    }
    public void adds_test_cube2()
    {
        StartCoroutine(LoadJsonData2());
    }

    void DrawPositionHeatmap(GameObject container, List<Position> positionList)
    {
        GameObject painter;
        Texture2D tx;
        Vector3 containerSize = container.transform.lossyScale;
        // Debug.Log(containerSize);
        INT size = new INT((int)containerSize.x * scale, (int)containerSize.z * scale);
        // Debug.Log("size---" + size.x);
        // Debug.Log("size-y--" + size.y);
        if (!PainterDics.ContainsKey(container.transform))
        {
            painter = heatmapContainer;
            Renderer renderer = painter.GetComponent<Renderer>();
            Material m = new Material(Shader.Find("Standard"));
            renderer.material = m;
            tx = new Texture2D(size.x, size.y);
            renderer.material.mainTexture = tx;
            PainterDics.Add(container.transform, painter);
        }
        else
        {
            painter = PainterDics[container.transform];
            Renderer renderer = painter.GetComponent<Renderer>();
            tx = renderer.material.mainTexture as Texture2D;
        }

        float[,] posArray = new float[size.x + 1, size.y + 1];
        foreach (var pos in positionList)
        {
            PosArrayAdd(posArray, pos, size);
        }

        for (int i = 0; i < size.x + 1; i++)
        {
            for (int j = 0; j < size.y + 1; j++)
            {
                tx.SetPixel(i, j, GetColor(posArray[i, j]));
            }
        }
        tx.Apply();
    }


    void PosArrayAdd(float[,] posArray, Position pos, INT size)
    {
        int minx = Mathf.Clamp(pos.x - scale / 2, 0, size.x + 1);
        int maxx = Mathf.Clamp(pos.x - scale / 2 + scale, 0, size.x + 1);
        int miny = Mathf.Clamp(pos.y - scale / 2, 0, size.y + 1);
        int maxy = Mathf.Clamp(pos.y - scale / 2 + scale, 0, size.y + 1);
        for (int i = minx; i < maxx; i++)
        {
            for (int j = miny; j < maxy; j++)
            {
                float dis = Vector2.Distance(new Vector2(i, j), new Vector2(pos.x, pos.y));
                if (dis <= scale / 2)
                {
                    posArray[i, j] += pos.contribution; 
                }
            }
        }
    }


    Color GetColor(float count)
    {
        count = count * 2 / scale;
        Color color = new Color(1, 1, 1, 0);
      
        if (count > 0 && count <= offset)
        {
            color = Color.Lerp(colors[0], colors[1], count / offset);
        }
        else if (count > offset && count <= offset * 2)
        {
            color = Color.Lerp(colors[1], colors[2], (count - offset) / offset);
        }
        else if (count > 2 * offset && count <= 3 * offset)
        {
            color = Color.Lerp(colors[2], colors[3], (count - offset * 2) / offset);
        }
        else if (count > 3 * offset && count <= 4 * offset)
        {
            color = Color.Lerp(colors[3], colors[4], (count - offset * 2) / offset);
        }
        else if (count > 4 * offset && count <= 5 * offset)
        {
            color = Color.Lerp(colors[4], colors[5], (count - offset * 2) / offset);
        }
        else if (count > 5 * offset && count <= 6 * offset)
        {
            color = Color.Lerp(colors[5], colors[6], (count - offset * 2) / offset);
        }
        else if (count > 6 * offset && count <= 7 * offset)
        {
            color = Color.Lerp(colors[6], colors[7], (count - offset * 2) / offset);
        }
        else if (count > 7 * offset && count <= 8 * offset)
        {
            color = Color.Lerp(colors[7], colors[8], (count - offset * 2) / offset);
        }
        else if (count > 8 * offset && count <= 9 * offset)
        {
            color = Color.Lerp(colors[8], colors[9], (count - offset * 2) / offset);
        }
        else if (count > 9 * offset && count <= 10 * offset)
        {
            color = Color.Lerp(colors[9], colors[10], (count - offset * 2) / offset);
        }
        else if (count > 10 * offset)
        {
            color = colors[10];
        }
       
        else
            //color = new Color(Color.blue.r, Color.blue.g, Color.blue.b, 1);
            color = colors[0];
        return color.a == 0 ? color : new Color(color.r, color.g, color.b, 1);
    }


    public struct Position
    {
        public int x;
        public int y;
        public int z;
        public float contribution; 

        public Position(double x, double y, double z, float contribution)
        {
            this.x = ((int)(x * scale));
            this.y = ((int)(y * scale));
            this.z = ((int)(z * scale));
            this.contribution = contribution;
        }
    }


    public struct INT
    {
        public int x;
        public int y;

        public INT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

}
