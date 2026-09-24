using System;
using System.Collections.Generic;
using System.Text;

namespace TestData
{
    public interface ISaveLoad
    {
       string FileName { get; set; }

        bool SaveToFile();

        bool LoadFromFile();
    }
}
