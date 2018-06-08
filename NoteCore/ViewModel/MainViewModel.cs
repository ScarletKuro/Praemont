using System.Diagnostics;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NoteCore.Model;
using NoteCore.Repositories;
using NoteCore.Service;
using NoteCore.Twitter.Pages;

namespace NoteCore.ViewModel
{
    /// <summary>
    /// This is the VM for the Main Window.
    /// This class contains properties that the View can data bind to.
    /// </summary>
    public sealed class MainViewModel 
        : ViewModelBase
    {
        private readonly IOptionRepository _optionRepository;
        private TwitterService _twitterService;
        public RelayCommand SlideNextCommand { get; private set; }
        public RelayCommand SlideBackCommand { get; private set; }
        public RelayCommand SaveOptionsCommand { get; private set; }
        public RelayCommand PreferencesCommand { get; private set; }
        public RelayCommand WebCommand { get; private set; }
        public RelayCommand LoginCommand { get; private set; }
        public RelayCommand AddNoteCommand { get; private set; }
        public RelayCommand SettingComannd { get; private set; }
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IOptionRepository conf, TwitterService twitterService, INavigationService navigationService)
        {
            SlideNextCommand = new RelayCommand(OnSlideNext);
            SlideBackCommand = new RelayCommand(OnSlideBack);
            PreferencesCommand = new RelayCommand(OnPreferences);
            SaveOptionsCommand = new RelayCommand(OnSave);
            LoginCommand = new RelayCommand(OnLogin);
            AddNoteCommand = new RelayCommand(OnAddNote);
            SettingComannd = new RelayCommand(OnSetting);
            WebCommand = new RelayCommand(OnWeb);
            _optionRepository = conf;
            _twitterService = twitterService;
            switch (_optionRepository.GetConfig.Theme)
            {
                case Enumerations.ThemeSelections.Light:
                    FirstFloor.ModernUI.Presentation.AppearanceManager.Current.ThemeSource = FirstFloor.ModernUI.Presentation.AppearanceManager.LightThemeSource;
                    break;
                case Enumerations.ThemeSelections.Dark:
                    FirstFloor.ModernUI.Presentation.AppearanceManager.Current.ThemeSource = FirstFloor.ModernUI.Presentation.AppearanceManager.DarkThemeSource;
                    break;
            }
        }

        private void OnWeb()
        {
            var link = string.Format("http://twitter.com/{0}", _optionRepository.GetConfig.Tokens.ScreenName);
            Process.Start(new ProcessStartInfo(link));
        }

        /// <summary>
        /// Gets the option
        /// </summary>
        public Options GetConfig
        {
            get { return _optionRepository.GetConfig; }
        }

        private void OnSave()
        {
            _optionRepository.Save();
        }
        /// <summary>
        /// Handle the settings command action
        /// </summary>
        private void OnSetting()
        {
            Switcher.ChangeViewCollection(Enumerations.PageCollections.SettingsPanel);
        }
        /// <summary>
        /// Handle the add note command action
        /// </summary>
        private void OnAddNote()
        {
            Switcher.ChangeViewCollection(Enumerations.PageCollections.AddNotePanel);
        }
        /// <summary>
        /// Handle the login command action
        /// </summary>
        private void OnLogin()
        {
            Switcher.ChangeViewCollection(Enumerations.PageCollections.LoginPanel);
        }
        /// <summary>
        /// Handle the slide back command action
        /// </summary>
        private void OnPreferences()
        {
            Switcher.ChangeViewCollection(Enumerations.PageCollections.NotePanels);
            _twitterService.Timeline.SwitchView(View.Home);
        }
        /// <summary>
        /// Handle the slide back command action
        /// </summary>
        private void OnSlideBack()
        {
            Switcher.SlideBack();
        }
        /// <summary>
        /// Handle the slide next command action
        /// </summary>
        private void OnSlideNext()
        {
            Switcher.SlideNext();
        }
    }
}
