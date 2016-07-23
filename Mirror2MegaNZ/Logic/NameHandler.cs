using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Mirror2MegaNZ.Logic
{
    public static class NameHandler
    {
        public static void ExtractFilenameAndDateTimeFromRemoteFilename(string remoteFilename, out string extractedFilename, out DateTime extractedDateTime)
        {
            var regex = new Regex(@"^(.*)_\[\[(\d{4})-(\d{0,2})-(\d{0,2})-(\d{0,2})-(\d{0,2})-(\d{0,2})\]\](\..*)$");
            var matches = regex.Matches(remoteFilename);
            var match = matches[0];

            var filenameWithoutExtension = match.Groups[1].Value;

            var year = int.Parse(match.Groups[2].Value);
            var month = int.Parse(match.Groups[3].Value);
            var day = int.Parse(match.Groups[4].Value);
            var hour = int.Parse(match.Groups[5].Value);
            var minute = int.Parse(match.Groups[6].Value);
            var second = int.Parse(match.Groups[7].Value);

            var fileExtensionWithDot = match.Groups[8].Value;

            extractedFilename = filenameWithoutExtension + fileExtensionWithDot;
            extractedDateTime = new DateTime(year, month, day, hour, minute, second);
        }

        public static string BuildRemoteFileName(string name, DateTime lastModificationDate)
        {
            // The remote name cannot be the same as the local file because
            // we cannot add metadata to the remote file to store the local last modification datetime.
            // For this reason, I add the local last modification datetime in the name of the file
            // that is created in remote.
            // The structure of the name is:
            //      <filename>_[[<year>-<month>-<day>-<hour>-<minute>-<second>]]
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(name);
            var extension = Path.GetExtension(name);
            var remoteFilename = string.Format("{0}_[[{1}-{2}-{3}-{4}-{5}-{6}]]",
                nameWithoutExtension.Trim(),
                lastModificationDate.Year,
                lastModificationDate.Month,
                lastModificationDate.Day,
                lastModificationDate.Hour,
                lastModificationDate.Minute,
                lastModificationDate.Second);

            if (!string.IsNullOrEmpty(extension))
            {
                return remoteFilename + extension;
            }
            else
            {
                return remoteFilename;
            }
        }

        /// <summary>
        /// Build the folder for the uploaded folder in the remote repository
        /// </summary>
        /// <param name="localFolderName">The name of the local folder</param>
        /// <returns></returns>
        public static string BuildRemoteFolderName(string localFolderName)
        {
            return localFolderName;
        }
    }
}
