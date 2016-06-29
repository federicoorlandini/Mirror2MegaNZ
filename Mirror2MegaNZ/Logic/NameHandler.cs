using System;
using System.IO;

namespace Mirror2MegaNZ.Logic
{
    public static class NameHandler
    {
        public static DateTime ExtractDateTimeFromName(string name)
        {
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(name).TrimEnd('.');
            var separatorPosition = nameWithoutExtension.LastIndexOf('-');
            var dateTimeString = nameWithoutExtension.Substring(separatorPosition + 1).Trim();
            var item = dateTimeString.Split('_');

            var year = int.Parse(item[0]);
            var month = int.Parse(item[1]);
            var day = int.Parse(item[2]);
            var hour = int.Parse(item[3]);
            var minute = int.Parse(item[4]);
            var second = int.Parse(item[5]);

            return new DateTime(year, month, day, hour, minute, second);
        }

        public static string BuildRemoteFileName(string name, DateTime lastModificationDate)
        {
            // The remote name cannot be the same as the local file because
            // we cannot add metadata to the remote file to store the local last modification datetime.
            // For this reason, I add the local last modification datetime in the name of the file
            // that is created in remote.
            // The structure of the name is:
            //      <filename>-[year]_[month]_[day]_[hour]_[minute]_[second]
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(name);
            var extension = Path.GetExtension(name);
            var remoteFilename = string.Format("{0}-{1}_{2}_{3}_{4}_{5}_{6}",
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
