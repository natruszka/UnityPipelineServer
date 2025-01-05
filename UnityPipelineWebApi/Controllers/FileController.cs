using Microsoft.AspNetCore.Mvc;
using UnityPipelineWebApi.Services;

namespace UnityPipelineWebApi.Controllers;

[Route("api")]
[ApiController]
public class FileController(FileService fileService) : ControllerBase
{
    [HttpPost("/[controller]/{buildName}")]
    public async Task<IActionResult> UploadFile(IFormFile file, Guid buildName)
    {
        try
        {
            return Ok(await fileService.SaveFile(file, buildName));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}