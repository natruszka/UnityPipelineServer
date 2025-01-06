using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UnityPipelineWebApi.DTOs;
using UnityPipelineWebApi.Entities;
using UnityPipelineWebApi.Services;

namespace UnityPipelineWebApi.Controllers;

[Route("api")]
[ApiController]
public class BuildController(FileService fileService, BuildService buildService) : ControllerBase
{
    [HttpPost("/[controller]/start")]
    public async Task<IActionResult> StartBuild()
    {
        Build build = new Build();
        return Ok(build.Guid);
    }
    [HttpPost("/[controller]/{buildName}")]
    public async Task<IActionResult> UploadGameObjectsAndBuild(Guid buildName, [FromBody]List<GameObjectInfoDto> gameObjectInfos)
    {
        try
        {
            var gameObjects = await buildService.ChangeFileGuidsToPaths(gameObjectInfos, buildName.ToString());
            await fileService.SaveGameObjectsToJson(gameObjects, buildName);
            await buildService.BuildAssetBundles(buildName);
            await buildService.BuildProject(buildName);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpPost("/[controller]/empty")]
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
    [HttpGet("/[controller]/{buildName}")]
    public async Task<IActionResult> DownloadBuild(Guid buildName)
    {
        try
        {
            var build = await buildService.DownloadBuild(buildName.ToString());
            return File(build, "application/octet-stream", buildName + ".apk");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    //
    // [HttpPost("assetbundleeditor/{buildName}")]
    // public async Task<IActionResult> BuildApk(Guid buildName)
    // {
    //     try
    //     {
    //         await buildService.BuildAssetBundles(buildName);
    //         return Ok();
    //     }
    //     catch (Exception ex)
    //     {
    //         return BadRequest(ex.Message);
    //     }
    // }
}