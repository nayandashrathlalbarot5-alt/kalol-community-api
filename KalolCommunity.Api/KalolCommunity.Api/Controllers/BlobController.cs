using System;
using System.Net;
using System.Threading.Tasks;
using KalolCommunity.Application.Common;
using KalolCommunity.Application.Interfaces.Infrastructure;
using KalolCommunity.Contracts.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KalolCommunity.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class BlobController : ControllerBase
    {
        private readonly IBlobService _blobService;

        public BlobController(IBlobService blobService)
        {
            _blobService = blobService;
        }

        /// <summary>
        /// Upload profile photo to temporary storage
        /// </summary>
        /// <param name="file">Image file (jpg/png, max 2MB)</param>
        /// <returns>Generated unique file name</returns>
        [HttpPost("upload-temp")]
        public async Task<IActionResult> UploadTemporaryPhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<BlobUploadResponseDTO>
                {
                    Success = false,
                    Message = ResponseMessages.NoFileProvided,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = null
                });
            }

            try
            {
                Console.WriteLine("API HIT TEST");
                using var stream = file.OpenReadStream();
                string fileName = await _blobService.UploadTempAsync(stream, file.FileName);

                return Ok(new ApiResponse<BlobUploadResponseDTO>
                {
                    Success = true,
                    Message = ResponseMessages.FileUploadedSuccessfully,
                    StatusCode = (int)HttpStatusCode.OK,
                    Data = new BlobUploadResponseDTO
                    {
                        FileName = fileName,
                        Message = ResponseMessages.TemporaryFileReadyForSubmission
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<BlobUploadResponseDTO>
                {
                    Success = false,
                    Message = ex.Message,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = null
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<BlobUploadResponseDTO>
                {
                    Success = false,
                    Message = ResponseMessages.FileUploadError,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Data = null
                });
            }
        }

        /// <summary>
        /// Delete temporary photo (if registration is cancelled)
        /// </summary>
        /// <param name="fileName">File name to delete</param>
        [HttpDelete("delete-temp/{fileName}")]
        public async Task<IActionResult> DeleteTemporaryPhoto(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ResponseMessages.FileNameRequired,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = null
                });
            }

            try
            {
                await _blobService.DeleteTempAsync(fileName);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = ResponseMessages.FileDeletedSuccessfully,
                    StatusCode = (int)HttpStatusCode.OK,
                    Data = null
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = ResponseMessages.FileDeleteError,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Data = null
                });
            }
        }
    }
}