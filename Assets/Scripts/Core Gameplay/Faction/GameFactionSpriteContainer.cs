using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "GameFactionSpriteContainer", menuName = "Scriptable Objects/GameFactionSpriteContainer")]
public class GameFactionSpriteContainer : ScriptableObject
{
    public Sprite[] factionSprites;
}

#if UNITY_EDITOR
[CustomEditor(typeof(GameFactionSpriteContainer))]
public class GameFactionSpriteContainerInspector : Editor
{
    private string folderPath = "Assets";

    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        GameFactionSpriteContainer container = (GameFactionSpriteContainer)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Auto-Assign Sprites", EditorStyles.boldLabel);

        // Folder picker
        EditorGUILayout.BeginHorizontal();
        folderPath = EditorGUILayout.TextField("Sprite Folder", folderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string selected = EditorUtility.OpenFolderPanel("Select Sprite Folder", folderPath, "");
            if (!string.IsNullOrEmpty(selected))
            {
                if (selected.StartsWith(Application.dataPath))
                    folderPath = "Assets" + selected.Substring(Application.dataPath.Length);
                else
                    Debug.LogError("Please select a folder inside the Assets folder.");
            }
        }
        EditorGUILayout.EndHorizontal();

        // Assign button
        if (GUILayout.Button("Assign Sprites"))
        {
            AssignSprites(container, folderPath);
        }
    }

    private void AssignSprites(GameFactionSpriteContainer container, string path)
    {
        string[] assetGUIDs = AssetDatabase.FindAssets("t:Sprite", new[] { path });
        Sprite[] sprites = assetGUIDs
            .Select(guid => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)))
            .ToArray();

        if (sprites.Length == 0)
        {
            EditorUtility.DisplayDialog("No Sprites Found", "No sprites found in folder: " + path, "OK");
            return;
        }

        container.factionSprites = sprites;
        EditorUtility.SetDirty(container);
        EditorUtility.DisplayDialog("Success", $"Assigned {sprites.Length} sprites to {container.name}.", "OK");
    }
}
#endif
