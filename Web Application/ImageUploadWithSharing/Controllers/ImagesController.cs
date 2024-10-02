using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

using ImageSharingWithUpload.Models;
using Microsoft.Extensions.Logging;

namespace ImageSharingWithUpload.Controllers
{
    public class ImagesController : Controller
    {
        private readonly IWebHostEnvironment hostingEnvironment;

        private readonly ILogger logger;

        public ImagesController(IWebHostEnvironment environment, ILogger<ImagesController> logger)
        {
            hostingEnvironment = environment;
            this.logger = logger;
        }

        protected void mkDirectories()
        {
            var dataDir = Path.Combine(hostingEnvironment.WebRootPath, "data", "images");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            var infoDir = Path.Combine(hostingEnvironment.WebRootPath, "data", "info");
            if (!Directory.Exists(infoDir))
            {
                Directory.CreateDirectory(infoDir);
            }
        }

        protected string imageDataFile(string id)
        {
            return Path.Combine(
               hostingEnvironment.WebRootPath, "data", "images", id + ".jpg");
        }

        protected string imageInfoFile(string id)
        {
            return Path.Combine(
               hostingEnvironment.WebRootPath, "data", "info", id + ".js");
        }

        protected void CheckAda()
        {
            var cookie = Request.Cookies["ADA"];
            logger.LogDebug("ADA cookie value: " + cookie);
            if (cookie != null && "true".Equals(cookie))
            {
                ViewBag.isADA = true;
            }
            else
            {
                ViewBag.isADA = false;
            }
        }

        // TODO
        [HttpGet]
        public IActionResult Upload()
        {
            CheckAda();
            ViewBag.Message = "";
            return View();
        }

        // TODO
        [HttpPost]
        public async Task<IActionResult> Upload(Image image,
                                                IFormFile imageFile)
        {
            CheckAda();

            if (ModelState.IsValid)
            {
                var username = Request.Cookies["Username"];
                if (username == null)
                {
                    return RedirectToAction("Register", "Account");
                }

                image.Username = username;

                /*
                 * Save image information on the server file system.
                 */

                if (imageFile != null && imageFile.Length > 0)
                {
                    mkDirectories();

                    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

                    // TODO save image on server file system
                    var image_path = imageDataFile(image.Id);
                    if (imageFile.ContentType != "image/jpeg")
                    {
                        ViewBag.Message = "Image must be in jpg";
                        return View(image);
                    }
                    else
                    {
                        //using (var stream = new FileStream(imagePath, FileMode.Create))
                        using (var stream = System.IO.File.Create(image_path))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                    }


                    // Save metadata as JSON object on file system
                    var jsonData = JsonSerializer.Serialize(image, jsonOptions);
                    await System.IO.File.WriteAllTextAsync(imageInfoFile(image.Id), jsonData);

                    return View("Details", image);
                }
                else
                {
                    ViewBag.Message = "No image file specified!";
                    return View(image);
                }

            }
            else
            {
                ViewBag.Message = "Please correct the errors in the form!";
                return View(image);
            }
        }

        // TODO
        [HttpGet]
        public IActionResult Query()
        {
            CheckAda();
            ViewBag.Message = "";
            return View();
        }

        // TODO
        [HttpGet]
        public async Task<IActionResult> Details(Image image)
        {
            CheckAda();

            var username = Request.Cookies["Username"];
            if (username == null)
            {
                return RedirectToAction("Register", "Account");
            }

            String fileName = imageInfoFile(image.Id);
            if (System.IO.File.Exists(fileName))
            {
                String jsonData = await System.IO.File.ReadAllTextAsync(fileName);
                Image imageInfo = JsonSerializer.Deserialize<Image>(jsonData);

                return View(imageInfo);
            }
            else
            {
                ViewBag.Message = "Image with identifer " + image.Id + " not found";
                ViewBag.Id = image.Id;

                return View("Query");
            }

        }

    }
}