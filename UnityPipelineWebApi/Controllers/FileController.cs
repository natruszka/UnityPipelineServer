using Microsoft.AspNetCore.Mvc;
using UnityPipelineWebApi.Services;

namespace UnityPipelineWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FileController(FileService fileService) : ControllerBase
{
    [HttpPost("files")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        try
        {
            await fileService.SaveFile(file);
            return Created();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}