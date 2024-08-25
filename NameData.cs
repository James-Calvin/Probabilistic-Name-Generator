using System;
using System.IO;
using System.Collections.Generic;

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