using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public class UserData
{
    public string key;
    public float strength;
    public float cohesive;
    public float friction;
    public float crack;
}

public class SetPage : MonoBehaviour
{
    public Transform content;

    public string jsonFilePath;

    private void Start()
    {
        LoadJsonUserData();
        InvokeRepeating("LoadJsonUserData", 3f, 3f);
    }

    private void LoadJsonUserData()
    {
        StartCoroutine(LoadJsonUserDataCoroutine());
    }

    IEnumerator LoadJsonUserDataCoroutine()
    {
        string url = Application.streamingAssetsPath + jsonFilePath;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonUserData = webRequest.downloadHandler.text;

                // Wrap the JSON array in an object with a "data" field
                jsonUserData = "{\"data\":" + jsonUserData + "}";

                // Now the JSON string is in the correct format for the Wrapper class
                Wrapper UserDataWrapper = JsonUtility.FromJson<Wrapper>(jsonUserData);
                List<UserData> userList = UserDataWrapper.data;

                // Clear previous user rows
                foreach (Transform child in content)
                {
                    Destroy(child.gameObject);
                }

                for (int i = 0; i < userList.Count; i++)
                {
                    // Load the prefab and check if it's not null
                    GameObject prefab = Resources.Load<GameObject>("Prefabs/UserRow");
                    if (prefab == null)
                    {
                        Debug.LogError("Prefab 'Prefabs/UserRow' could not be loaded. Check the path and ensure it exists in the Resources folder.");
                        continue; // Skip this iteration
                    }

                    GameObject go = Instantiate(prefab, content) as GameObject;
                    if (i % 2 != 0)
                    {
                        for (int j = 0; j < go.transform.GetChild(0).childCount; j++)
                        {
                            Image image = go.transform.GetChild(0).GetChild(j).GetComponent<Image>();
                            if (image != null)
                            {
                                image.sprite = Resources.Load<Sprite>("UI/样式B-1");
                            }
                            else
                            {
                                Debug.LogError("Image component not found on child " + j + " of UserRow prefab.");
                            }
                        }
                    }
                    UserRow userRow = go.GetComponent<UserRow>();
                    if (userRow != null)
                    {
                        userRow.key.text = userList[i].key;
                        userRow.strength.text = userList[i].strength.ToString("F2");
                        userRow.cohesive.text = userList[i].cohesive.ToString("F2");
                        userRow.frivtion.text = userList[i].friction.ToString("F2");
                        userRow.crack.text = userList[i].crack.ToString("F2");
                    }
                    else
                    {
                        Debug.LogError("UserRow script not found on the loaded prefab.");
                    }
                }

            }
            else
            {
                Debug.LogError("Error downloading JSON UserData: " + webRequest.error);
            }
        }
    }
}

// The Wrapper class remains the same
[System.Serializable]
public class Wrapper
{
    public List<UserData> data;
}
