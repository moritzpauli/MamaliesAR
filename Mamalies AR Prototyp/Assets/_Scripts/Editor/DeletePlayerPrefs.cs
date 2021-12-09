using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DeletePlayerPrefs : MonoBehaviour
{
    [MenuItem("PlayerPrefs/Delete PlayerPrefs")]
    static void RemoveTestingImages()
    {
        if (EditorUtility.DisplayDialog("Delete Player Prefs",
            "Do you want to delete the saved player Prefs?",
            "Delete Player Prefs", "Cancel"))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("Deleted Player Prefs!");
        }
    }
}
