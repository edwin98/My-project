using UnityEngine;

/// <summary>
/// This script is created to help resolve Burst compilation issues.
/// It provides a minimal, clean script that should compile successfully.
/// </summary>
public class BurstCompilationFix : MonoBehaviour
{
    [Header("Burst Compilation Fix")]
    [SerializeField] private bool enableDebugLogging = false;
    
    void Start()
    {
        if (enableDebugLogging)
        {
            Debug.Log("BurstCompilationFix: Script loaded successfully");
        }
    }
    
    void Update()
    {
        // Empty update method to ensure the script is active
    }
} 