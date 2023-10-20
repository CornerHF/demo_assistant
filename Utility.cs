using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Windows;
using UnManaged;
using System.Windows.Input;
using System.IO;
using System.Text.RegularExpressions;

namespace DemoAssistant
{
    public static class Utility
    {
        // Message Box
        public static void ShowYesNo(string title, string message, Action onYes, Action onNo)
        {
            var _result = MessageBox.Show(title, message, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
            if (_result == MessageBoxResult.Yes)
            {
                try
                {
                    onYes?.Invoke();
                }
                catch (Exception ex) { }
            }
            else
            {
                try
                {
                    onNo?.Invoke();
                }
                catch (Exception ex) { }
            }
        }

        public static void ShowMessage(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
        }

        public static void ShowError(string message)
        {
            MessageBox.Show(message, "錯誤", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
        }

        // ref : https://stackoverflow.com/questions/5497064/how-to-get-the-full-path-of-running-process/5497319#5497319
        public static string GetMainModuleFilepath(int processId)
        {
            string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            {
                using (var results = searcher.Get())
                {
                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                    if (mo != null)
                    {
                        return (string)mo["ExecutablePath"];
                    }
                }
            }
            return null;
        }

        public static KeyModifier ToKeyModifiers(this ModifierKeys modifierKey)
        {
            switch (modifierKey)
            {
                case ModifierKeys.None: return KeyModifier.None;
                case ModifierKeys.Alt: return KeyModifier.Alt;
                case ModifierKeys.Control: return KeyModifier.Ctrl;
                case ModifierKeys.Shift: return KeyModifier.Shift;
                case ModifierKeys.Windows: return KeyModifier.Win;
            }

            return (KeyModifier)(modifierKey);
        }

        // ref : https://stackoverflow.com/questions/3754118/how-to-filter-directory-enumeratefiles-with-multiple-criteria
        // Regex version
        // Takes same patterns, and executes in parallel
        public static IEnumerable<string> GetFiles(string path,
                            string searchPattern = "",
                            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (string.IsNullOrEmpty(searchPattern))
                return Enumerable.Empty<string>();

            var _patterns = searchPattern.Split('|');

            return _patterns.AsParallel().SelectMany(pattern =>
            {
                try
                {
                    return Directory.EnumerateFiles(path, pattern, searchOption);
                }
                catch
                {
                    return Enumerable.Empty<string>();
                }
            });
                        
        }
    }
}
