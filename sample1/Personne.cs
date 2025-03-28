namespace sample1;
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