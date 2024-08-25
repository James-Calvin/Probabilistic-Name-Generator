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