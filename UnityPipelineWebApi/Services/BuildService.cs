using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using UnityPipelineWebApi.DTOs;
using UnityPipelineWebApi.Entities;

namespace UnityPipelineWebApi.Services;

public class BuildService(IConfiguration configuration, IMemoryCache memoryCache)
{
    private readonly string _projectPath = configuration["ProjectPath"] ?? throw new Exception("ProjectPath not found in appsettings.json");
    private readonly string _relativeUploadsPath = "Assets/Uploads";
    private string UnityExePath => GetUnityExePath();
    private string GetUnityExePath()
    {
        return configuration["UnityExePath"] ?? throw new Exception("UnityExePath not found in appsettings.json");
    }

    private bool CheckIfDirectoryEmptyOrCreate(string path)
    {
        try
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
        catch (DirectoryNotFoundException ex)
        {
            Directory.CreateDirectory(path);
            return true;
        }
    }

    public async Task BuildEmptyProject()
    {
        await CreateProject();
    }

    private async Task CreateProject()
    {
        try
        {
            if (!CheckIfDirectoryEmptyOrCreate(_projectPath))
            {
                throw new Exception("Project directory is not empty");
            }

            var process = new Process
            {
                StartInfo =
                {
                    FileName = UnityExePath,
                    Arguments = $"-batchmode -createProject {_projectPath} -quit",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task BuildAssetBundles(Guid buildName)
    {
        try
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = UnityExePath,
                    Arguments = $"-batchmode -quit -projectPath {_projectPath} -executeMethod BatchBuild.BuildAssetBundles -buildName {buildName} -logfile log.txt",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                throw new Exception("BuildAssetBundles failed");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    public async Task<List<GameObjectInfo>> ChangeFileGuidsToPaths(List<GameObjectInfoDto> gameObjectInfoDtos)
    {
        var gameObjectInfos = new List<GameObjectInfo>();
        foreach (var gameObjectInfoDto in gameObjectInfoDtos)
        {
            var gameObjectInfo = new GameObjectInfo
            {
                Name = gameObjectInfoDto.Name,
                Position = gameObjectInfoDto.Position,
                Rotation = gameObjectInfoDto.Rotation,
                Scale = gameObjectInfoDto.Scale,
                Components = new List<Component>()
            };
            foreach (var component in gameObjectInfoDto.Components)
            {
                gameObjectInfo.Components.Add(new Component()
                {
                    Type = component.Type,
                    Path = memoryCache.TryGetValue(component.Guid, out var path) ? path?.ToString() : string.Empty,
                    RelativePath = CreateRelativePath(path?.ToString() ?? string.Empty)
                });
            }
            gameObjectInfos.Add(gameObjectInfo);
        }
        return gameObjectInfos;
    }
    
    private string CreateRelativePath(string path)
    {
        return _relativeUploadsPath + "/" + Path.GetFileName(path);
    }
    public async Task BuildProject(Guid buildName)
    {
        try
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = UnityExePath,
                    Arguments = $"-batchmode -quit -projectPath {_projectPath} -executeMethod BatchBuild.BuildApk -buildName {buildName} -logfile log.txt",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                throw new Exception("Build failed");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}