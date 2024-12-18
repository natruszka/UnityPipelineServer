using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UnityPipelineWebApi.DTOs;
using UnityPipelineWebApi.Entities;
using UnityPipelineWebApi.Services;

namespace UnityPipelineWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BuildController(FileService fileService, BuildService buildService) : ControllerBase
{
    [HttpPost("builds/start")]
    public async Task<IActionResult> StartBuild()
    {
        Build build = new Build();
        return Ok(build.Guid);
    }
    [HttpPost]
    public async Task<IActionResult> UploadGameObjectsAndBuild(Guid buildName, [FromBody]List<GameObjectInfoDto> gameObjectInfos/*[FromBody] string gameObjectInfoJson */)
    {
        try
        {
            var gameObjects = await buildService.ChangeFileGuidsToPaths(gameObjectInfos);
            await fileService.SaveGameObjectsToJson(gameObjects, buildName);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpPost("builds/empty")]
    public async Task<IActionResult> BuildEmptyProject()
    {
        try
        {
            await buildService.BuildEmptyProject();
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpGet("builds")]
    public async Task<IActionResult> DownloadBuild(string buildName)
    {
        return Ok(buildName);
    }

    [HttpPost("build/assetbundles")]
    public async Task<IActionResult> BuildAssetBundles(Guid buildName)
    {
        await buildService.BuildAssetBundles(buildName);
        await buildService.BuildProject(buildName);
        return Ok();
    }
}