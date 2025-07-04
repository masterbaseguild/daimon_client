using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using Dummiesman;

public class ModelPalette
{
    private readonly List<string> modelUrls = new();
    private readonly List<string> models = new();

    public ModelPalette(string[] ids)
    {
        foreach (string id in ids)
        {
            modelUrls.Add($"https://media.daimon.world/public/models/{id}.obj");
            LoadModel(modelUrls[modelUrls.Count - 1], (modelData) =>
            {
                models.Add(modelData);
                // debug: instantiate model
                Vector3 spawnpoint = new Vector3(80, 64, 94);
                InstantiateModel(models.Count - 1, spawnpoint, Quaternion.identity);
            });
        }
        Debug.Log("ModelPalette initialized with " + modelUrls.Count + " models.");
    }

    public static void LoadModel(string url, Action<string> onModelLoaded)
    {
        // since this coroutine uses the unity networking api, it must be run on the main thread
        _ = MainThreadDispatcher.Instance.StartCoroutine(LoadModelCoroutine(url, onModelLoaded));
    }

    private static IEnumerator LoadModelCoroutine(string url, Action<string> onModelLoaded)
    {
        using UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to load model from {url}: {www.error}");
            onModelLoaded?.Invoke(null);
        }
        else
        {
            string model = DownloadHandlerBuffer.GetContent(www);
            Debug.Log($"Model loaded from {url}");
            Debug.Log(model);
            onModelLoaded?.Invoke(model);
        }
    }

    public string GetModel(int index)
    {
        return models[index];
    }

    public void InstantiateModel(int index, Vector3 position, Quaternion rotation)
    {
        Vector3 offset = new Vector3(-0.5f, 0.5f, 0.5f);
        string modelData = GetModel(index);
        if (modelData != null)
        {
            using (var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(modelData)))
            {
                GameObject loadedObject = new OBJLoader().Load(stream);
                if (loadedObject == null)
                {
                    Debug.LogError("Failed to instantiate model from OBJ data.");
                    return;
                }

                loadedObject.transform.position = position;
                loadedObject.transform.rotation = rotation;
                loadedObject.name = $"Model_{index}";
                // add collider
                GameObject child = loadedObject.transform.GetChild(0).gameObject;
                child.transform.localPosition = offset;
                MeshCollider meshCollider = child.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = child.GetComponent<MeshFilter>().mesh;
                loadedObject.layer = LayerMask.NameToLayer("Ground");
                Debug.Log($"Model {index} instantiated at {position} with rotation {rotation}.");
            }
        }
        else
        {
            Debug.LogError($"Model data at index {index} is null.");
        }
    }
}