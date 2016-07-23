namespace Mirror2MegaNZ.Logic
{
    static class FileSizeFormatter
    {
        private const int OneKiloByte = 1024;
        private const int OneMegaByte = 1024 * 1024;
        private const int OneGigaByte = 1024 * 1024 * 1024;

        public static string Format(long sizeInByte)
        {
            if(sizeInByte < OneKiloByte )
            {
                return string.Format("{0} bytes", sizeInByte);
            }
            else if(sizeInByte >= OneKiloByte && sizeInByte < OneMegaByte)
            {
                var size = (double)sizeInByte / OneKiloByte;
                return string.Format("{0} KBytes", size.ToString("F2"));
            }
            else if(sizeInByte >= OneMegaByte && sizeInByte <= OneGigaByte)
            {
                var size = (double)sizeInByte / OneMegaByte;
                return string.Format("{0} MBytes", size.ToString("F2"));
            }
            else
            {
                var size = (double)sizeInByte / OneGigaByte;
                return string.Format("{0} GBytes", size.ToString("F2"));
            }
        }
    }
}
