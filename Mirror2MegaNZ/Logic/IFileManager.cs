using System.IO;

namespace Mirror2MegaNZ.Logic
{
    public interface IFileManager
    {
        FileStream GetStreamToReadFile(string filePath);
    }
}