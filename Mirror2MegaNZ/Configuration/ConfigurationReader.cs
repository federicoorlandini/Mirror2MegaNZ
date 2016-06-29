using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Mirror2MegaNZ.Configuration
{
    public class ConfigurationReader
    {
        private const string FullFilePath = @"AppData\accounts.json";
        private readonly List<Account> _accounts;

        public ConfigurationReader()
        {
            using (StreamReader r = new StreamReader(FullFilePath))
            {
                string json = r.ReadToEnd();
                _accounts = JsonConvert.DeserializeObject<List<Account>>(json);
            }
        }

        public List<Account> Accounts
        {
            get { return _accounts; }
        }
    }
}
