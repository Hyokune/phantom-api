using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using PhantomAPI.Models;

namespace PhantomAPI.Helpers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhantomController : ControllerBase
    {
        private readonly PhantomAPIContext _context;
        private IConfiguration _configuration;

        public PhantomController(PhantomAPIContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Phantom
        [HttpGet]
        public IEnumerable<PhantomThread> GetPhantomThread()
        {
            return _context.PhantomThread;
        }

        // GET: api/Phantom/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPhantomThread([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var phantomThread = await _context.PhantomThread.FindAsync(id);

            if (phantomThread == null)
            {
                return NotFound();
            }

            return Ok(phantomThread);
        }

        // PUT: api/Phantom/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPhantomThread([FromRoute] int id, [FromBody] PhantomThread phantomThread)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != phantomThread.Id)
            {
                return BadRequest();
            }

            _context.Entry(phantomThread).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PhantomThreadExists(id))
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

        // POST: api/Phantom
        [HttpPost]
        public async Task<IActionResult> PostPhantomThread([FromBody] PhantomThread phantomThread)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.PhantomThread.Add(phantomThread);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPhantomThread", new { id = phantomThread.Id }, phantomThread);
        }

        // DELETE: api/Phantom/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhantomThread([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var phantomThread = await _context.PhantomThread.FindAsync(id);
            if (phantomThread == null)
            {
                return NotFound();
            }

            _context.PhantomThread.Remove(phantomThread);
            await _context.SaveChangesAsync();

            return Ok(phantomThread);
        }

        private bool PhantomThreadExists(int id)
        {
            return _context.PhantomThread.Any(e => e.Id == id);
        }

        // GET: api/Phantom/threads/{user}
        [Route("threads/{user}")]
        [HttpGet]
        public async Task<List<PhantomThread>> GetUserThreads([FromRoute] string user)
        {
            var threads = (from m in _context.PhantomThread
                           where user == m.User
                           select m).Distinct();

            var returned = await threads.ToListAsync();

            return returned;
        }

        // GET: api/Phantom/threads/
        [Route("threads")]
        [HttpGet]
        public IEnumerable<PhantomThread> GetUserThreads()
        {
            return _context.PhantomThread;
        }


        // POST: api/Phantom/Upload
        [HttpPost, Route("upload/")]
        public async Task<IActionResult> UploadFile([FromForm] PhantomThreadItem thread)
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }
            try
            {
                using (var stream = thread.Image.OpenReadStream())
                {
                    var cloudBlock = await UploadToBlob(thread.Image.FileName, null, stream);
                    //// Retrieve the filename of the file you have uploaded
                    //var filename = provider.FileData.FirstOrDefault()?.LocalFileName;
                    if (string.IsNullOrEmpty(cloudBlock.StorageUri.ToString()))
                    {
                        return BadRequest("An error has occured while uploading your file. Please try again.");
                    }

                    PhantomThread phantomThread = new PhantomThread();
                    phantomThread.Title = thread.Title;
                    phantomThread.Content = thread.Content;
                    phantomThread.User = thread.User;

                    System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                    phantomThread.Height = image.Height.ToString();
                    phantomThread.Width = image.Width.ToString();
                    phantomThread.Url = cloudBlock.SnapshotQualifiedUri.AbsoluteUri;

                    System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();
                    var myTimeZone = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
                    var currentDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, myTimeZone);
                    phantomThread.Uploaded = currentDateTime.ToString();

                    _context.PhantomThread.Add(phantomThread);
                    await _context.SaveChangesAsync();

                    return Ok($"File: {thread.Title} has successfully uploaded");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"An error has occured. Details: {ex.Message}");
            }


        }

        // POST: api/Phantom/Upload
        [HttpPost, Route("upload/noimg")]
        public async Task<IActionResult> UploadThread([FromForm] PhantomThreadItemText thread)
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }
            try
            {
                PhantomThread phantomThread = new PhantomThread();
                phantomThread.Title = thread.Title;
                phantomThread.Content = thread.Content;
                phantomThread.User = thread.User;

                phantomThread.Height = "";
                phantomThread.Width = "";
                phantomThread.Url = "";


                System.Globalization.CultureInfo.CurrentCulture.ClearCachedData();
                var myTimeZone = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
                var currentDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, myTimeZone);
                phantomThread.Uploaded = currentDateTime.ToString();

                _context.PhantomThread.Add(phantomThread);
                await _context.SaveChangesAsync();

                return Ok($"File: {thread.Title} has successfully uploaded");
                
            }
            catch (Exception ex)
            {
                return BadRequest($"An error has occured. Details: {ex.Message}");
            }


        }

        private async Task<CloudBlockBlob> UploadToBlob(string filename, byte[] imageBuffer = null, System.IO.Stream stream = null)
        {

            var accountName = _configuration["AzureBlob:name"];
            var accountKey = _configuration["AzureBlob:key"]; ;
            var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer imagesContainer = blobClient.GetContainerReference("images");

            string storageConnectionString = _configuration["AzureBlob:connectionString"];

            // Check whether the connection string can be parsed.
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                try
                {
                    // Generate a new filename for every new blob
                    var fileName = Guid.NewGuid().ToString();
                    fileName += GetFileExtention(filename);

                    // Get a reference to the blob address, then upload the file to the blob.
                    CloudBlockBlob cloudBlockBlob = imagesContainer.GetBlockBlobReference(fileName);

                    if (stream != null)
                    {
                        await cloudBlockBlob.UploadFromStreamAsync(stream);
                    }
                    else
                    {
                        return new CloudBlockBlob(new Uri(""));
                    }

                    return cloudBlockBlob;
                }
                catch (StorageException ex)
                {
                    return new CloudBlockBlob(new Uri(""));
                }
            }
            else
            {
                return new CloudBlockBlob(new Uri(""));
            }

        }

        private string GetFileExtention(string fileName)
        {
            if (!fileName.Contains("."))
                return ""; //no extension
            else
            {
                var extentionList = fileName.Split('.');
                return "." + extentionList.Last(); //assumes last item is the extension 
            }
        }
    }
}