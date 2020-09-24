using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using UnityEditor;
using UnityEngine;


public class EditorExtensions
{
    // ** 根据proto定义生成cs 类
    [MenuItem("Protobuf/GenerateProto")]
    public static void GenerateProto()
    {
        string protoc;
#if UNITY_EDITOR_WIN
        protoc = $"{Environment.CurrentDirectory}/Protobuf/protoc.exe";
#else
        Debug.LogWarning("平台没有集成对应的Protobuf程序");
#endif
        if(string.IsNullOrEmpty(protoc))
        {
            return;
        }
        var dataPath = Application.dataPath;
        var outputDir = $"{dataPath}/Scripts/ProtocGenerated/";
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
        var inputDir = $"{dataPath}/Proto/";
        if(!Directory.Exists(inputDir))
        {
            Directory.CreateDirectory(inputDir);
        }
        var files = Directory.GetFiles(inputDir, "*.proto");
        if (files.Length == 0)
        {
            return;
        }
        var args = new string[]
        {
            $"--proto_path={inputDir}",
            $"--csharp_out={outputDir}",
        };
        Array.Resize(ref args, files.Length + 2);
        Array.Copy(files, 0, args, 2, files.Length);
        ShellUtility.ShellWithError(protoc, args);
    }
}
