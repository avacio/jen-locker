using UnityEngine;
using UnityEditor;

public class BatchApplyMaterial : EditorWindow
{
    private Material targetMaterial;

    [MenuItem("Tools/Batch Apply Material to Children")]
    public static void ShowWindow()
    {
        GetWindow<BatchApplyMaterial>("Batch Apply Material");
    }

    private void OnGUI()
    {
        targetMaterial = (Material)EditorGUILayout.ObjectField("New Material", targetMaterial, typeof(Material), false);

        if (GUILayout.Button("Apply to Selected & Children") && targetMaterial != null)
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
                Undo.RecordObjects(renderers, "Batch Apply Material");
                foreach (Renderer renderer in renderers)
                {
                    Material[] newMats = new Material[renderer.sharedMaterials.Length];
                    for (int i = 0; i < newMats.Length; i++)
                    {
                        newMats[i] = targetMaterial;
                    }
                    renderer.sharedMaterials = newMats;
                }
            }
        }
    }
}