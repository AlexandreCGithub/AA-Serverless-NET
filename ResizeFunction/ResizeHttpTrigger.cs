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
            if (!int.TryParse(req.Query["w"], out int w) || !int.TryParse(req.Query["h"], out int h) || w <= 0 || h <= 0)
            {
                return new BadRequestObjectResult("Les paramètres 'w' et 'h' doivent être des entiers positifs.");
            }

            if (w > 2000 || h > 2000)
            {
                return new BadRequestObjectResult("Les paramètres 'w' et 'h' doivent être inférieur ou égal à 2000.");
            }

            byte[] targetImageBytes;
            using (var msInput = new MemoryStream())
            {
                await req.Body.CopyToAsync(msInput);
                targetImageBytes = msInput.ToArray();
            }

            if (targetImageBytes.Length == 0)
            {
                return new BadRequestObjectResult("Aucune image envoyée.");
            }

            try
            {
                using (var msInput = new MemoryStream())
                {
                    await req.Body.CopyToAsync(msInput);
                    msInput.Position = 0;

                    // Optionnel : Vérifier la taille du fichier (Cas 9)
                    const long maxFileSize = 5 * 1024 * 1024; // par exemple, 5 MB
                    if (msInput.Length > maxFileSize)
                    {
                        return new ObjectResult("La taille de l'image dépasse la limite autorisée.")
                        {
                            StatusCode = StatusCodes.Status413PayloadTooLarge
                        };
                    }

                    try
                    {
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
                    catch (SixLabors.ImageSharp.UnknownImageFormatException)
                    {
                        return new BadRequestObjectResult("Le format de l'image n'est pas pris en charge.");
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"Erreur lors du traitement de l'image: {ex.Message}");
                        return new BadRequestObjectResult("Erreur lors du traitement de l'image. Assurez-vous que le fichier envoyé est une image valide.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Gestion globale des erreurs inattendues (Cas 7)
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

