using System.Collections.Generic;
using UnityEngine;

public class GameObjectHierarchyPrinter : MonoBehaviour
{
    public List<GameObject> gameObjects; // Assign this in the Inspector

    void Start()
    {
        foreach (GameObject obj in gameObjects)
        {
            PrintHierarchy(obj, 0);
        }
    }
    public string completeText = "";
    void PrintHierarchy(GameObject obj, int depth)
    {
        string prefix = new string('-', depth * 2);
        Debug.Log(prefix + obj.name);
        completeText += prefix + obj.name + "\n";
        foreach (Transform child in obj.transform)
        {
            PrintHierarchy(child.gameObject, depth + 1);
        }
    }
}
