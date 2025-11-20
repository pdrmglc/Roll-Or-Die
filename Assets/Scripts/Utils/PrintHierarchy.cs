using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;

public class PrintHierarchy : MonoBehaviour
{
    [ContextMenu("Print Full Hierarchy (Single Log + Components)")]
    void Print()
    {
        StringBuilder sb = new StringBuilder();

        void PrintChildren(Transform t, string indent)
        {
            // Nome do GameObject
            sb.AppendLine(indent + "- " + t.name);

            // Componentes do GameObject
            var comps = t.GetComponents<Component>();
            foreach (var c in comps)
            {
                if (c == null)
                {
                    sb.AppendLine(indent + "    [Missing Component]");
                    continue;
                }

                // Nome do tipo do componente
                string typeName = c.GetType().Name;

                // Se for um Behaviour (MonoBehaviour, etc), mostramos se está enabled
                var behaviour = c as Behaviour;
                if (behaviour != null)
                {
                    sb.AppendLine(indent + "    - " + typeName + " (enabled: " + behaviour.enabled + ")");
                }
                else
                {
                    sb.AppendLine(indent + "    - " + typeName);
                }
            }

            // Recurse para os filhos
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
