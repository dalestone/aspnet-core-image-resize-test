using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SkiaSharp;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {
        private IHostingEnvironment _env;

        public SampleDataController(IHostingEnvironment env)
        {
            _env = env;
        }

        private static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IList<IFormFile> files)
        {
            const int size = 150;
            const int quality = 75;

            var avatars = Path.Combine(_env.WebRootPath, "avatars");
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    using (var inputStream = new SKManagedStream(file.OpenReadStream()))
                    {
                        using (var original = SKBitmap.Decode(inputStream))
                        {
                            int width, height;
                            if (original.Width > original.Height)
                            {
                                width = size;
                                height = original.Height * size / original.Width;
                            }
                            else
                            {
                                width = original.Width * size / original.Height;
                                height = size;
                            }

                            using (var resized = original.Resize(new SKImageInfo(width, height), SKFilterQuality.High))
                            {
                                if (resized == null) return BadRequest();

                                using (var image = SKImage.FromBitmap(resized))
                                {
                                    var temp = image.Encode(SKEncodedImageFormat.Png, quality).AsStream();
                                    var filePath = Path.Combine(avatars, $"{Guid.NewGuid().ToString().Replace("-", "")}.png");

                                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                                    {
                                        await temp.CopyToAsync(fileStream);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Ok("avatar saved");
        }

        [HttpGet("[action]")]
        public IEnumerable<WeatherForecast> WeatherForecasts()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                DateFormatted = DateTime.Now.AddDays(index).ToString("d"),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            });
        }

        public class WeatherForecast
        {
            public string DateFormatted { get; set; }
            public int TemperatureC { get; set; }
            public string Summary { get; set; }

            public int TemperatureF
            {
                get
                {
                    return 32 + (int)(TemperatureC / 0.5556);
                }
            }
        }
    }
}
