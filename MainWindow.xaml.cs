using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;

namespace DemoAssistant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenu notifyMenu;

        private Setting setting;

        private UnManaged.HotKey _restartGameHotKey;

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        // Init ->
        private void Init()
        {
            // Init Notify Icon and Menu
            {
                notifyIcon = new System.Windows.Forms.NotifyIcon();
                notifyIcon.Text = "Demo助手";

                notifyMenu = new System.Windows.Forms.ContextMenu();
                notifyMenu.MenuItems.Add("退出", (sender, args) => Close());

                notifyIcon.ContextMenu = notifyMenu;

                using (var stream = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/DA.ico")).Stream)
                {
                    notifyIcon.Icon = new System.Drawing.Icon(stream);
                }

                notifyIcon.Visible = true;

                notifyIcon.DoubleClick += (sender, args) =>
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
            }

            // debug init exe location
            {
                setting = Setting.Load(AppDomain.CurrentDomain.BaseDirectory + @"da_setting.json");

                this.DataContext = setting; // set setting as data context
            }

            // Init Hot Key
            {
                RebindHotKey();
                
            }
        }


        // Event ->
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            OnClose();
        }


        private void OnRestartGameHotKeyHandler(UnManaged.HotKey hotkey)
        {
            if (setting.AnyChange)
            {
                Utility.ShowMessage("提示", "你有未保存的設定，你需要保存當前的設定才能繼續重啟遊戲。");
                return;
            }


            var _msg = "要重啟遊戲嗎？";

            if (setting.RemoveSaveFolderContent && !string.IsNullOrEmpty(setting.SaveFilePattern))
            {
                var _files = setting.GetMatchedSaveFileFullNames();

                _msg = $"要刪除存檔並重啟遊戲嗎？\n存檔路徑 : 「{setting.SaveFolderPath}」\n存檔格式 : 「{setting.SaveFilePattern}」\n存檔數 : {_files.Length}";
            }

            Utility.ShowYesNo(_msg, "提問", () =>
            {
                ProcessRestartDemoGame();
            }, null);
        }


        private void button_browseGameExePath_Click(object sender, RoutedEventArgs e)
        {
            var _dialog = new Microsoft.Win32.OpenFileDialog();
            _dialog.InitialDirectory = System.IO.Path.GetDirectoryName(setting.GameExePath);

            var _result = _dialog.ShowDialog();
            if (_result == true)
            {
                setting.GameExePath = _dialog.FileName;
            }
        }

        private void button_browseSaveFolderPath_Click(object sender, RoutedEventArgs e)
        {
            var _dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            _dialog.SelectedPath = setting.SaveFolderPath;

            var _result = _dialog.ShowDialog();
            if (_result == true)
            {
                setting.SaveFolderPath = _dialog.SelectedPath;
            }
        }

        private void button_checkFileDetail_Click(object sender, RoutedEventArgs e)
        {
            var _window = new TextBoxWindow();

            _window.Show();

            var _fileList = setting.GetMatchedSaveFileFullNames().ToList();
            _window.TextList = _fileList;
            _window.OnDoubleClickText += (fileName) =>
            {
                if (File.Exists(fileName))
                {
                    var _dirName = System.IO.Path.GetDirectoryName(fileName);
                    Process.Start("explorer.exe", _dirName);
                }
            };
        }

        // ref : https://tyrrrz.me/blog/hotkey-editor-control-in-wpf
        private void textBox_shortCut_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Don't let the event pass further because we don't want
            // standard textbox shortcuts to work.
            e.Handled = true;

            // Get modifiers and key data
            var modifiers = Keyboard.Modifiers;
            var key = e.Key;

            // When Alt is pressed, SystemKey is used instead
            if (key == Key.System)
            {
                key = e.SystemKey;
            }

            // Pressing delete, backspace or escape without modifiers clears the current value
            if (modifiers == ModifierKeys.None &&
                (key == Key.Delete || key == Key.Back || key == Key.Escape))
            {
                //Hotkey = null;
                return;
            }

            // If no actual key was pressed - return
            if (key == Key.LeftCtrl ||
                key == Key.RightCtrl ||
                key == Key.LeftAlt ||
                key == Key.RightAlt ||
                key == Key.LeftShift ||
                key == Key.RightShift ||
                key == Key.LWin ||
                key == Key.RWin ||
                key == Key.Clear ||
                key == Key.OemClear ||
                key == Key.Apps)
            {
                return;
            }

            //
            setting.ShortCutKey = key;
            setting.ShortCutModifierKey = modifiers;


            Keyboard.ClearFocus();
        }


        private void button_clearShortCut_Click(object sender, RoutedEventArgs e)
        {
            setting.ShortCutKey = Key.None;
            setting.ShortCutModifierKey = ModifierKeys.None;

            Keyboard.ClearFocus();
        }

        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            setting.Save();

            RebindHotKey();
        }

        private void button_restore_Click(object sender, RoutedEventArgs e)
        {
            setting = Setting.Load(setting.SettingPath);
            this.DataContext = setting;
        }

        // Func ->
        private void OnClose()
        {
            if (notifyIcon != null)
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
            }

            if (_restartGameHotKey != null)
            {
                _restartGameHotKey.Unregister();
                _restartGameHotKey.Dispose();
            }
        }

        public void RebindHotKey()
        {
            if (_restartGameHotKey != null)
            {
                _restartGameHotKey.Unregister();
                _restartGameHotKey.Dispose();
                _restartGameHotKey = null;
            }

            if (setting.ShortCutKey != Key.None)
            {
                _restartGameHotKey = new UnManaged.HotKey(setting.ShortCutKey, setting.ShortCutModifierKey.ToKeyModifiers(), OnRestartGameHotKeyHandler);
            }
        }

        public void ProcessRestartDemoGame()
        {
            try
            {
                var _gameExePath = Environment.ExpandEnvironmentVariables(setting.GameExePath);
                var _targetProcessName = System.IO.Path.GetFileNameWithoutExtension(_gameExePath);

                // 1. remove exist exe
                var _processList = Process.GetProcesses();
                foreach (var process in _processList)
                {
                    var _currentGameExeFullPath = _gameExePath;
                    if (System.IO.File.Exists(_currentGameExeFullPath))
                    {
                        _currentGameExeFullPath = System.IO.Path.GetFullPath(_gameExePath);
                    }

                    if (_targetProcessName == process.ProcessName)
                    {
                        // double check full path
                        var _fullName = Utility.GetMainModuleFilepath(process.Id);
                        if (_fullName == _currentGameExeFullPath)
                        {
                            process.Kill();
                            process.WaitForExit();
                        }
                    }
                }

                // 2. remove save file (without remove save folder)
                if (setting.RemoveSaveFolderContent)
                {
                    var _fileNames = setting.GetMatchedSaveFileFullNames();

                    foreach(var fullName in _fileNames)
                    {
                        File.Delete(fullName);
                    }
                }

                // 3. restart exe
                System.Diagnostics.Process.Start(_gameExePath);
            }
            catch(Exception ex)
            {
                Utility.ShowError(ex.Message);
            }
        }

    }
}
