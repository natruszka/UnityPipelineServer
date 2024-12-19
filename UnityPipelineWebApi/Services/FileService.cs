using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using UnityPipelineWebApi.Entities;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace UnityPipelineWebApi.Services;

public class FileService(IConfiguration configuration, IMemoryCache memoryCache)
{
    private readonly string _projectPath = configuration["ProjectPath"] ?? throw new Exception("ProjectPath not found in appsettings.json");
    private string UploadsPath => GetUploadsPath();
    private string GetUploadsPath()
    {
        return Path.Combine(_projectPath, "Assets\\Uploads");
    }
    private string GetUploadsPath(Guid buildName)
    {
        return Path.Combine(_projectPath, "Assets\\Uploads", buildName.ToString());
    }
    public async Task<Guid> SaveFile(IFormFile file, Guid buildName)
    {
        try
        {
            var fileName = $"{buildName}_{RemoveWhitespace(file.FileName)}";
            var uploadsPath = GetUploadsPath(buildName);
            var filePath = Path.GetFullPath(uploadsPath + "\\" + fileName);

            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
                
            }
            if(!Directory.Exists(Path.Combine(uploadsPath, buildName.ToString())))
            {
                Directory.CreateDirectory(Path.Combine(uploadsPath, buildName.ToString()));
            }
            
            await using var
                stream = new FileStream(filePath,
                    FileMode.Create); //Create or CreateNew? CreateNew will throw an exception if the file already exists
            await file.CopyToAsync(stream);
            var fileGuid = Guid.NewGuid();
            memoryCache.Set(fileGuid, filePath);
            return fileGuid;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    public async Task SaveGameObjectsToJson(List<GameObjectInfo> gameObjectInfos, Guid buildName)
    {
        var filePath = Path.Combine(_projectPath,"Assets\\StreamingAssets\\ConfigurationData", buildName.ToString(), "Data.json");
        var json = JsonConvert.SerializeObject(gameObjectInfos);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task CleanAfterBuild(string buildName)
    {
        
    }
    private async Task SaveJsonData(string json, string buildName)
    {
        var filePath = Path.Combine(UploadsPath, "data.json");
        await using FileStream createStream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(createStream, json);
    }
    private string RemoveWhitespace(string fileName)
    {
        return fileName.Replace(" ", string.Empty);
    }
}