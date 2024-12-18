using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace UnityPipelineWebApi.Entities;

public class GameObjectInfo
{
    public string Name { get; set; } = string.Empty;
    public float[] Position = [0, 0, 0];
    public float[] Rotation = [0, 0, 0];
    public float[] Scale = [1, 1, 1];
    public List<Component> Components { get; set; } = new();
}