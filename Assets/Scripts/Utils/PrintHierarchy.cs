using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;

public class PrintHierarchy : MonoBehaviour
{
    [ContextMenu("Print Full Hierarchy (Single Log)")]
    void Print()
    {
        StringBuilder sb = new StringBuilder();

        void PrintChildren(Transform t, string indent)
        {
            sb.AppendLine(indent + "- " + t.name);
            foreach (Transform child in t)
                PrintChildren(child, indent + "  ");
        }

        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();

        foreach (var root in roots)
            PrintChildren(root.transform, "");

        Debug.Log(sb.ToString());
    }
}
