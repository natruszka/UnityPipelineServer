using Microsoft.AspNetCore.Mvc;
using UnityPipelineWebApi.Entities;
using UnityPipelineWebApi.Services;

namespace UnityPipelineWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BuildController(FileService fileService, BuildService buildService) : ControllerBase
{
    [HttpPost("files")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        try
        {
            await fileService.SaveFile(file);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpPost]
    public async Task<IActionResult> UploadGameObjectsAndBuild([FromForm] GameObjectInfo gameObjectInfo,[FromForm] List<IFormFile> files,  bool useGoogleDrive)
    {
        var gameObjectInfos = new List<GameObjectInfo>();
        gameObjectInfos.Add(gameObjectInfo);
        var name = await fileService.PrepareForBuild(gameObjectInfos, files, useGoogleDrive);
        await buildService.BuildProject(name);
        await fileService.CleanAfterBuild(name);
        return Ok(name);
    }
    [HttpPost("builds")]
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
}