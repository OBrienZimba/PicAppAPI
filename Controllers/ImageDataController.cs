using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PicAppAPI.Data;

using PicAppAPI.Models;

namespace PicAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageDataController : ControllerBase
    {
        private readonly PicAppAPIContext _context;

        public ImageDataController(PicAppAPIContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImageData>>> GetImageData()
        {
            var imageDataList = await _context.ImageData.ToListAsync();

            foreach (var imageData in imageDataList)
            {
                // Build the full URL for the image file path
                var fileName = Path.GetFileName(imageData.FilePath);
                imageData.FilePath = $"{Request.Scheme}://{Request.Host}/images/{fileName}";
            }

            return Ok(imageDataList);
        }





        // GET: api/ImageData/5
        [HttpGet("file/{id}")]
        public async Task<IActionResult> GetImageAsFile(int id)
        {
            var imageData = await _context.ImageData.FindAsync(id);

            if (imageData == null)
            {
                return NotFound(new { message = $"Image with ID {id} not found" });
            }

            // Build the full file path
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageData.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = "Image file not found on the server" });
            }

            var contentType = "image/jpeg"; 
            return PhysicalFile(filePath, contentType, imageData.FileName);
        }


        // PUT: api/ImageData/
        [HttpPut("{id}")]
        public async Task<IActionResult> PutImageData(int id, ImageData imageData)
        {
            if (id != imageData.Id)
            {
                return BadRequest();
            }

            _context.Entry(imageData).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImageDataExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<ImageData>> PostImageData(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("Image file is missing.");
            }

            // Define the folder path
            var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

            // Ensure the folder exists
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            // Generate a unique file name
            var uniqueFileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
            var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

            try
            {
                // Save the file to the specified path
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Save file data to the database with the file path
                var imageData = new ImageData
                {
                    FileName = imageFile.FileName,
                    FilePath = $"/images/{uniqueFileName}", // Relative path for serving image
                    UploadDate = DateTime.UtcNow
                };

                _context.ImageData.Add(imageData);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetImageData", new { id = imageData.Id }, imageData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }




        // DELETE: api/ImageData/id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImageData(int id)
        {
            var imageData = await _context.ImageData.FindAsync(id);

            if (imageData == null)
            {
                return NotFound(new { message = $"Image with ID {id} not found" });
            }

            _context.ImageData.Remove(imageData);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting image", details = ex.Message });
            }
            return NoContent();
        }


        private bool ImageDataExists(int id)
        {
            return _context.ImageData.Any(e => e.Id == id);
        }
    }
}
