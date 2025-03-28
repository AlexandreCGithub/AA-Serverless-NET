namespace sample1;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Formats.Jpeg;

class Program
{
    static void Main(string[] args)
    {
        // 1. Hello World!
        var p1 = new Personne("Paul", 20);
        //Console.WriteLine(p1.Hello(true));
        //Console.WriteLine(p1.Hello(false));
        
        // 2. NuGet packages
        string json = JsonConvert.SerializeObject(p1, Formatting.Indented);
        Console.WriteLine(json);

        // 3. Traitement d'image locale
        string inputFile = "./chat.jpg";  // Dossier source
        string outputFile = "./output.jpg"; // Dossier destination

        ProcessImage(inputFile, outputFile);
    }

static void ProcessImage(string filePath, string outputFile)
{
    try
    {
        using (Image image = Image.Load(filePath))
        {
            image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2));
            image.Save(outputFile);
            Console.WriteLine($"Image sauvegardée: {outputFile}");
        }

        // Identifier l'image redimensionnée
        ImageInfo imageInfo = Image.Identify(outputFile);
        Console.WriteLine($"Width: {imageInfo.Width}");
        Console.WriteLine($"Height: {imageInfo.Height}");

        using (Image imageM = Image.Load(outputFile))
        {
            ImageMetadata metadata = imageM.Metadata;
            
            // Extraction des métadonnées JPEG si disponible
            JpegMetadata jpegData = metadata.GetJpegMetadata();
            if (jpegData != null)
            {
                Console.WriteLine("JPEG Metadata:");
                Console.WriteLine($"Quality: {jpegData.Quality}");
            }

            // Affichage des métadonnées EXIF si elles existent
            if (metadata.ExifProfile != null)
            {
                Console.WriteLine("EXIF Metadata:");
                foreach (var tag in metadata.ExifProfile.Values)
                {
                    Console.WriteLine($"{tag.Tag}: {tag.GetValue()}");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erreur avec {filePath}: {ex.Message}");
    }
}
}


public class Personne
{
    public string Nom { get; set; }
    public int Age { get; set; }

    public Personne(string nom, int age)
    {
        Nom = nom;
        Age = age;
    }

    public string Hello(bool isLowercase)
    {
        string message = $"hello {Nom}, you are {Age}";
        return isLowercase ? message.ToLower() : message.ToUpper();
    }
}
