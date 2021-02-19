using System;
using System.Reflection;
using Google.Protobuf;
using UnityEngine;
public static class TransformTools
{
    public static byte[] OldData2NewData(CodeNode oldData)
    {
        var newTree = new BevTree();
        newTree.Name = "BehaviourTree";
        newTree.Comments = oldData.Name;
        newTree.ZoomFactor = 1;
        newTree.Translation = new Vector2Proto() { X = -5000, Y = -5000 };
        HandleRoot(ref newTree, ref oldData);
        return SerializeTools.ToData(newTree);
    }

    private static void HandleChildren(ref BevTree newTree, ref CodeNode oldData, ref int startId, BevNode parent, int depth)
    {
        if (oldData.Children != null)
        {
            for (var i = 0; i < oldData.Children.Count; ++i)
            {
                var child = oldData.Children[i];
                var bevNode = CreateNodeFromCodeNode(child, startId++, depth, parent.Id, parent.Position.X);
                var connection = CreateConnection(parent, bevNode);
                newTree.Connections.Add(connection);
                newTree.Nodes.Add(bevNode);
                parent.OutConnectionsUid.Add(connection.Uid);
                bevNode.InConnectionsUid.Add(connection.Uid);
                HandleChildren(ref newTree, ref child, ref startId, bevNode, depth + 1);
            }
        }
    }
    private static BevConnection CreateConnection(BevNode source, BevNode target)
    {
        var bevConnection = new BevConnection();
        bevConnection.Uid = System.Guid.NewGuid().ToString();
        bevConnection.SourceNodeUid = source.Uid;
        bevConnection.TargetNodeUid = target.Uid;
        bevConnection.IsActive = true;
        return bevConnection;
    }
    private static void HandleRoot(ref BevTree newTree, ref CodeNode oldData)
    {
        if (oldData.Children.Count == 1)
        {
            var startId = 1;
            var root = oldData.Children[0];
            var bevNode = CreateNodeFromCodeNode(root, startId++, 1, 0, 5000);
            newTree.PrimeNodeUid = bevNode.Uid;
            newTree.Nodes.Add(bevNode);
            HandleChildren(ref newTree, ref root, ref startId, bevNode, 2);
        }
    }

    private static void TaskParser(ref CodeNode n, ref BevNode bevNode)
    {
        var taskProperty = typeof(CodeNode).GetProperty(n.Type);
        var data = (IMessage)taskProperty.GetValue(n);
        if (data != null)
        {
            bevNode.Task = ByteString.CopyFrom(data.ToData());
        }
    }
    private static BevNode CreateNodeFromCodeNode(CodeNode n, int id, int depth, int parentId, float startX)
    {
        var bevNode = new BevNode();
        var uid = System.Guid.NewGuid().ToString();
        bevNode.Uid = uid;
        bevNode.Id = id;
        var position = new Vector2(startX + 110 * (id - parentId), 5000 + 100 * depth);
        bevNode.Position = position.ToVector2Proto();
        bevNode.TaskType = n.Type;
        bevNode.Comment = n.Name;
        switch (n.Type)
        {
            case "Selector":
                bevNode.Name = "SELECTOR";
                bevNode.Type = "Selector";
                break;
            case "Sequence":
                bevNode.Name = "SEQUENCER";
                bevNode.Type = "Sequencer";
                break;
            case "RandomSelector":
                bevNode.Name = "RANDOMSELECTOR";
                bevNode.Type = "RandomSelector";
                break;
            default:
                switch (n.NodeType)
                {
                    case BevNodeType.Action:
                        bevNode.Name = "ACTION";
                        bevNode.Type = "ActionNode";
                        TaskParser(ref n, ref bevNode);
                        break;
                    case BevNodeType.Condition:
                        bevNode.Name = "CONDITION";
                        bevNode.Type = "ConditionNode";
                        TaskParser(ref n, ref bevNode);
                        break;
                    default:
                        UnityEngine.Debug.LogWarning($"CreateNodeFromCodeNode Type not found: {n.Type}");
                        break;
                }
                break;
        }
        return bevNode;
    }
}