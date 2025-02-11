﻿using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using UnityPipelineWebApi.DTOs;
using UnityPipelineWebApi.Entities;

namespace UnityPipelineWebApi.Services;

public class BuildService(IConfiguration configuration, IMemoryCache memoryCache)
{
    private readonly string _projectPath = configuration["ProjectPath"] ?? throw new Exception("ProjectPath not found in appsettings.json");
    private readonly string _templatePath = configuration["TemplatePath"] ?? throw new Exception("TemplatePath not found in appsettings.json");
    private readonly string _relativeUploadsPath = "assets/uploads";
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
                    Arguments = $"-batchmode -cloneFromTemplate \"{_templatePath}\" -createProject \"{_projectPath}\" -quit -logfile projectlog.txt",
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
                throw new Exception("BuildAssetBundles failed. Check logs for more information.");
            }
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
                    Arguments = $"-batchmode -quit -projectPath \"{_projectPath}\" -executeMethod BatchBuild.BuildAssetBundles -buildName {buildName} -logfile log.txt",
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

    public async Task<MemoryStream> DownloadBuild(string buildName)
    {   
        if (string.IsNullOrEmpty(buildName))
        {
            throw new Exception("Build name is empty");
        }
        var buildPath = Path.Combine(_projectPath, "Builds", "Android", buildName +".apk");
        if (!File.Exists(buildPath))
        {
            throw new Exception("Build not found");
        }
        var memory = new MemoryStream();
        await using var stream = new FileStream(buildPath, FileMode.Open);
        await stream.CopyToAsync(memory);
        memory.Position = 0;
        return memory;
    }
    public async Task<List<GameObjectInfo>> ChangeFileGuidsToPaths(List<GameObjectInfoDto> gameObjectInfoDtos, string buildName)
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
                    RelativePath = CreateRelativePath(buildName, path?.ToString() ?? string.Empty)
                });
            }
            gameObjectInfos.Add(gameObjectInfo);
        }
        return gameObjectInfos;
    }
    
    private string CreateRelativePath(string buildName, string path)
    {
        return _relativeUploadsPath + "/" + buildName + "/" + Path.GetFileName(path);
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
                    Arguments = $"-batchmode -quit -projectPath \"{_projectPath}\" -executeMethod BatchBuild.BuildApk -buildName {buildName} -logfile log.txt",
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