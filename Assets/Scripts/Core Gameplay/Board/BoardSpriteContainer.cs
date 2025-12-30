#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "BoardSpriteContainer", menuName = "ScriptableObjects/BoardSpriteContainer")]
public class BoardSpriteContainer : ScriptableObject
{
    public Sprite[] boardSprites;
    public Sprite[] boardUpperSprites;
}

#if UNITY_EDITOR
[CustomEditor(typeof(BoardSpriteContainer))]
public class BoardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BoardSpriteContainer board = (BoardSpriteContainer)target;

        if (GUILayout.Button("Load Board Sprites From Folder"))
        {
            LoadSprites(board, false);
        }

        if (GUILayout.Button("Load Board Upper Sprites From Folder"))
        {
            LoadSprites(board, true);
        }
    }

    private void LoadSprites(BoardSpriteContainer board, bool isBoardUpper)
    {
        string folderPath = EditorUtility.OpenFolderPanel(
            "Select Sprite Folder",
            "Assets",
            ""
        );

        if (string.IsNullOrEmpty(folderPath))
            return;

        // Convert absolute path to relative Unity path
        folderPath = "Assets" + folderPath.Substring(Application.dataPath.Length);

        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

        if (isBoardUpper)
        {
            board.boardUpperSprites = new Sprite[guids.Length];
        }
        else
        {
            board.boardSprites = new Sprite[guids.Length];
        }

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

            if (isBoardUpper)
            {
                board.boardUpperSprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            }
            else
            {
                board.boardSprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            }
        }

        EditorUtility.SetDirty(board);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Loaded {board.boardSprites.Length} sprites from {folderPath}");
    }
}
#endif
