namespace UnityPipelineWebApi.Entities;

public class Build
{
    public Guid Guid { get; set; } = Guid.NewGuid();
    public string UnityVersion { get; set; }= string.Empty;
    public IFormFile? BuildFile { get; set; }= null;
    public List<GameObjectInfo> GameObjects { get; } = new();
}