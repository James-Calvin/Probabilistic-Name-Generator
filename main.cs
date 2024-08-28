using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;


class Program
{
    public static Version version = new(3, 0, 0);

    const string GIVEN_NAMES_ALL = "DATA/Given-Names/All.txt";
    const string GIVEN_NAMES_FEMALES = "DATA/Given-Names/Female.txt";
    const string GIVEN_NAMES_MALES = "DATA/Given-Names/Male.txt";
    const string GIVEN_NAMES_NEUTRAL = "DATA/Given-Names/Neutral.txt";

    const string SURNAMES_ALL = "DATA/Surnames/Surnames_All.csv";
    const string SURNAMES_AMERICAN = "DATA/Surnames/Surnames_American.csv"; // Native north-american
    const string SURNAMES_ASIAN = "DATA/Surnames/Surnames_Asian.csv";
    const string SURNAMES_BLACK = "DATA/Surnames/Surnames_Black.csv";
    const string SURNAMES_HISPANIC = "DATA/Surnames/Surnames_Hispanic.csv";
    const string SURNAMES_WHITE = "DATA/Surnames/Surnames_White.csv";

    enum ArgType
    {
        Gender,
        Race,
        Beta,
        Unknown
    }

    static ArgType GetArgType(string arg)
    {
        try
        {
            double.Parse(arg);
            return ArgType.Beta;
        }
        catch { }

        switch (arg.ToLower())
        {
            case "female":
            case "f":
                return ArgType.Gender;
            case "m":
            case "male":
                return ArgType.Gender;
            case "n":
            case "neutral":
                return ArgType.Gender;

            case "american":
                return ArgType.Race;
            case "asian":
                return ArgType.Race;
            case "black":
                return ArgType.Race;
            case "hispanic":
                return ArgType.Race;
            case "white":
                return ArgType.Race;

            default:
                return ArgType.Unknown;
        }
    }

    static string GetGivenNamePath(string gender)
    {
        switch (gender.ToLower())
        {
            case "female":
            case "f":
                return GIVEN_NAMES_FEMALES;

            case "m":
            case "male":
                return GIVEN_NAMES_MALES;

            case "n":
            case "neutral":
                return GIVEN_NAMES_NEUTRAL;

            default:
                return GIVEN_NAMES_ALL;
        }
    }

    static string GetSurnamePath(string race)
    {
        switch (race.ToLower())
        {

            case "american":
                return SURNAMES_AMERICAN;
            case "asian":
                return SURNAMES_ASIAN;
            case "black":
                return SURNAMES_BLACK;
            case "hispanic":
                return SURNAMES_HISPANIC;
            case "white":
                return SURNAMES_WHITE;

            default:
                return SURNAMES_ALL;
        }
    }

    static string GetExecutableDirectory()
    {
        return Path.GetDirectoryName(AppContext.BaseDirectory);
    }

    static string GetDataFilePath(string relativePath)
    {
        return Path.Combine(GetExecutableDirectory(), relativePath);
    }

    public static void Main(string[] args)
    {
        // Utility arguments
        if (args.Length > 0)
        {
            switch (args[0].ToLower())
            {
                case "help":
                case "h":
                case "?":
                    Console.WriteLine("Usage: namegen {gender} {rarity1} {race} {rarity2}");
                    Console.WriteLine("- gender: [male | m | female | f | neutral | n]");
                    Console.WriteLine("- rarity1: the given name (first name) rarity");
                    Console.WriteLine("- race: [american | asian | black | hispanic | white]");
                    Console.WriteLine("  - Note: race attribute affects surname only.");
                    Console.WriteLine("- rarity2: the surname (last name) rarity");
                    Console.WriteLine("All arguments are optional.");
                    Console.WriteLine("The order of arguments does not matter (except that rarity1 must appear before rarity2). ");
                    Console.WriteLine("Rarity: values range from 0 to 100. Default is 50 (follows the same distr)");
                    Console.WriteLine("- 0 makes common names are extremely likely (expect to see duplicates)");
                    Console.WriteLine("- 50 (default) where the outputs follow the same distribution as the data");
                    Console.WriteLine("- 75 is where all names are equally likely to appear");
                    Console.WriteLine("- 100 makes rare names more common than common names");
                    Console.WriteLine("Example usage:");
                    Console.WriteLine("namegen");
                    Console.WriteLine(" output: \"Anna Wright\" note: will generate a name based on data (names are as likely to appear as often they appear in the data)");
                    Console.WriteLine("namegen male 100 black 0");
                    Console.WriteLine(" output: \"Terell Bailey\" note: will generate a more rare male given name with a common black surname");
                    Console.WriteLine("namegen f 80");
                    Console.WriteLine(" output: \"Maisha Camero\" note: will generate a more rare female given name and a more rare surname");
                    return;

                case "v":
                case "version":
                    Console.WriteLine($"Version {version.ToString(3)}");
                    return;
            }
        }

        // Setting default values
        string givenNameDataPath = GIVEN_NAMES_ALL;
        string surnameDataPath = SURNAMES_ALL;
        double givenNameRarity = 50;
        double surnameRarity = 50;

        // Harvest any entered beta values
        List<double> betas = new List<double>();
        for (int i = 0; i < args.Length; i++)
        {
            try
            {
                double betaValue = double.Parse(args[i]);
                betas.Add(betaValue);
            }
            catch { }
        }

        // Set beta values if captured
        if (betas.Count > 0)
        {
            givenNameRarity = betas[0];

            if (betas.Count > 1)
            {
                surnameRarity = betas[1];
            }
            else
            {
                surnameRarity = betas[0];
            }
        }

        // Check for name data specifications
        for (int i = 0; i < args.Length; i++)
        {
            var type = GetArgType(args[i]);

            switch (type)
            {
                case ArgType.Gender:
                    givenNameDataPath = GetGivenNamePath(args[i]);
                    break;

                case ArgType.Race:
                    surnameDataPath = GetSurnamePath(args[i]);
                    break;
            }
        }

        static double Map(double value, double inMin, double inMax, double outMin, double outMax)
        {
            return (value - inMin) / (inMax - inMin) * (outMax - outMin) + outMin;
        }

        // Helper function to transform user input to beta value before loading data
        static double RarityToBeta(double rarity)
        {
            rarity = Math.Clamp(rarity, 0, 100);
            if (rarity <= 50)
                return Map(rarity, 0, 50, 2, 1);
            return Map(rarity, 50, 100, 1, -1);
        }

        // This function just cleans up duplicate loading data code
        static NameData LoadNameData(string path, double rarity)
        {
            var filepath = GetDataFilePath(path);
            double beta = RarityToBeta(rarity);
            return NameData.Load(filepath, beta);
        }

        // Load the relative data with the beta values
        var givenNameData = LoadNameData(givenNameDataPath, givenNameRarity);
        var surnameData = LoadNameData(surnameDataPath, surnameRarity);

        // Generate the name
        var name = NameGenerator.Next(givenNameData, surnameData);

        // Convert name to Title Case
        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        name = textInfo.ToTitleCase(name.ToLower()); // Must call ToLower to work properly
        Console.WriteLine(name);
    }
}