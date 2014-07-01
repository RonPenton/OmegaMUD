using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OmegaMUD.Parsing;

namespace OmegaMUD
{
    public partial class MajorModelEntities
    {
        public static MajorModelEntities Load(string filename)
        {
            return new MajorModelEntities(String.Format("metadata=res://*/MajorModel.csdl|res://*/MajorModel.ssdl|res://*/MajorModel.msl;provider=System.Data.SqlServerCe.4.0;provider connection string=\"Data Source={0}\"", filename));
        }

        Dictionary<string, Regex> _regularExpressions = new Dictionary<string, Regex>();
        Dictionary<string, string> _settings = new Dictionary<string, string>();
        Dictionary<string, string[]> _arrays = new Dictionary<string, string[]>();
        private Regex GetRegex(string name)
        {
            Regex regex;
            if (!_regularExpressions.TryGetValue(name, out regex))
            {
                regex = _regularExpressions[name] = new Regex(Settings.Single(x => x.Key == name).Value);
            }
            return regex;
        }
        private string GetSetting(string name)
        {
            string setting;
            if (!_settings.TryGetValue(name, out setting))
            {
                setting = _settings[name] = Settings.Single(x => x.Key == name).Value;
            }
            return setting;
        }
        private string[] GetArray(string name)
        {
            string[] array;
            if (!_arrays.TryGetValue(name, out array))
            {
                array = _arrays[name] = Settings.Single(x => x.Key == name).Value.Split('|');
            }
            return array;
        }

        public Regex DragRegex { get { return GetRegex("DragRegex"); } }
        public Regex FollowRegex { get { return GetRegex("FollowString"); } }

        public string DarkString { get { return GetSetting("DarkString"); } }
        public string ExitString { get { return GetSetting("ExitString"); } }
        public string RoomExitString { get { return GetSetting("RoomExitString"); } }
        public string RoomItemString { get { return GetSetting("RoomItemString"); } }
        public string RoomPeopleString { get { return GetSetting("RoomPeopleString"); } }

        public Regex RoomItemRegex { get { return GetRegex("RoomItemRegex"); } }
        public Regex RoomPeopleRegex { get { return GetRegex("RoomPeopleRegex"); } }
        public Regex RoomExitRegex { get { return GetRegex("RoomExitRegex"); } }

        TokenSequence _mudEnterSequence;
        public TokenSequence MudEnterSequence
        {
            get
            {
                if (_mudEnterSequence == null)
                    _mudEnterSequence = new TokenSequence(this.Settings.Single(x => x.Key == "EnterSequence").Value);
                return _mudEnterSequence;
            }
        }

        public string[] PlainExitNames { get { return GetArray("PlainExitNames"); } }
        public string[] SpecialExitNames { get { return GetArray("SpecialExitNames"); } }
        public string[] BarrierStateNames { get { return GetArray("BarrierStateNames"); } }
        public string[] DoorNames { get { return GetArray("DoorNames"); } }
        public string[] GateNames { get { return GetArray("GateNames"); } }
        public string SecretPassageText { get { return GetSetting("SecretPassageText"); } }
        public string NoExitsText { get { return GetSetting("NoExitsText"); } }
        public int TollMultiplier { get { return Int32.Parse(GetSetting("TollMultiplier")); } }
        Dictionary<string, int> _currencies;
        public Dictionary<string, int> Currencies
        {
            get
            {
                if (_currencies == null)
                {
                    _currencies = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
                    var names = this.Settings.Single(x => x.Key == "CurrencyNames").Value.Split('|');
                    var values = this.Settings.Single(x => x.Key == "CurrencyValues").Value.Split('|');
                    for (int i = 0; i < names.Length; i++)
                    {
                        _currencies[names[i]] = Int32.Parse(values[i]);
                    }
                }
                return _currencies;
            }
        }

        public string InventoryString { get { return GetSetting("InventoryString"); } }
        public Regex InventoryRegex { get { return GetRegex("InventoryRegex"); } }
        public Regex InventoryKeyRegex { get { return GetRegex("InventoryKeyRegex"); } }
        public Regex InventoryWeightRegex { get { return GetRegex("InventoryWeightRegex"); } }
        public string InventoryNoItemString { get { return GetSetting("InventoryNoItemString"); } }
        public string InventoryNoKeyString { get { return GetSetting("InventoryNoKeyString"); } }

        public Regex StatRegex { get { return GetRegex("StatRegex"); } }

        public string ExperienceStartString { get { return GetSetting("ExperienceStartString"); } }
        public Regex ExperienceRegex { get { return GetRegex("ExperienceRegex"); } }

        public Regex BashSuccessRegex { get { return GetRegex("BashSuccessRegex"); } }
        public Regex BashFailRegex { get { return GetRegex("BashFailRegex"); } }
        public Regex PickSuccessRegex { get { return GetRegex("PickSuccessRegex"); } }
        public Regex PickFailRegex { get { return GetRegex("PickFailRegex"); } }
        public Regex KeySuccessRegex { get { return GetRegex("KeySuccessRegex"); } }
        public Regex KeyFailRegex { get { return GetRegex("KeyFailRegex"); } }
        public string UseKeyCommandString { get { return GetSetting("UseKeyCommandString"); } }
        public string BashCommandString { get { return GetSetting("BashCommandString"); } }
        public string PickCommandString { get { return GetSetting("PickCommandString"); } }
        public string OpenCommandString { get { return GetSetting("OpenCommandString"); } }
        public Regex DoorOpenRegex { get { return GetRegex("DoorOpenRegex"); } }        
    }
}
