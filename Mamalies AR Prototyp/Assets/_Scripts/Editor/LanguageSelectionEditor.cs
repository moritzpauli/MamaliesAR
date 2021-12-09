using UnityEditor;


[CustomEditor(typeof(LanguageSelection))]
public class LanguageSelectionEditor : Editor
{
    private SerializedProperty sizeWarningString;
    private SerializedProperty noConnectionWarningString;
    private SerializedProperty downloadMessageString;
    private SerializedProperty deletionMessageString;
    private SerializedProperty cancelDownloadMessageString;

    private SerializedProperty sizeWarningObject;
    private SerializedProperty noConnectionWarningObject;
    private SerializedProperty downloadMessageObject;
    private SerializedProperty deletionMessageObject;
    private SerializedProperty cancelDownloadMessageObject;


    private SerializedProperty languageButtons;
    private SerializedProperty languageAddressablesManager;
    private SerializedProperty germanToggle;

    private SerializedProperty recognitionSceneName;
    private SerializedProperty cachedTextColor;
    private SerializedProperty remoteTextColor;

    private SerializedProperty startButtonGrayout;

    private SerializedProperty loadingPanel;

    private bool showMessageStrings = false;
    private bool showDependencies = false; 

    private void OnEnable()
    {
        sizeWarningString = serializedObject.FindProperty("sizeWarningString");
        noConnectionWarningString = serializedObject.FindProperty("noConnectionWarningString");
        downloadMessageString = serializedObject.FindProperty("downloadMessageString");
        deletionMessageString = serializedObject.FindProperty("deletionMessageString");
        cancelDownloadMessageString = serializedObject.FindProperty("cancelDownloadMessageString");

        sizeWarningObject = serializedObject.FindProperty("sizeWarningObject");
        noConnectionWarningObject = serializedObject.FindProperty("noConnectionWarningObject");
        downloadMessageObject = serializedObject.FindProperty("downloadMessageObject");
        deletionMessageObject = serializedObject.FindProperty("deletionMessageObject");
        cancelDownloadMessageObject = serializedObject.FindProperty("cancelDownloadMessageObject");


        languageButtons = serializedObject.FindProperty("languageButtons");
        languageAddressablesManager = serializedObject.FindProperty("languageAddressablesManager");

        recognitionSceneName = serializedObject.FindProperty("recognitionSceneName");
        cachedTextColor = serializedObject.FindProperty("cachedTextColor");
        remoteTextColor = serializedObject.FindProperty("remoteTextColor");

        startButtonGrayout = serializedObject.FindProperty("startButtonGrayout");
        loadingPanel = serializedObject.FindProperty("loadingPanel");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        showMessageStrings = EditorGUILayout.Foldout(showMessageStrings, "Player Messages");
        if (showMessageStrings)
        {
            EditorGUILayout.PropertyField(sizeWarningString);
            EditorGUILayout.PropertyField(noConnectionWarningString);
            EditorGUILayout.PropertyField(downloadMessageString);
            EditorGUILayout.PropertyField(deletionMessageString);
            EditorGUILayout.PropertyField(cancelDownloadMessageString);

            EditorGUILayout.PropertyField(sizeWarningObject);
            EditorGUILayout.PropertyField(noConnectionWarningObject);
            EditorGUILayout.PropertyField(downloadMessageObject);
            EditorGUILayout.PropertyField(deletionMessageObject);
            EditorGUILayout.PropertyField(cancelDownloadMessageObject);
        }

        showDependencies = EditorGUILayout.Foldout(showDependencies, "Other Dependencies");
        if (showDependencies)
        {
            EditorGUILayout.PropertyField(languageButtons);
            EditorGUILayout.PropertyField(languageAddressablesManager);
            EditorGUILayout.PropertyField(startButtonGrayout);
            EditorGUILayout.PropertyField(loadingPanel);
        }

        EditorGUILayout.PropertyField(recognitionSceneName);
        EditorGUILayout.PropertyField(cachedTextColor);
        EditorGUILayout.PropertyField(remoteTextColor);

        serializedObject.ApplyModifiedProperties();

    }

}
