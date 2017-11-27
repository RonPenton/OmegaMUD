using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD.Data
{
    public class Wallet
    {
        Dictionary<string, int> _dictionary = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        public int this[string currencyName]
        {
            get
            {
                if (_dictionary.ContainsKey(currencyName))
                    return _dictionary[currencyName];
                return 0;
            }
            set
            {
                _dictionary[currencyName] = value;
            }
        }

        public Dictionary<string, int> Dictionary { get { return _dictionary; } }

        public void Clear()
        {
            foreach (var pair in _dictionary.ToList())
                _dictionary[pair.Key] = 0;
        }
    }
}
