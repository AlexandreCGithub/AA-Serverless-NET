namespace sample1;
public class Personne
{
    public string nom { get; set; }
    public int age { get; set; }

    public Personne(string nom, int age)
    {
        this.nom = nom;
        this.age = age;
    }

    public string Hello(bool isLowercase)
    {
        string message = $"hello {nom}, you are {age}";
        return isLowercase ? message.ToLower() : message.ToUpper();
    }
}