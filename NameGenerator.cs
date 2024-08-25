using System;
using System.Collections.Generic;

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