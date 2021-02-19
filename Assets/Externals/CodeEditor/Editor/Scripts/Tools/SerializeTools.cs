using Google.Protobuf;
using Google.Protobuf.Reflection;
using System;
using System.IO;
using UnityEngine;

public static class SerializeTools
{
    static JsonFormatter jsonFormatter = new JsonFormatter(new JsonFormatter.Settings(true));
    static JsonParser jsonParser = new JsonParser(new JsonParser.Settings(99));
    public static string ToJson(this IMessage msg)
    {
        return jsonFormatter.Format(msg);
    }
    public static byte[] ToData(this IMessage msg)
    {
        return msg.ToByteArray();
    }
    public static IMessage FromData(Type t, byte[] data)
    {
        var result = (IMessage)Activator.CreateInstance(t);
        if (data != null)
        {
            try
            {
                result.MergeFrom(data);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        return result;
    }
    public static T FromData<T>(byte[] data) where T : IMessage, new()
    {
        T result = default(T);
        if (data != null)
        {
            result = new T();
            try
            {
                result.MergeFrom(data);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        return result;
    }
    public static T FromJson<T>(string json) where T : IMessage, new()
    {
        try
        {
            return jsonParser.Parse<T>(json);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return default(T);
        }
    }
    public static IMessage FromJson(string json, MessageDescriptor descriptor)
    {
        try
        {
            return jsonParser.Parse(json, descriptor);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }
}