using System.Diagnostics;
using System.Text.Json;
using UnityPipelineWebApi.Entities;

namespace UnityPipelineWebApi.Services;

public class FileService(IConfiguration configuration)
{
    private string GetUploadsPath()
    {
        return configuration["UploadsPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..\\..", "Uploads");
    }

    private string GetUnityExePath()
    {
        return configuration["UnityExePath"] ?? throw new Exception("UnityExePath not found in appsettings.json");
    }

    public async Task<Guid> SaveFile(IFormFile file, string buildName = "")
    {
        try
        {
            var fileName = $"{buildName}_{RemoveWhitespace(file.FileName)}";
            var uploadsPath = GetUploadsPath();
            var filePath = buildName == String.Empty ? Path.Combine(uploadsPath, fileName) : Path.Combine(uploadsPath, buildName, fileName);
            if (File.Exists(filePath))
            {
                return;
            }
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
                
            }
            if(!Directory.Exists(Path.Combine(uploadsPath, buildName)) && buildName != String.Empty)
            {
                Directory.CreateDirectory(Path.Combine(uploadsPath, buildName));
            }

            await using var
                stream = new FileStream(filePath,
                    FileMode.Create); //Create or CreateNew? CreateNew will throw an exception if the file already exists
            await file.CopyToAsync(stream);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    public async Task<string> PrepareForBuild(List<GameObjectInfo> gameObjectInfos, List<IFormFile> files, bool useGoogleDrive)
    {
        var build = new Build();
        if (useGoogleDrive)
        {
            return "Not implemented";
        }
        var gameObjectInfosWithFiles = await ConcatenateFiles(gameObjectInfos, files);
        List<GameObjectJsonInfo> gameObjectJsonInfos = new List<GameObjectJsonInfo>();
        foreach (var gameObjectInfo in gameObjectInfosWithFiles)
        {
            foreach (var file in gameObjectInfo.Files)
            {
                await SaveFile(file, build.Name);
            }
            gameObjectJsonInfos.Add(new GameObjectJsonInfo
            {
                Position = gameObjectInfo.Position,
                Rotation = gameObjectInfo.Rotation,
                Scale = gameObjectInfo.Scale,
                Components = gameObjectInfo.Files.Select(component => new GameObjectJsonComponent  
                {
                    FilePath =  $"{build.Name}_{RemoveWhitespace(component.FileName)}"
                }).ToList()
            });
        }
        // var json = JsonSerializer.Serialize(gameObjectJsonInfos, GameObjectJsonContext.Default.ListGameObjectJsonComponent);
        // await SaveJsonData(json, build.Name);
        return build.Name;
    }
    
    public async Task CleanAfterBuild(string buildName)
    {
        
    }
    private async Task<List<GameObjectInfoWithFile>> ConcatenateFiles(List<GameObjectInfo> gameObjectInfos, List<IFormFile> files)
    {
        var gameObjectInfosWithFiles = new List<GameObjectInfoWithFile>();
        var i = 0;
        foreach (var gameObjectInfo in gameObjectInfos)
        {
            var gameObjectInfoWithFile = new GameObjectInfoWithFile
            {
                Position = gameObjectInfo.Position,
                Rotation = gameObjectInfo.Rotation,
                Scale = gameObjectInfo.Scale,
                Types = gameObjectInfo.ConvertToList(),
            };
            foreach (var type in gameObjectInfoWithFile.Types)
            {
                gameObjectInfoWithFile.Files.Add(files[i]);
                i++;
            }
            gameObjectInfosWithFiles.Add(gameObjectInfoWithFile);
        }
        return gameObjectInfosWithFiles;
    }
    private async Task SaveJsonData(string json, string buildName)
    {
        var uploadsPath = GetUploadsPath();
        var filePath = Path.Combine(uploadsPath, buildName, "data.json");
        await using FileStream createStream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(createStream, json);
    }
    private string RemoveWhitespace(string fileName)
    {
        return fileName.Replace(" ", string.Empty);
    }
}