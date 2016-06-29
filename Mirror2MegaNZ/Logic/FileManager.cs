using System.IO;

namespace Mirror2MegaNZ.Logic
{
    /// <summary>
    /// Class to handle the file
    /// </summary>
    public class FileManager : IFileManager
    {
        /// <summary>
        /// Return a stream to read a file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public FileStream GetStreamToReadFile(string filePath)
        {
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }
    }
}
