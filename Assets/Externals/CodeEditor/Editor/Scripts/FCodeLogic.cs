using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CodeEditor;
using UnityEditor;
using UnityEngine;

public class FCodeLuaFunction
{
    public string name { get; set; }
    public string shortName { get; set; }
    public List<Token> functionParameters { get; }
    public List<Token> returnParameters { get; }
    public List<Token> tokens { get; }
    public Dictionary<int, List<Token>> line2Tokens { get; }
    public int fileStartLine { get; }
    public int maxLine { get; set; }
    public FCodeLuaFunction(int fileStartLine)
    {
        tokens = new List<Token>();
        functionParameters = new List<Token>();
        returnParameters = new List<Token>();
        line2Tokens = new Dictionary<int, List<Token>>();
        this.fileStartLine = fileStartLine;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"name : {name} , startLine : {fileStartLine} , parameters : ");
        int i = 1;
        foreach (var parameter in functionParameters)
        {
            sb.Append($" p{i++}({parameter.Original()}) ");
        }
        sb.AppendLine();
        i = 1;
        foreach (var entry in line2Tokens)
        {
            sb.Append($" {i++} : ");
            foreach (var token in entry.Value)
            {
                sb.Append($" {token.Original()} ");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}

public class FCodeLib
{
    public Dictionary<string, string> parametersTypeRef { get; set; }
    public Dictionary<string, FCodeLuaFunction> fcodeFunctions { get; set; }


}
public class FCodeLogic
{
    public const string templateInstance = "default";
    public class SingleLogic
    {
        private FCodeTiming m_timing;
        private FCodeLuaFunction m_command;
        private FCodeLuaFunction m_condition;
        public FCodeTiming timing
        {
            get => m_timing;
            set => m_timing = value;
        }

        public FCodeLuaFunction command => m_command;
        public FCodeLuaFunction condition => m_condition;
        public SingleLogic(FCodeTiming timing, FCodeLuaFunction command, FCodeLuaFunction condition)
        {
            m_timing = timing;
            m_command = command;
            m_condition = condition;
        }

    }
    private List<SingleLogic> m_logicList = new List<SingleLogic>();

    private List<FCodeLuaFunction> m_vars = new List<FCodeLuaFunction>();
    public List<SingleLogic> logicList { get => m_logicList; }
    public List<FCodeLuaFunction> vars
    {
        get => m_vars;
    }
    public string name { get; set; }
    public List<InstanceConfig> instances = new List<InstanceConfig>();
    public void SyncConfigValChange(InstanceConfig.ConfigVal configVal)
    {
        foreach (var instanceConfig in instances)
        {
            if (instanceConfig.instanceName != FCodeLogic.templateInstance)
            {
                var entry = instanceConfig.values.Find(v => v.key.Equals(configVal.key));
                entry.type = configVal.type;
            }
        }
    }
    public void SyncConfigValRemove(InstanceConfig.ConfigVal configVal)
    {
        foreach (var instanceConfig in instances)
        {
            if (instanceConfig.instanceName != FCodeLogic.templateInstance)
            {
                var entry = instanceConfig.values.Find(v => v.key.Equals(configVal.key));
                instanceConfig.values.Remove(entry);
            }
        }
    }
    public void SyncConfigValAdd(InstanceConfig.ConfigVal configVal)
    {
        foreach (var instanceConfig in instances)
        {
            if (instanceConfig.instanceName != FCodeLogic.templateInstance)
            {
                instanceConfig.values.Add((InstanceConfig.ConfigVal)configVal.Clone());
            }
        }
    }
    public void NewInstance(string instanceName)
    {
        var template = instances.Find(v => v.instanceName == FCodeLogic.templateInstance);
        var toAdd = new FCodeLogic.InstanceConfig()
        {
            instanceName = instanceName,
        };
        if (template != null)
        {
            template.values.ForEach(v => toAdd.values.Add((InstanceConfig.ConfigVal)v.Clone()));
        }
        instances.Add(toAdd);
    }
    public class InstanceConfig
    {
        public string instanceName;
        public List<ConfigVal> values = new List<ConfigVal>();
        public class ConfigVal : ICloneable
        {
            public string key;
            public InstanceConfigType type = InstanceConfigType.None;
            public object value;

            public object Clone()
            {
                return new ConfigVal()
                {
                    key = this.key,
                    type = this.type,
                    value = this.value,
                };
            }
        }
    }
    public enum InstanceConfigType
    {
        None,
        String,
        Number,
    };
}