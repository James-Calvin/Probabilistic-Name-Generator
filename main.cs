using System;
using System.Collections.Generic;
using System.Globalization;


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
    const string SURNAMES_Hispanic = "DATA/Surnames/Surnames_Hispanic.csv";
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
                return SURNAMES_Hispanic;
            case "white":
                return SURNAMES_WHITE;

            default:
                return SURNAMES_ALL;
        }
    }

    public static void Main(string[] args)
    {
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
        var givenNameData = NameData.Load(givenNameDataPath, givenNameBeta);
        var surnameData = NameData.Load(surnameDataPath, surnameBeta);

        // Generate the name
        var name = NameGenerator.Next(givenNameData, surnameData);

        // Convert name to Title Case
        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        name = textInfo.ToTitleCase(name.ToLower()); // Must call ToLower to work properly
        Console.WriteLine(name);
    }
}