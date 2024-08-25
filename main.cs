using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class NameDataEntry
{
    public string Name { get; }
    public int Count { get; }
    public double Probability { get; set; } = 0;

    public NameDataEntry(string name, int count)
    {
        Name = name;
        Count = count;
    }

    override public string ToString()
    {
        return Name;
    }
}

class NameData
{
    private List<NameDataEntry> _data;

    private NameData()
    {
        _data = new();
    }

    public static NameData Load(string filepath, double beta = 1)
    {
        var collection = new NameData();
        StreamReader reader = new(filepath);
        string line;

        while ((line = reader.ReadLine()) != null)
        {
            string[] data = line.Split(",");
            if (data.Length != 2) continue;
            string name = data[0];
            // It is possible for this to fail due to malformed file
            int count = int.Parse(data[1]);
            collection.Add(new NameDataEntry(name, count));
        }
        reader.Close();

        collection.CalculateProbabilities(beta);

        return collection;
    }

    private void Add(NameDataEntry data)
    {
        _data.Add(data);
    }

    public void CalculateProbabilities(double beta = 1)
    {
        double transform(int input)
        {
            return Math.Pow(input, beta);
        }

        // Calculate the total amount after transforming the data
        double total = 0;
        foreach (var data in _data)
        {
            total += transform(data.Count);
        }

        // Calculate the probabilities
        foreach (var data in _data)
        {
            data.Probability = transform(data.Count) / total;
        }
    }

    public string GetName(double position)
    {
        double total = 0;
        foreach (var data in _data)
        {
            total += data.Probability;
            if (total >= position) return data.Name;
        }
        return null;
        // throw new ArgumentOutOfRangeException("position must be between 0 and 1");
    }
}

class NameGenerator
{
    private static Random random = new();

    public static string Next(NameData givenData, NameData surnameData)
    {
        string given = givenData.GetName(random.NextDouble());
        string surname = surnameData.GetName(random.NextDouble());
        return $"{given} {surname}";
    }

    public static List<string> Next(NameData givenData, NameData surnameData, int count)
    {
        List<string> names = new();
        for (int i = 0; i < count; i++)
        {
            names.Add(Next(givenData, surnameData));
        }
        return names;
    }
}

class Program
{
    public static double GetDoubleFromUser(string prompt, double? defaultValue = null)
    {
        double output;
        string response = "";
        while (true)
        {
            try
            {
                Console.Write(prompt);
                response = Console.ReadLine();
                output = double.Parse(response);
                break;
            }
            catch
            {
                if (response.Trim() == "" && defaultValue != null) return (double)defaultValue;
                Console.WriteLine("Invalid. Please try again.");
            }
        }

        return output;
    }

    public static void Main(string[] args)
    {
        Console.WriteLine(args.Length);

        Console.WriteLine("What is the beta value?");
        Console.WriteLine("beta determines how likely common names are to appear");
        Console.WriteLine(">1 makes common names are extemely likely to appear");
        Console.WriteLine("1 makes common names as likely as they appear in the data");
        Console.WriteLine("0 makes all names equally likely to appear");
        Console.WriteLine("<0 makes common names unlikely to appear");
        Console.WriteLine("---");
        Console.WriteLine("You may leave the entry blank to use the default value of 1");

        double betaGivenName = GetDoubleFromUser("Enter the beta value for given names: ", 1);
        double betaSurname = GetDoubleFromUser("Enter the beta value for given names: ", 1);


        // Loading given names
        var givenAll = NameData.Load("DATA/Given Names/All.txt", betaGivenName);
        var givenFemales = NameData.Load("DATA/Given Names/Female.txt", betaGivenName);
        var givenMales = NameData.Load("DATA/Given Names/Male.txt", betaGivenName);
        var givenNeutrals = NameData.Load("DATA/Given Names/Neutral.txt", betaGivenName);

        // Loading surnames
        var surnamesAll = NameData.Load("DATA/Surnames/Surnames_All.csv", betaSurname);
        var surnamesWhite = NameData.Load("DATA/Surnames/Surnames_White.csv", betaSurname);
        var surnamesBlack = NameData.Load("DATA/Surnames/Surnames_Black.csv", betaSurname);
        var surnamesAsian = NameData.Load("DATA/Surnames/Surnames_Asian.csv", betaSurname);
        var surnamesAmerican = NameData.Load("DATA/Surnames/Surnames_American.csv", betaSurname);
        var surnamesHispanic = NameData.Load("DATA/Surnames/Surnames_Hispanic.csv", betaSurname);

        // Random random = new();

        // Selecting 15 "All" names 
        Console.WriteLine();
        Console.WriteLine("--- General Names ---");
        var generalNames = NameGenerator.Next(givenAll, surnamesAll, 15);
        for (int i = 0; i < generalNames.Count; i++) Console.WriteLine($"{i + 1}. {generalNames[i]}");
        Console.WriteLine();


        string GetNameList(string race, NameData surname, bool willDisplay = true)
        {
            string output = "";
            output += ($"--- {race} Names ---\n");
            output += ("Females:\n");
            var femalesNames = NameGenerator.Next(givenFemales, surname, 5);
            for (int i = 0; i < femalesNames.Count; i++) output += ($"{i + 1:d2}. {femalesNames[i]}\n");
            output += ("Males:\n");
            var maleNames = NameGenerator.Next(givenMales, surname, 5);
            for (int i = 0; i < maleNames.Count; i++) output += ($"{i + 6:d2}. {maleNames[i]}\n");
            output += ("Neutrals:\n");
            var neutralNames = NameGenerator.Next(givenNeutrals, surname, 5);
            for (int i = 0; i < neutralNames.Count; i++) output += ($"{i + 11}. {neutralNames[i]}\n");

            if (willDisplay) Console.WriteLine(output);

            return output;
        }

        string fileContent = "";

        fileContent += GetNameList("White", surnamesWhite);
        fileContent += "\n";

        fileContent += GetNameList("Black", surnamesBlack);
        fileContent += "\n";

        fileContent += GetNameList("Asian and Pacific Islander", surnamesAsian);
        fileContent += "\n";

        fileContent += GetNameList("American Indian and Alaska Native", surnamesAmerican);
        fileContent += "\n";

        fileContent += GetNameList("Hispanic", surnamesHispanic);
        fileContent += "\n";

        string footer = "---\n\n";
        footer += ("Note: Only surnames have race data associated with it. ");
        footer += ("All surname data is from the US's 2010 census. ");
        footer += ("All given name data is from the ");
        footer += ("Social Security Administration's Georgia's ");
        footer += ("new baby data including years 1984 - 2023. ");
        footer += ("Gender neutral names were generated by an intersection ");
        footer += ("of the male and female given name data.");
        Console.WriteLine(footer);

        string directory = "OUTPUT";
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        DateTime now = DateTime.Now;
        string filename = $"Names_{now.ToString("yyyyMMddHHmmss")}(beta: {betaGivenName},{betaSurname}).txt";
        using (StreamWriter writer = new(directory + "/" + filename))
        {
            writer.WriteLine($"A beta value of {betaGivenName} was used to generate the following given names and a beta value of {betaSurname} was used to generate the surnames.\n");
            writer.WriteLine(fileContent);
            writer.WriteLine(footer);
        }
    }
}