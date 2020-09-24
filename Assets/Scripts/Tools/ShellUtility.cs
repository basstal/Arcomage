using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public static class ShellUtility
{
    public class ShellResult
    {
        public int exitCode { get;}
        public string stdOut { get; }
        public string stdErr { get; }

        public ShellResult(int exitCode, string stdOut, string stdErr)
        {
            this.exitCode = exitCode;
            this.stdOut = stdOut;
            this.stdErr = stdErr;
        }
    }
    public static ShellResult Shell(string executable, string[] args, string workingDir = null, bool waitForExit = true, Dictionary<string, string> environments = null)
    {
        try
        {
            Process process = new Process();
            process.StartInfo.FileName = executable;
            process.StartInfo.Arguments = string.Join(" ", args);
            process.StartInfo.WorkingDirectory = workingDir ?? Directory.GetCurrentDirectory();

            if (environments != null)
            {
                foreach (var entry in environments)
                {
                    process.StartInfo.EnvironmentVariables[entry.Key] = entry.Value;
                }
            }

            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            StringBuilder stdoutsb = new StringBuilder();
            process.OutputDataReceived += (obj, sender) => { stdoutsb.Append("\n" + sender.Data); };

            process.StartInfo.RedirectStandardError = true;
            StringBuilder stderrsb = new StringBuilder();
            process.ErrorDataReceived += (obj, sender) => { stderrsb.Append("\n" + sender.Data); };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (waitForExit)
            {
                process.WaitForExit();
                return new ShellResult(process.ExitCode, stdoutsb.ToString(), stderrsb.ToString());
            }
            else
            {
                return new ShellResult(0, "", "");
            }
        }
        catch (Exception e)
        {
            return new ShellResult(-1, "", e.Message);
        }
    }

    public static ShellResult ShellWithError(string cmd, string[] args, string workingDir = null, bool waitForExit = true, Dictionary<string, string> environments = null)
    {
        var result = Shell(cmd, args, workingDir, waitForExit, environments);
        if (result.exitCode != 0)
        {
            UnityEngine.Debug.LogError($"Shell failed: {result.stdErr}");
        }
        else
        {
            UnityEngine.Debug.Log($"Shell success: {result.stdOut}");
        }

        return result;
    }
}
