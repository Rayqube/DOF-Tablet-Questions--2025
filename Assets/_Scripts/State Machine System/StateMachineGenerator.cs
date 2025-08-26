#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;

public class StateMachineGenerator : MonoBehaviour
{
    [SerializeField] private string stateMachineDirectoryPath = "Systems/State Machine System/";

    [Button]
    public void GenerateStateMachineFiles()
    {
        string fullPath = Path.Combine(Application.dataPath, stateMachineDirectoryPath);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        int maxLevel = GetMaxDepth(transform, 1);
        for (int i = 1; i <= maxLevel; i++)
        {
            string levelStateFilePath = Path.Combine(fullPath, $"Level{i}State.cs");
            if (!File.Exists(levelStateFilePath))
            {
                File.WriteAllText(levelStateFilePath, GenerateLevelStateClass(i));
            }
        }

        GenerateStateMachine(transform, fullPath, 1);

        AssetDatabase.Refresh();
    }

    private int GetMaxDepth(Transform parent, int level)
    {
      
        int maxDepth = level;

        if (parent.childCount == 0)
        { return  maxDepth - 1; }

        foreach (Transform child in parent)
        {
            maxDepth = Mathf.Max(maxDepth, GetMaxDepth(child, level + 1));
        }
        return maxDepth;
    }

    private void GenerateStateMachine(Transform parent, string directoryPath, int level)
    {
        foreach (Transform child in parent)
        {
            string childDir = Path.Combine(directoryPath, child.name);
            if (!Directory.Exists(childDir))
            {
                Directory.CreateDirectory(childDir);
            }

            string stateClassName = child.name + "State";
            string stateMachineClassName = child.name + "StateMachine";
            string stateFilePath = Path.Combine(childDir, stateClassName + ".cs");
            string stateMachineFilePath = Path.Combine(childDir, stateMachineClassName + ".cs");

            if (!File.Exists(stateFilePath))
            {
                File.WriteAllText(stateFilePath, GenerateStateClass(stateClassName, level));
            }

            if (child.childCount > 0 && !File.Exists(stateMachineFilePath))
            {
                File.WriteAllText(stateMachineFilePath, GenerateStateMachineClass(stateMachineClassName));
            }

            GenerateStateMachine(child, childDir, level + 1);
        }
    }

    private string GenerateStateClass(string className, int level)
    {
        string baseClass = $"Level{level}State";
        return $"using UnityEngine;\n\npublic class {className} : {baseClass} \n{{\n    public override void Enter(string message = \"\", State previousState = null)\n    {{\n        base.Enter(message,previousState);\n    }}\n    public override void Exit(string message = \"\", State newState = null)\n    {{\n        base.Exit(message,newState);\n    }}\n    public override void Tick()\n    {{\n        base.Tick();\n    }}\n}}";
    }

    private string GenerateStateMachineClass(string className)
    {
        return $"using UnityEngine;\nusing System;\n\npublic class {className} : StateMachine \n{{\n    public static {className} Instance {{ get; private set; }}\n\n    public override void Awake()\n    {{\n        if (Instance != null && Instance != this)\n        {{\n            Destroy(gameObject);\n            return;\n        }}\n        Instance = this;\n        base.Awake();\n    }}\n\n    public override void Update()\n    {{\n        base.Update();\n    }}\n}}";
    }

    private string GenerateLevelStateClass(int level)
    {
        return $"using UnityEngine;\n\npublic class Level{level}State : State \n{{\n    public override void Enter(string message = \"\", State previousState = null)\n    {{\n        base.Enter(message,previousState);\n    }}\n    public override void Exit(string message = \"\", State newState = null)\n    {{\n        base.Exit(message,newState);\n    }}\n    public override void Tick()\n    {{\n        base.Tick();\n    }}\n}}";
    }


    [Button]
    public void AssignStateMachineScripts()
    {
        FindAndAssignScripts(transform, 1);
      
    }

    private void FindAndAssignScripts(Transform parent, int level)
    {
        foreach (Transform child in parent)
        {
            string stateClassName = child.name + "State";
            string stateMachineClassName = child.name + "StateMachine";

            AssignScriptToGameObject(child, stateClassName);
            if (child.childCount > 0)
            {
                AssignScriptToGameObject(child, stateMachineClassName);
            }
            FindAndAssignScripts(child, level + 1);
        }
    }

    private void AssignScriptToGameObject(Transform obj, string scriptName)
    {
        string[] guids = AssetDatabase.FindAssets(scriptName + " t:MonoScript");
        if (guids.Length == 0)
        {
            Debug.LogWarning($"Script {scriptName} not found in project.");
            return;
        }

        string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);

        if (monoScript == null)
        {
            Debug.LogWarning($"Failed to load script: {scriptName}");
            return;
        }

        System.Type scriptType = monoScript.GetClass();
        if (scriptType == null)
        {
            Debug.LogWarning($"Class {scriptName} not found. Ensure the script is compiled.");
            return;
        }

        if (obj.GetComponent(scriptType) == null)
        {
            Undo.AddComponent(obj.gameObject, scriptType);
            Debug.Log($"Assigned {scriptName} to {obj.name}");
        }
    }
}
#endif
