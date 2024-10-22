namespace UnityPipelineWebApi.Entities;

public class Build
{
    public Guid Guid { get; set; } = new Guid();
    public string UnityVersion { get; set; }= string.Empty;
    public IFormFile? BuildFile { get; set; }= null;
}