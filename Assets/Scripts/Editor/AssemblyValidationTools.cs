using GameScripts;

namespace GameEditorScripts
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    public static class AssemblyValidationTools
    {
        /// <summary>
        /// We are using types that we know to be in our runtime assemblies to get references
        /// to our runtime assemblies. If these types are ever moved to a different assembly, we will get
        /// misleading results
        /// </summary>
        [MenuItem("Tools/Assembly/Check runtime assemblies for editor dependencies")]
        private static void CheckSelectedAssembly()
        {
            CheckAssembly(typeof(Player).Assembly);

            //common.Runtime
            // CheckAssembly(typeof(ATypeFromRuntimeAssemblyB).Assembly);
        }

        private static void CheckAssembly(Assembly assembly)
        {
            if (IsEditorAssembly(assembly))
            {
                Debug.Log($"{assembly.GetName().Name} is an editor assembly");
                return;
            }

            HashSet<string> allVisited = new HashSet<string>();

            Stack<string> currentDependencyPath = new Stack<string>();

            List<Stack<string>> editorDependencyChains = new List<Stack<string>>();

            DependsOnEditorAssemblies(assembly, currentDependencyPath, editorDependencyChains, allVisited);

            if (0 < editorDependencyChains.Count)
            {
                foreach (Stack<string> dependencyChain in editorDependencyChains)
                {
                    PrintEditorDependent(dependencyChain);
                }
            }
            else
            {
                PrintNoEditorDependencies(assembly.GetName().Name);
            }
        }

        private static void PrintNoEditorDependencies(string name)
        {
            Debug.Log($"{ColouredLogString(name, Color.green)} has no editor dependencies");
        }

        private static void PrintEditorDependent(Stack<string> dependencyChain)
        {
            string[] stackContents = dependencyChain.ToArray();
            Array.Reverse(stackContents);

            int numElements = stackContents.Length;

            int iElement = 0;

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Editor dependency found: ");

            if (iElement < numElements)
            {
                stringBuilder.Append(ColouredLogString(stackContents[iElement], Color.red));
                iElement += 1;
            }

            for (; iElement < numElements - 1; ++iElement)
            {
                stringBuilder.Append($" > {stackContents[iElement]}");
            }

            if (iElement < numElements)
            {
                stringBuilder.Append(" > ");
                stringBuilder.Append(ColouredLogString(stackContents[iElement], Color.red));
            }

            Debug.LogWarning(stringBuilder.ToString());
        }

        private static void DependsOnEditorAssemblies(
            Assembly assembly,
            Stack<string> currentDependencyPath,
            List<Stack<string>> editorDependencyChains,
            HashSet<string> visitedNonEditorAssemblies)
        {
            if (visitedNonEditorAssemblies.Contains(assembly.GetName().Name))
            {
                return;
            }

            if (IsEditorAssembly(assembly))
            {
                editorDependencyChains.Add(DuplicateDependencyPathAndAddAssembly(currentDependencyPath, assembly.GetName().Name));
                return;
            }

            // we don't mark editor assemblies as visited because we want to find all the user assemblies that depend on them
            visitedNonEditorAssemblies.Add(assembly.GetName().Name);

            if (IsIgnoredUserAssembly(assembly.GetName()))
            {
                return;
            }

            currentDependencyPath.Push(assembly.GetName().Name);

            AssemblyName[] listOfAssemblyNames = assembly.GetReferencedAssemblies();
            foreach (var dependencyName in listOfAssemblyNames)
            {
                try
                {
                    Assembly dependency = Assembly.Load(dependencyName);

                    DependsOnEditorAssemblies(dependency, currentDependencyPath, editorDependencyChains, visitedNonEditorAssemblies);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                }
            }

            currentDependencyPath.Pop();
        }

        private static Stack<string> DuplicateDependencyPathAndAddAssembly(Stack<string> dependencyPath, string lastAssemblyName)
        {
            string[] stackContents = dependencyPath.ToArray();
            Array.Reverse(stackContents);
            Stack<string> newDependencyChain = new Stack<string>(stackContents);
            newDependencyChain.Push(lastAssemblyName);

            return newDependencyChain;
        }

        private static bool IsEditorAssembly(Assembly assembly)
        {
            if (Attribute.IsDefined(assembly, typeof(AssemblyIsEditorAssembly)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsIgnoredUserAssembly(AssemblyName assemblyName)
        {
            var name = assemblyName.Name;

            return
                name == "Assembly-CSharp" ||
                name == "Assembly-CSharp-firstpass";
        }

        private static string ColouredLogString(object message, Color color)
        {
            var colouredLogString =
                $"<color=#{(byte)(color.r * 255f):X2}{(byte)(color.g * 255f):X2}{(byte)(color.b * 255f):X2}>{message}</color>";

            return colouredLogString;
        }
    }
}