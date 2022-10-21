// using System.Collections.Generic;
// using UnityEngine;
// using System.Linq;
// using System.IO;
//
// public class ShellResult
// {
//     public int ExitCode { get; private set; }
//     public string StdOut { get; private set; }
//     public string StdErr { get; private set; }
//
//     public ShellResult(int exitCode, string stdOut, string stdErr)
//     {
//         ExitCode = exitCode;
//         StdOut = stdOut;
//         StdErr = stdErr;
//     }
// }
//
// public static class ShellProxy
// {
//     public static ShellResult Shell(string executable, object[] args, string workingDir = null, bool waitForExit = true, Dictionary<string, string> envrionments = null)
//     {
//         try
//         {
//             System.Diagnostics.Process process = new System.Diagnostics.Process();
// #if UNITY_EDITOR_WIN
//             if (executable == "python") executable = "pythonw";
// #endif
//             process.StartInfo.FileName = executable;
//             process.StartInfo.Arguments = string.Join(" ", args.Select(p => p.ToString()).ToArray());
//             if (workingDir != null)
//             {
//                 process.StartInfo.WorkingDirectory = workingDir;
//             }
//             else
//             {
//                 process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
//             }
//
//             if (envrionments != null)
//             {
//                 foreach (var entry in envrionments)
//                 {
//                     process.StartInfo.EnvironmentVariables[entry.Key] = entry.Value;
//                 }
//             }
//
//             process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
//             process.StartInfo.CreateNoWindow = true;
//             process.StartInfo.UseShellExecute = false;
//             process.StartInfo.RedirectStandardOutput = true;
//             System.Text.StringBuilder stdoutsb = new System.Text.StringBuilder();
//             process.OutputDataReceived += (obj, sender) => { stdoutsb.Append("\n" + sender.Data); };
//
//             process.StartInfo.RedirectStandardError = true;
//             System.Text.StringBuilder stderrsb = new System.Text.StringBuilder();
//             process.ErrorDataReceived += (obj, sender) => { stderrsb.Append("\n" + sender.Data); };
//
//             process.Start();
//
//             process.BeginOutputReadLine();
//             process.BeginErrorReadLine();
//
//             if (waitForExit)
//             {
//                 process.WaitForExit();
//                 return new ShellResult(process.ExitCode, stdoutsb.ToString(), stderrsb.ToString());
//             }
//             else
//             {
//                 return new ShellResult(0, "", "");
//             }
//         }
//         catch (System.Exception e)
//         {
//             return new ShellResult(-1, "", e.Message);
//         }
//     }
//
//     public static ShellResult ShellWithError(string cmd, object[] args, string workingDir = null, bool waitForExit = true, Dictionary<string, string> envrionments = null)
//     {
//         var result = Shell(cmd, args, workingDir, waitForExit, envrionments);
//         if (result.ExitCode != 0)
//         {
//             Debug.LogError(string.Format("Shell failed: {0}", result.StdErr));
//         }
//         else
//         {
//             Debug.Log(string.Format("Shell success: {0}", result.StdOut));
//         }
//
//         return result;
//     }
// }