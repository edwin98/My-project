using UnityEngine;
using UnityEngine.UI;
using DrillingData;

namespace DrillingData
{
    /// <summary>
    /// 三维轨迹可视化场景设置器
    /// </summary>
    public class Trajectory3DSceneSetup : MonoBehaviour
    {
        [Header("场景设置")]
        public bool autoSetupOnStart = true;
        public bool createUI = true;
        public bool createLighting = true;
        
        [Header("预制体引用")]
        public GameObject uiCanvasPrefab;
        public Material defaultLineMaterial;
        public Material defaultPointMaterial;
        public Material defaultAxisMaterial;
        
        void Start()
        {
            if (autoSetupOnStart)
            {
                SetupScene();
            }
        }
        
        /// <summary>
        /// 设置场景
        /// </summary>
        [ContextMenu("设置场景")]
        public void SetupScene()
        {
            Debug.Log("开始设置三维轨迹可视化场景...");
            
            // 设置相机
            SetupCamera();
            
            // 设置光照
            if (createLighting)
            {
                SetupLighting();
            }
            
            // 创建可视化组件
            SetupVisualizationComponents();
            
            // 创建UI
            if (createUI)
            {
                SetupUI();
            }
            
            Debug.Log("场景设置完成！");
        }
        
        /// <summary>
        /// 设置相机
        /// </summary>
        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }
            
            // 设置相机参数
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            mainCamera.fieldOfView = 60f;
            mainCamera.nearClipPlane = 0.1f;
            mainCamera.farClipPlane = 1000f;
            
            // 设置相机位置
            mainCamera.transform.position = new Vector3(10f, 10f, 10f);
            mainCamera.transform.LookAt(Vector3.zero);
            
            Debug.Log("相机设置完成");
        }
        
        /// <summary>
        /// 设置光照
        /// </summary>
        private void SetupLighting()
        {
            // 查找或创建主光源
            Light mainLight = FindObjectOfType<Light>();
            if (mainLight == null)
            {
                GameObject lightObject = new GameObject("Directional Light");
                mainLight = lightObject.AddComponent<Light>();
            }
            
            // 设置光源参数
            mainLight.type = LightType.Directional;
            mainLight.intensity = 1f;
            mainLight.color = Color.white;
            mainLight.shadows = LightShadows.Soft;
            mainLight.shadowStrength = 0.8f;
            mainLight.shadowBias = 0.05f;
            mainLight.shadowNormalBias = 0.4f;
            
            // 设置光源位置和旋转
            mainLight.transform.position = new Vector3(10f, 10f, 10f);
            mainLight.transform.LookAt(Vector3.zero);
            
            // 创建环境光
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.3f, 0.3f, 0.3f);
            RenderSettings.ambientEquatorColor = new Color(0.2f, 0.2f, 0.2f);
            RenderSettings.ambientGroundColor = new Color(0.1f, 0.1f, 0.1f);
            
            Debug.Log("光照设置完成");
        }
        
        /// <summary>
        /// 设置可视化组件
        /// </summary>
        private void SetupVisualizationComponents()
        {
            // 创建数据处理器
            GameObject processorObject = new GameObject("DrillingDataProcessor");
            UnityDrillingDataProcessor dataProcessor = processorObject.AddComponent<UnityDrillingDataProcessor>();
            
            // 创建可视化器
            GameObject visualizerObject = new GameObject("Trajectory3DVisualizer");
            Trajectory3DVisualizer visualizer = visualizerObject.AddComponent<Trajectory3DVisualizer>();
            
            // 设置引用
            visualizer.dataProcessor = dataProcessor;
            visualizer.targetCamera = Camera.main;
            
            // 设置材质
            if (defaultLineMaterial != null)
                visualizer.lineMaterial = defaultLineMaterial;
            if (defaultPointMaterial != null)
                visualizer.pointMaterial = defaultPointMaterial;
            if (defaultAxisMaterial != null)
                visualizer.axisMaterial = defaultAxisMaterial;
            
            Debug.Log("可视化组件设置完成");
        }
        
        /// <summary>
        /// 设置UI
        /// </summary>
        private void SetupUI()
        {
            // 创建Canvas
            GameObject canvasObject = new GameObject("Trajectory3DCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1;
            
            // 添加CanvasScaler
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            // 添加GraphicRaycaster
            canvasObject.AddComponent<GraphicRaycaster>();
            
            // 创建UI控制器
            GameObject controllerObject = new GameObject("Trajectory3DController");
            controllerObject.transform.SetParent(canvasObject.transform);
            Trajectory3DController controller = controllerObject.AddComponent<Trajectory3DController>();
            
            // 设置控制器引用
            controller.dataProcessor = FindObjectOfType<UnityDrillingDataProcessor>();
            controller.visualizer = FindObjectOfType<Trajectory3DVisualizer>();
            controller.targetCamera = Camera.main;
            
            // 创建UI面板
            CreateUIPanel(controllerObject);
            
            Debug.Log("UI设置完成");
        }
        
        /// <summary>
        /// 创建UI面板
        /// </summary>
        private void CreateUIPanel(GameObject parent)
        {
            // 创建主面板
            GameObject panelObject = new GameObject("ControlPanel");
            panelObject.transform.SetParent(parent.transform);
            
            RectTransform panelRect = panelObject.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.offsetMin = new Vector2(10, 10);
            panelRect.offsetMax = new Vector2(300, -10);
            
            Image panelImage = panelObject.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);
            
            // 创建标题
            CreateUIText(panelObject, "标题", "三维轨迹可视化控制", new Vector2(0, 0.95f), new Vector2(1, 0.05f), 24, Color.white);
            
            // 创建控制按钮
            float buttonY = 0.85f;
            float buttonHeight = 0.08f;
            float buttonSpacing = 0.02f;
            
            CreateUIButton(panelObject, "ResetViewButton", "重置视角", new Vector2(0.05f, buttonY), new Vector2(0.9f, buttonHeight));
            buttonY -= buttonHeight + buttonSpacing;
            
            CreateUIButton(panelObject, "AutoRotateButton", "开始旋转", new Vector2(0.05f, buttonY), new Vector2(0.9f, buttonHeight));
            buttonY -= buttonHeight + buttonSpacing;
            
            CreateUIButton(panelObject, "ToggleLineButton", "隐藏轨迹线", new Vector2(0.05f, buttonY), new Vector2(0.9f, buttonHeight));
            buttonY -= buttonHeight + buttonSpacing;
            
            CreateUIButton(panelObject, "TogglePointsButton", "隐藏轨迹点", new Vector2(0.05f, buttonY), new Vector2(0.9f, buttonHeight));
            buttonY -= buttonHeight + buttonSpacing;
            
            CreateUIButton(panelObject, "ToggleLabelsButton", "隐藏深度标签", new Vector2(0.05f, buttonY), new Vector2(0.9f, buttonHeight));
            buttonY -= buttonHeight + buttonSpacing;
            
            CreateUIButton(panelObject, "ToggleAxisButton", "隐藏坐标轴", new Vector2(0.05f, buttonY), new Vector2(0.9f, buttonHeight));
            buttonY -= buttonHeight + buttonSpacing;
            
            // 创建颜色方案下拉框
            CreateUIDropdown(panelObject, "ColorSchemeDropdown", "颜色方案", new Vector2(0.05f, buttonY), new Vector2(0.9f, buttonHeight));
            buttonY -= buttonHeight + buttonSpacing;
            
            // 创建滑块
            CreateUISlider(panelObject, "LineWidthSlider", "线宽", new Vector2(0.05f, buttonY), new Vector2(0.9f, buttonHeight));
            buttonY -= buttonHeight + buttonSpacing;
            
            CreateUISlider(panelObject, "PointSizeSlider", "点大小", new Vector2(0.05f, buttonY), new Vector2(0.9f, buttonHeight));
            buttonY -= buttonHeight + buttonSpacing;
            
            // 创建状态文本
            CreateUIText(panelObject, "StatusText", "状态: 就绪", new Vector2(0.05f, 0.1f), new Vector2(0.9f, 0.05f), 14, Color.white);
            CreateUIText(panelObject, "PointCountText", "轨迹点数量: 0", new Vector2(0.05f, 0.05f), new Vector2(0.9f, 0.05f), 14, Color.white);
            CreateUIText(panelObject, "DepthRangeText", "深度范围: 0.0m - 0.0m", new Vector2(0.05f, 0.0f), new Vector2(0.9f, 0.05f), 14, Color.white);
        }
        
        /// <summary>
        /// 创建UI文本
        /// </summary>
        private void CreateUIText(GameObject parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, int fontSize, Color color)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent.transform);
            
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            Text textComponent = textObject.AddComponent<Text>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.color = color;
            textComponent.alignment = TextAnchor.MiddleLeft;
        }
        
        /// <summary>
        /// 创建UI按钮
        /// </summary>
        private void CreateUIButton(GameObject parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent.transform);
            
            RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
            buttonRect.anchorMin = anchorMin;
            buttonRect.anchorMax = anchorMax;
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            
            // 创建按钮文本
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(buttonObject.transform);
            
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            Text textComponent = textObject.AddComponent<Text>();
            textComponent.text = text;
            textComponent.fontSize = 16;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;
        }
        
        /// <summary>
        /// 创建UI下拉框
        /// </summary>
        private void CreateUIDropdown(GameObject parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject dropdownObject = new GameObject(name);
            dropdownObject.transform.SetParent(parent.transform);
            
            RectTransform dropdownRect = dropdownObject.AddComponent<RectTransform>();
            dropdownRect.anchorMin = anchorMin;
            dropdownRect.anchorMax = anchorMax;
            dropdownRect.offsetMin = Vector2.zero;
            dropdownRect.offsetMax = Vector2.zero;
            
            Image dropdownImage = dropdownObject.AddComponent<Image>();
            dropdownImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            Dropdown dropdown = dropdownObject.AddComponent<Dropdown>();
            dropdown.targetGraphic = dropdownImage;
            
            // 创建标签文本
            CreateUIText(parent, name + "Label", label, anchorMin, new Vector2(anchorMin.x + 0.3f, anchorMax.y), 14, Color.white);
        }
        
        /// <summary>
        /// 创建UI滑块
        /// </summary>
        private void CreateUISlider(GameObject parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject sliderObject = new GameObject(name);
            sliderObject.transform.SetParent(parent.transform);
            
            RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
            sliderRect.anchorMin = anchorMin;
            sliderRect.anchorMax = anchorMax;
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;
            
            Slider slider = sliderObject.AddComponent<Slider>();
            
            // 创建标签文本
            CreateUIText(parent, name + "Label", label, anchorMin, new Vector2(anchorMin.x + 0.3f, anchorMax.y), 14, Color.white);
        }
        
        /// <summary>
        /// 清理场景
        /// </summary>
        [ContextMenu("清理场景")]
        public void CleanupScene()
        {
            // 删除可视化相关组件
            Trajectory3DVisualizer[] visualizers = FindObjectsOfType<Trajectory3DVisualizer>();
            foreach (var viz in visualizers)
            {
                DestroyImmediate(viz.gameObject);
            }
            
            Trajectory3DController[] controllers = FindObjectsOfType<Trajectory3DController>();
            foreach (var controller in controllers)
            {
                DestroyImmediate(controller.gameObject);
            }
            
            // 删除UI Canvas
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (var canvas in canvases)
            {
                if (canvas.name.Contains("Trajectory3D"))
                {
                    DestroyImmediate(canvas.gameObject);
                }
            }
            
            Debug.Log("场景清理完成");
        }
    }
} 