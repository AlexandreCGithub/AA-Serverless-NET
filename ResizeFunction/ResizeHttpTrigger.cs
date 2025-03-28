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
        // Vérification des paramètres w et h
        if (!int.TryParse(req.Query["w"], out int w) || !int.TryParse(req.Query["h"], out int h) || w <= 0 || h <= 0)
        {
            return new BadRequestObjectResult("Les paramètres 'w' et 'h' doivent être des entiers positifs.");
        }

        if (w > 2000 || h > 2000)
        {
            return new BadRequestObjectResult("Les paramètres 'w' et 'h' doivent être inférieur ou égal à 2000.");
        }

        // Lire le corps de la requête
        string requestBody = String.Empty;
        try
        {
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            // Vérifier si le corps de la requête est vide
            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Aucune image envoyée.");
            }

            byte[] imageBytes = Encoding.UTF8.GetBytes(requestBody);

            // Vérification de la taille du fichier
            const long maxFileSize = 5 * 1024 * 1024; // Limite de 5 Mo
            if (imageBytes.Length > maxFileSize)
            {
                return new ObjectResult("La taille de l'image dépasse la limite autorisée.")
                {
                    StatusCode = StatusCodes.Status413PayloadTooLarge
                };
            }

            // Traiter l'image (redimensionnement)
            byte[] targetImageBytes;

            using (var msInput = new MemoryStream(imageBytes))
            {
                try
                {
                    using (var image = Image.Load(msInput))
                    {
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

            // Retourner l'image redimensionnée
            return new FileContentResult(targetImageBytes, "image/jpeg");
        }
        catch (Exception ex)
        {
            log.LogError($"Erreur interne: {ex.Message}");
            return new ObjectResult("Une erreur interne est survenue, veuillez réessayer plus tard.")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}

