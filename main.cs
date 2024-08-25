using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;


class Program
{
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
        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }

    static string GetDataFilePath(string relativePath)
    {
        return Path.Combine(GetExecutableDirectory(), relativePath);
    }

    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            switch (args[0].ToLower())
            {
                case "-help":
                case "help":
                case "-h":
                case "h":
                case "-?":
                case "?":
                    Console.WriteLine("Generate random names with the syntax:");
                    Console.WriteLine("  namegen {gender} {race} {beta1} {beta2}");
                    Console.WriteLine("gender: male | female | neutral");
                    Console.WriteLine("race: american | asian | black | hispanic | white");
                    Console.WriteLine("beta1: beta value for given name (first name)");
                    Console.WriteLine("beta2: beta value for the surname (last name)");
                    Console.WriteLine("All arguments are optional.");
                    Console.WriteLine("If no gender or race is selected, then names are generated from the entire set of name in the data.");
                    Console.WriteLine("Beta values represent how common of a name to generate.");
                    Console.WriteLine("Any value can be used (E.g. -2.3), but here are some interesting values to use:");
                    Console.WriteLine(": 2 means names common names will appear most of the time.");
                    Console.WriteLine(": 1 means names generate as often as they appear in the data.");
                    Console.WriteLine(": 0 means all names generate with equal chance of being selected.");
                    Console.WriteLine(": -1 means uncommon names are generated more often than common");
                    Console.WriteLine("The order of arguments does not matter (except that beta1 must appear before beta2). ");
                    Console.WriteLine("If only one beta value is given, that value will be used for both the given name and the surname.");
                    return;
            }
        }

        string givenNameDataPath = GIVEN_NAMES_ALL;
        string surnameDataPath = SURNAMES_ALL;
        double givenNameBeta = 1;
        double surnameBeta = 1;

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
            givenNameBeta = betas[0];

            if (betas.Count > 1)
            {
                surnameBeta = betas[1];
            }
            else
            {
                surnameBeta = betas[0];
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

        // Load the relative data with the beta values
        givenNameDataPath = GetDataFilePath(givenNameDataPath);
        var givenNameData = NameData.Load(givenNameDataPath, givenNameBeta);
        surnameDataPath = GetDataFilePath(surnameDataPath);
        var surnameData = NameData.Load(surnameDataPath, surnameBeta);

        // Generate the name
        var name = NameGenerator.Next(givenNameData, surnameData);

        // Convert name to Title Case
        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        name = textInfo.ToTitleCase(name.ToLower()); // Must call ToLower to work properly
        Console.WriteLine(name);
    }
}