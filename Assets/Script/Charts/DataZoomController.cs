using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XCharts.Runtime;

public class DataZoomController : MonoBehaviour
{
    public Slider dataZoomSlider;
    public BaseChart[] charts;

    void Start()
    {
        dataZoomSlider.minValue = 0;
        dataZoomSlider.maxValue = 100;

        dataZoomSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnSliderValueChanged(float value)
    {
        foreach (var chart in charts)
        {
            DataZoom dataZoomComponent = chart.EnsureChartComponent<DataZoom>();
            if (dataZoomComponent != null)
            {

                float start = Mathf.Clamp(value - 10, 0, 90);
                float end = Mathf.Clamp(value, 10, 100);


                dataZoomComponent.start = start;
                dataZoomComponent.end = end;

                XAxis xAxis = chart.EnsureChartComponent<XAxis>(); xAxis.refreshComponent();
                xAxis.refreshComponent();
                chart.RefreshChart();
            }
        }
    }

    void OnDestroy()
    {
        if (dataZoomSlider != null)
        {
            dataZoomSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }
}
