using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;


namespace StudentCorreiaLegrand
{
    public static class ResizeHttpTrigger
    {
        [FunctionName("ResizeHttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // Récupération des paramètres w et h
            if (!req.Query.ContainsKey("w") || !req.Query.ContainsKey("h"))
            {
                return new BadRequestObjectResult("Les paramètres 'w' et 'h' sont requis.");
            }

            if (!int.TryParse(req.Query["w"], out int w) || !int.TryParse(req.Query["h"], out int h) || w <= 0 || h <= 0)
            {
                return new BadRequestObjectResult("Les paramètres 'w' et 'h' doivent être des entiers positifs.");
            }

            if (w > 2000 || h > 2000)
            {
                return new BadRequestObjectResult("Les paramètres 'w' et 'h' doivent être inférieurs ou égaux à 2000.");
            }

            byte[] targetImageBytes;
            try
            {
                using (var msInput = new MemoryStream())
                {
                    await req.Body.CopyToAsync(msInput);
                    msInput.Position = 0;

                    if (msInput.Length == 0)
                    {
                        return new BadRequestObjectResult("Aucune image envoyée.");
                    }

                    // Vérification de la taille de l'image
                    const long maxFileSize = 5 * 1024 * 1024; // 5 MB
                    if (msInput.Length > maxFileSize)
                    {
                        return new ObjectResult("La taille de l'image dépasse la limite autorisée de 5 MB.")
                        {
                            StatusCode = StatusCodes.Status413PayloadTooLarge
                        };
                    }

                    using (var image = Image.Load(msInput))
                    {
                        // Redimensionner l'image
                        image.Mutate(x => x.Resize(w, h));

                        using (var msOutput = new MemoryStream())
                        {
                            image.SaveAsJpeg(msOutput);
                            targetImageBytes = msOutput.ToArray();
                        }
                    }
                }
            }
            catch (SixLabors.ImageSharp.UnknownImageFormatException)
            {
                return new BadRequestObjectResult("Le format de l'image n'est pas pris en charge.");
            }
            catch (Exception ex)
            {
                log.LogError($"Erreur interne: {ex.Message}");
                return new ObjectResult("Une erreur interne est survenue, veuillez réessayer plus tard.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
            return new FileContentResult(targetImageBytes, "image/jpeg");
        }
    }
}

