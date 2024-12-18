using System.Text.Json.Serialization;

namespace UnityPipelineWebApi.Entities;

public class Component
{
    public string Type = string.Empty;
    public string Path = string.Empty;
    public string RelativePath = string.Empty;
}