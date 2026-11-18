using UnityEditor;
using UnityEngine;
using System.IO;

public class ExportLockerTexture
{
    [MenuItem("Tools/Export Locker Texture to PNG")]
    public static void Export()
    {
        string texturePath = "Assets/Models/image_0.texture2D";
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

        if (texture == null)
        {
            Debug.LogError("Could not find texture at: " + texturePath);
            return;
        }

        // Make a readable copy of the texture
        RenderTexture rt = RenderTexture.GetTemporary(texture.width, texture.height);
        Graphics.Blit(texture, rt);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D readableTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        readableTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readableTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        // Save to PNG
        byte[] pngBytes = readableTexture.EncodeToPNG();
        string outputPath = "Assets/Models/school_locker_texture.png";
        File.WriteAllBytes(outputPath, pngBytes);

        AssetDatabase.Refresh();
        Debug.Log("Texture exported to: " + outputPath);
    }
}
