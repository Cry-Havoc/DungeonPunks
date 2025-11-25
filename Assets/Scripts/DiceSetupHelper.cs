using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper script to set up die face rotations in the editor
/// </summary>
public class DiceSetupHelper : MonoBehaviour
{
    public Die die;

    [Header("Face Setup")]
    [Range(0, 9)]
    public int currentFaceValue = 0;

    [ContextMenu("Save Current Rotation to Face")]
    public void SaveCurrentRotation()
    {
        if (die == null)
        {
            Debug.LogError("Die reference not set!");
            return;
        }

        if (currentFaceValue < 0 || currentFaceValue > 9)
        {
            Debug.LogError("Face value must be 0-9");
            return;
        }

        die.faces[currentFaceValue].rotation = die.transform.rotation;
        Debug.Log($"Saved rotation for face {currentFaceValue}: {die.transform.rotation.eulerAngles}");

#if UNITY_EDITOR
        EditorUtility.SetDirty(die);
#endif
    }

    [ContextMenu("Test Roll Current Face")]
    public void TestRoll()
    {
        if (die == null) return;
        die.RollToValue(currentFaceValue, 2f);
    }

    [ContextMenu("Initialize Face Array")]
    public void InitializeFaces()
    {
        if (die == null) return;

        die.faces = new DiceFace[10];
        for (int i = 0; i < 10; i++)
        {
            die.faces[i] = new DiceFace(i, Vector3.zero);
        }

        Debug.Log("Initialized 10 face slots");

#if UNITY_EDITOR
        EditorUtility.SetDirty(die);
#endif
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(DiceSetupHelper))]
public class DiceSetupHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DiceSetupHelper helper = (DiceSetupHelper)target;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "SETUP INSTRUCTIONS:\n" +
            "1. Click 'Initialize Face Array' to create face slots\n" +
            "2. Manually rotate the die so face shows the number you want\n" +
            "3. Set 'Current Face Value' to that number (0 = ten)\n" +
            "4. Click 'Save Current Rotation to Face'\n" +
            "5. Repeat for all 10 faces (0-9)\n" +
            "6. Test with 'Test Roll Current Face'",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Initialize Face Array", GUILayout.Height(30)))
        {
            helper.InitializeFaces();
        }

        if (GUILayout.Button("Save Current Rotation to Face", GUILayout.Height(30)))
        {
            helper.SaveCurrentRotation();
        }

        if (GUILayout.Button("Test Roll Current Face", GUILayout.Height(30)))
        {
            helper.TestRoll();
        }
    }
}
#endif