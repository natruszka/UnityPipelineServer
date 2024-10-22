namespace UnityPipelineWebApi.DTOs;

public class GameObjectInfoDto
{
    public float[] Position { get; set; }= [0, 0, 0];
    public float[] Rotation { get; set; }= [0, 0, 0];
    public float[] Scale { get; set; } = [1, 1, 1];
    public List<ComponentDto> Components { get; } = new();
}
