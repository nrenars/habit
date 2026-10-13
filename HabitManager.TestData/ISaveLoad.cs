namespace TestData
{
    public interface ISaveLoad
    {
       string FileName { get; set; }

        bool SaveToFile();

        bool LoadFromFile();
    }
}
