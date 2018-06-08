using ProtoBuf;

namespace NoteCore
{
    /// <summary>
    /// Provide different enums
    /// </summary>
    public static class Enumerations
    {
        #region Panel Enum
        /// <summary>
        /// Defines available panels.
        /// </summary>
        public enum PageCollections
        {
            /// <summary>
            /// Note panel.
            /// </summary>
            NotePanels,
            /// <summary>
            /// Settings panel.
            /// </summary>
            SettingsPanel,
            /// <summary>
            /// Add note panel.
            /// </summary>
            AddNotePanel,
            /// <summary>
            /// Login panel.
            /// </summary>
            LoginPanel
        }
        #endregion
        #region Theme Enum
        /// <summary>
        /// Defines available themes.
        /// </summary>
        [ProtoContract]
        public enum ThemeSelections
        {
            /// <summary>
            /// Dark theme.
            /// </summary>
            [ProtoEnum]
            Dark,
            /// <summary>
            /// Light Theme
            /// </summary>
            [ProtoEnum]
            Light
        }
        #endregion
    }
}