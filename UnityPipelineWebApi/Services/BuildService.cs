using System.Diagnostics;

namespace UnityPipelineWebApi.Services;

public class BuildService(IConfiguration configuration)
{
    private readonly string _projectPath = Path.Combine(Directory.GetCurrentDirectory(), "..\\..", "UnityProject");

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
        await ConfigureVRProject();
    }

    private async Task CreateProject()
    {
        try
        {
            var unityExePath = GetUnityExePath();

            if (!CheckIfDirectoryEmptyOrCreate(_projectPath))
            {
                throw new Exception("Project directory is not empty");
            }

            var process = new Process
            {
                StartInfo =
                {
                    FileName = unityExePath,
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

    public async Task BuildProject(string buildName)
    {
        
    }
    private async Task ConfigureVRProject()
    {
        string manifestPath = Path.Combine(_projectPath, "Packages", "manifest.json");

        string manifestContent = await File.ReadAllTextAsync(manifestPath);
        
        if (!manifestContent.Contains("com.unity.xr.management"))
        {
            manifestContent = manifestContent.Replace(
                "\"dependencies\": {", 
                "\"dependencies\": {\n    \"com.unity.xr.management\": \"4.2.0\",\n    \"com.unity.xr.oculus\": \"3.0.1\",\n    \"com.unity.xr.openxr\": \"1.3.1\",");
        }

        await File.WriteAllTextAsync(manifestPath, manifestContent);
    }
}