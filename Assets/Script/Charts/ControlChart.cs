using System.Collections;
using UnityEngine;
using XCharts.Runtime;
using System.Collections.Generic;
using UnityEngine.Networking;
using LitJson;

[System.Serializable]
public class Data
{
    public string name;
    public float value;
}


[System.Serializable]
public class ChartData
{
    public List<Series> series;
    public xAxis xAxis;
    public yAxis yAxis;
}
[System.Serializable]
public class Series
{
    public string name;
    public bool display;
    public List<DataPoint> data;
}

[System.Serializable]
public class DataPoint
{
    public string key;
    public float value;
}

[System.Serializable]
public class xAxis
{
    public string title;
    public List<string> label;
}

[System.Serializable]
public class yAxis
{
    public string title;
    public List<string> categories;
}
public class ControlChart : MonoBehaviour
{
    public LineChart lineChart; // 引用你的 LineChart 组件

    public string jsonFilePath;

    public int type;

    //private float[] ynewData;
    private float[] ynewData;
    private string[] xnewData;
    void Start()
    {
        if(type== 1){
            StartCoroutine(LoadData());
        }
        else if (type == 2)
        {
            StartCoroutine(LoadDatas());
        }

    }
    
    
    IEnumerator LoadData()
    {

        string LineAssetsPath = Application.streamingAssetsPath + jsonFilePath;

        UnityWebRequest manifestRequest = UnityWebRequest.Get(LineAssetsPath);
        yield return manifestRequest.SendWebRequest();

        if (manifestRequest.isNetworkError || manifestRequest.isHttpError)
        {
            Debug.Log(manifestRequest.error);
        }
        else
        {
            JsonData jsonData = JsonMapper.ToObject(manifestRequest.downloadHandler.text);
            xnewData = new string[jsonData.Count];
            ynewData = new float[jsonData.Count];
           
            //Debug.Log(jsonData.Count);
            for (int i = 0; i < jsonData.Count; i++)
            {
                ynewData[i] = float.Parse(jsonData[i]["value"].ToString());
                ynewData[i] = Mathf.Round(ynewData[i] * 100) / 100f;
                xnewData[i] = jsonData[i]["key"].ToString();

            }
            UpdateXAxis(xnewData);

            // 更新图表的纵坐标
            UpdateYAxis(ynewData);

            // 刷新图表显示
            lineChart.RefreshChart();
        }
    }

    IEnumerator LoadDatas()
    {
        Debug.Log(333);
        string LineAssetsPath = Application.streamingAssetsPath + jsonFilePath;

        UnityWebRequest manifestRequest = UnityWebRequest.Get(LineAssetsPath);
        yield return manifestRequest.SendWebRequest();

        if (manifestRequest.isNetworkError || manifestRequest.isHttpError)
        {
            Debug.Log(manifestRequest.error);
        }
        else
        {
            string jsonData = manifestRequest.downloadHandler.text;
            ChartData chartData = JsonUtility.FromJson<ChartData>(jsonData);
            Debug.Log(chartData);
            UpdateLineChart(chartData);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateXAxis(string[] xAxisData)
    {
        for (int i = 0; i < xAxisData.Length; i++)
        {
            //Debug.Log(xAxisData[i]);
            lineChart.AddXAxisData(xAxisData[i].ToString());
        }
    }

    void UpdateYAxis(float[] newData)
    {
        if (lineChart.series.Count > 0)
        {
            Serie serie = lineChart.series[0]; // 获取第一个系列数据
            serie.ClearData(); // 清除当前系列的所有数据

            // 准备一个 List<double> 类型的数据
            List<double> dataList = new List<double>();
            foreach (var data in newData)
            {
                
                //dataList.Add((double)data); // 将 float 转换为 double 并添加到列表中
                lineChart.AddData(0, data);
          
            }
            
            // 添加新的数据
            serie.AddData(dataList);

            // 刷新图表
            //lineChart.RefreshChart();
        }
        else
        {
            Debug.LogError("No series found in the chart.");
        }
    }


    private void UpdateLineChart(ChartData chartData)
    {
        var title = lineChart.EnsureChartComponent<Title>();
      

        // 设置X轴
        var xAxis = lineChart.EnsureChartComponent<XAxis>();
        xAxis.ClearData();
        // 设置x轴类型为时间
        xAxis.type = Axis.AxisType.Category;
        //xAxis.axisLabel.show = true;
        //xAxis.axisLabel.formatter = "{value}";
        //xAxis.splitLine.show = true;
        xAxis.data.Clear();
        for (int i = 0; i < chartData.xAxis.label.Count; i++)
        {
            lineChart.AddXAxisData(chartData.xAxis.label[i].ToString());
        }

        // 设置Y轴
        var yAxis = lineChart.EnsureChartComponent<YAxis>();
        yAxis.ClearData();
        yAxis.type = Axis.AxisType.Value;


        // 更新系列数据
        foreach (var series in chartData.series)
        {
            // 检查是否已存在同名的系列
            var serie = lineChart.GetSerie(series.name);
            if (serie == null)
            {
                // 如果不存在，则添加一个新的系列
                serie = lineChart.AddSerie<Line>(series.name);
            }
            else
            {
                // 如果存在，确保它是一个Line类型
                if (serie is Line lineSerie)
                {
                    // 清除现有数据点
                    lineSerie.ClearData();
                }
                else
                {
                    Debug.LogError($"Series with name '{series.name}' exists but is not of type Line.");
                    continue;
                }
            }

            // 如果display为false，则隐藏系列
            serie.show = series.display;

            // 添加新的数据点
            foreach (var dataPoint in series.data)
            {
                // 获取dataPoint.time在xAxis.categories中的索引
                int index = chartData.xAxis.label.IndexOf(dataPoint.key);
                // 如果索引大于等于0，则将dataPoint.data添加到serie中
                if (index >= 0)
                {
                    serie.AddData(index, dataPoint.value);
                }
            }
        }
    }

    // 当对象被销毁时调用
    private void OnDestroy()
    {
        // 取消调用LoadJsonData方法
        CancelInvoke("LoadDatas");
    }
    
}
