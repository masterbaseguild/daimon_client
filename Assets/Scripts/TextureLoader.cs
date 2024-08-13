using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public static class TextureLoader
{
    public static void LoadTexture(string url, Action<Texture2D> onTextureLoaded)
    {
        CoroutineRunner.Instance.StartCoroutine(LoadTextureCoroutine(url, onTextureLoaded));
    }

    private static IEnumerator LoadTextureCoroutine(string url, Action<Texture2D> onTextureLoaded)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                onTextureLoaded?.Invoke(null);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                onTextureLoaded?.Invoke(texture);
            }
        }
    }
}
