using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Input;
using Newtonsoft.Json;
using System.ComponentModel;

namespace DemoAssistant
{
    public class Setting : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _settingPath = null;
        private bool anyChange = false;

        [JsonProperty]
        private string gameExePath;
        [JsonIgnore]
        public string GameExePath
        {
            get { return gameExePath; }
            set 
            {
                gameExePath = value;
                AnyChange = true;
                OnPropertyChanged("gameExePath");
            }
        }

        [JsonProperty]
        private string saveFolderPath;
        [JsonIgnore]
        public string SaveFolderPath
        {
            get 
            {
                return saveFolderPath; 
            }
            set 
            {
                saveFolderPath = value;
                AnyChange = true;
                OnPropertyChanged("saveFolderPath");
                OnPropertyChanged("MatchedSaveFileCount");
            }
        }

        [JsonProperty]
        private string saveFilePattern;
        [JsonIgnore]
        public string SaveFilePattern
        {
            get { return saveFilePattern; }
            set
            {
                saveFilePattern = value;
                AnyChange = true;
                OnPropertyChanged("saveFilePattern");
                OnPropertyChanged("MatchedSaveFileCount");
            }
        }

        [JsonProperty]
        public string MatchedSaveFileCount
        {
            get
            {
                var _fileNames = GetMatchedSaveFileFullNames();
                return _fileNames.Length.ToString();
            }
        }

        [JsonProperty]
        private bool removeSaveFolderContent;
        [JsonIgnore]
        public bool RemoveSaveFolderContent
        {
            get { return removeSaveFolderContent; }
            set
            {
                removeSaveFolderContent = value;
                AnyChange = true;
                OnPropertyChanged("removeSaveFolderContent");
            }
        }

        [JsonProperty]
        private Key shortCutKey = Key.F9; // default
        [JsonIgnore]
        public Key ShortCutKey
        {
            get { return shortCutKey; }
            set 
            {
                shortCutKey = value;
                AnyChange = true;
                OnPropertyChanged("shortCutKey");
                OnPropertyChanged("ShortCutFullName");
            }
        }

        [JsonProperty]
        private ModifierKeys shortCutModifierKey = ModifierKeys.Control; // default
        [JsonIgnore]
        public ModifierKeys ShortCutModifierKey
        {
            get { return shortCutModifierKey; }
            set 
            {
                shortCutModifierKey = value;
                AnyChange = true;
                OnPropertyChanged("shortCutModifierKey");
                OnPropertyChanged("ShortCutFullName");
            }
        }

        [JsonIgnore]
        public string ShortCutFullName
        {
            get
            {
                if (ShortCutModifierKey != ModifierKeys.None)
                {
                    return ShortCutModifierKey + " + " + ShortCutKey;
                }
                else if (ShortCutKey != Key.None)
                {
                    return ShortCutKey.ToString();
                }
                else
                {
                    return "";
                }
            }
        }

        [JsonIgnore]
        public string SettingPath => _settingPath;

        [JsonIgnore]
        public bool AnyChange
        {
            get { return anyChange; }
            private set
            {
                anyChange = value;
                OnPropertyChanged("AnyChange");
            }
        }




        public Setting()
        {
            
        }

        public void Save()
        {
            try
            {
                var _json = JsonConvert.SerializeObject(this);
                File.WriteAllText(_settingPath, _json);

                AnyChange = false;
            }
            catch(Exception ex)
            {
                Utility.ShowError(ex.Message);
            }
        }

        public static Setting Load(string filePath)
        {
            Setting _setting = null;

            if (File.Exists(filePath))
            {
                try
                {
                    var _json = File.ReadAllText(filePath);
                    _setting = JsonConvert.DeserializeObject<Setting>(_json);
                }
                catch (Exception ex)
                {
                    Utility.ShowError(ex.Message);
                }
            }

            if (_setting == null)
            {
                _setting = new Setting();
            }

            _setting._settingPath = filePath;
            return _setting;
        }


        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //
        public string[] GetMatchedSaveFileFullNames()
        {
            var _realSaveFolderPath = Environment.ExpandEnvironmentVariables(SaveFolderPath);

            if (!Directory.Exists(_realSaveFolderPath))
                return new string[0];

            if (string.IsNullOrEmpty(SaveFilePattern))
                return new string[0];

            var _files = Utility.GetFiles(_realSaveFolderPath, SaveFilePattern, SearchOption.AllDirectories);
            var _fullNameArray = _files.ToArray();

            return _fullNameArray;
        }

    }
}
