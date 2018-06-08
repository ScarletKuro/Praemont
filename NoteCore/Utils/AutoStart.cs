using System.Reflection;
using Microsoft.Win32;

namespace NoteCore.Utils
{
    public static class AutoStart
    {
        private const string RunLocation = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "Praemont";
        /// <summary>
        /// Set or unsets the autostart value for the assembly.
        /// </summary>
        public static void SetAutoStart(bool value)
        {
            var key = Registry.CurrentUser.CreateSubKey(RunLocation);
            if (value)
            {
                if (key != null) key.SetValue(ValueName, Assembly.GetEntryAssembly().Location);
            }
            else
            {
                    if (key != null && IsAutoStartEnabled)
                    {
                        key.DeleteValue(ValueName);
                    }
            }
        }

        /// <summary>
        /// Returns whether auto start is enabled.
        /// </summary>
        public static bool IsAutoStartEnabled
        {
            get
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(RunLocation);
                if (key == null)
                    return false;

                var value = (string)key.GetValue(ValueName);
                if (value == null)
                    return false;
                return (value == Assembly.GetEntryAssembly().Location);
            }
        }
    }
}
