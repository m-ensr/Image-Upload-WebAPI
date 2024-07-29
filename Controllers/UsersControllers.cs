using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using image_upload.Models;
using image_upload.Data;

namespace image_upload.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<UsersController> _logger;
        private readonly ApplicationDbContext _context;

        public UsersController(IWebHostEnvironment webHostEnvironment, ILogger<UsersController> logger, ApplicationDbContext context)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _context = context;
        }

        [HttpPost("upload-photo")]
        public async Task<IActionResult> UploadPhoto([FromForm] int userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                var existingPhoto = await _context.ImageCollections
                    .FirstOrDefaultAsync(ic => ic.UserID == userId);
                if (existingPhoto != null)
                {
                    var oldPhotoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", existingPhoto.PhotoPath);
                    if (System.IO.File.Exists(oldPhotoPath))
                    {
                        System.IO.File.Delete(oldPhotoPath);
                    }

                    _context.ImageCollections.Remove(existingPhoto);
                }

                var newPhotoPath = fileName;
                var newPhoto = new ImageCollection
                {
                    UserID = userId,
                    PhotoPath = newPhotoPath
                };
                _context.ImageCollections.Add(newPhoto);
                await _context.SaveChangesAsync();

                var photoUrl = $"/images/{newPhotoPath}";

                return Ok(new { PhotoUrl = photoUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photo.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error uploading photo.");
            }
        }

        [HttpGet("get-photo")]
        public IActionResult GetPhoto([FromQuery] int userId)
        {

            var photoRecord = _context.ImageCollections
                .FirstOrDefault(ic => ic.UserID == userId);

            if (photoRecord == null)
            {
                return NotFound();
            }

            var photoPath = photoRecord.PhotoPath;
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", photoPath);

            _logger.LogInformation($"Full path to file: {fullPath}");

            if (!System.IO.File.Exists(fullPath))
            {
                _logger.LogError($"File not found: {fullPath}");
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, "image/jpeg");
        }

        [HttpDelete("delete-photo")]
        public async Task<IActionResult> DeletePhoto([FromQuery] int userId)
        {
            var photoRecord = await _context.ImageCollections
                .FirstOrDefaultAsync(ic => ic.UserID == userId);

            if (photoRecord == null)
            {
                return NotFound();
            }

            var photoPath = photoRecord.PhotoPath;
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", photoPath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            photoRecord.PhotoPath = string.Empty;
            _context.ImageCollections.Update(photoRecord);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
