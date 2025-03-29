namespace sample1;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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
        string inputFolder = "./images";  // Dossier source
        string outputFolder = "./outputFolder"; // Dossier destination
        
        string[] files = Directory.GetFiles(inputFolder, "*.jpg"); // Liste des fichiers JPG
        Console.WriteLine($"Nombre d'images trouvées: {files.Length}");
        
        
        // Traitement séquentiel des images
        Stopwatch sequentielStopwatch = Stopwatch.StartNew();
        foreach (string file in files)
        {
            ProcessImage(file, outputFolder);
        }
        sequentielStopwatch.Stop();

        Console.WriteLine($"Traitement séquentiel des images terminé en {sequentielStopwatch.ElapsedMilliseconds} ms");
        
        // Traitement parallèle des images
        Stopwatch paralleleStopwatch = Stopwatch.StartNew();
        Parallel.ForEach(files, file =>
        {
            ProcessImage(file, outputFolder);
        });
        paralleleStopwatch.Stop();
        
        Console.WriteLine($"Traitement parallèle des images terminé en {paralleleStopwatch.ElapsedMilliseconds} ms");

    }

    static void ProcessImage(string filePath, string outputFolder)
    {
        try
        {
            string outputFilePath = Path.Combine(outputFolder, Path.GetFileName(filePath));
            using (Image image = Image.Load(filePath))
            {
                image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2));
                image.Save(outputFilePath);
                //Console.WriteLine($"Image sauvegardée: {outputFilePath}");
            }

            // Identifier l'image redimensionnée
            ImageInfo inputImageInfo = Image.Identify(filePath);
            //Console.WriteLine($"Width input: {inputImageInfo.Width}");
            //Console.WriteLine($"Height input: {inputImageInfo.Height}");

            ImageInfo outputImageInfo = Image.Identify(outputFilePath);
            //Console.WriteLine($"Width output: {outputImageInfo.Width}");
            //Console.WriteLine($"Height output: {outputImageInfo.Height}");

            using (Image imageM = Image.Load(outputFilePath))
            {
                ImageMetadata metadata = imageM.Metadata;
                
                // Extraction des métadonnées JPEG si disponible
                JpegMetadata jpegData = metadata.GetJpegMetadata();
                if (jpegData != null)
                {
                    //Console.WriteLine("JPEG Metadata:");
                    //Console.WriteLine($"Quality: {jpegData.Quality}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur avec {filePath}: {ex.Message}");
        }
    }
}
