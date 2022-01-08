using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class IconGenerator : MonoBehaviour
{
    Camera cam;
    Item[] sceneObjects;

    private void Start()
    {
        sceneObjects = GetComponentsInChildren<Item>();
        foreach (Transform child in transform)
            child.gameObject.SetActive(false);
        StartCoroutine(Screenshot());
    }

    private IEnumerator Screenshot()
    {
        for (int i = 0; i < sceneObjects.Length; i++)
        {
            Item item = sceneObjects[i];
            item.gameObject.SetActive(true);
            yield return null;
            TakeScreenshot($"{Application.dataPath}/Icons/{item.displayName}_Icon.png");
            yield return null;
            item.gameObject.SetActive(false);
        }
    }

    void TakeScreenshot(string fullPath)
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }

        RenderTexture rt = new RenderTexture(1080, 1080, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(1080, 1080, TextureFormat.RGBA32, false);
        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, 1080, 1080), 0, 0);
        cam.targetTexture = null;
        RenderTexture.active = null;

        if (Application.isEditor)
        {
            DestroyImmediate(rt);
        }
        else
        {
            Destroy(rt);
        }

        byte[] bytes = screenShot.EncodeToPNG();
        System.IO.File.WriteAllBytes(fullPath, bytes);
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}
