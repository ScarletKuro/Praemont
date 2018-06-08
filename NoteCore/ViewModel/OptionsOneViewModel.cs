using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NoteCore.Model;
using NoteCore.Repositories;
using NoteCore.Service;
using NoteCore.Twitter.Model;

namespace NoteCore.ViewModel
{
    public sealed class OptionsOneViewModel
        : ViewModelBase
    {
        private readonly IOptionRepository _optionRepository;
        private TwitterService _twitterService;
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand DelfaultCommand { get; private set; }
        public RelayCommand ResetCommand { get; private set; }
        public RelayCommand LogOutCommand { get; private set; }
        public OptionsOneViewModel(IOptionRepository conf, TwitterService twitterService)
        {
            SaveCommand = new RelayCommand(OnSave);
            ResetCommand = new RelayCommand(ResetApplication);
            DelfaultCommand = new RelayCommand(OnDelfault);
            LogOutCommand = new RelayCommand(OnLogOut);
            _optionRepository = conf;
            _twitterService = twitterService;
        }

        private void OnLogOut()
        {
            _optionRepository.GetConfig.Tokens = new OAuthTokens();
            _optionRepository.Save();
            Switcher.ChangeViewCollection(Enumerations.PageCollections.LoginPanel);
            _twitterService.ResetTimeline();
        }

        private  void OnSave()
        {
            _optionRepository.Save();
        }
        private void OnDelfault()
        {
            _optionRepository.GetConfig.IsShowInTaskbar = false;
            _optionRepository.GetConfig.IsStartUp = false;
            _optionRepository.GetConfig.IsTopMost = false;
            _optionRepository.Save();
        }

        private void ResetApplication()
        {
            _optionRepository.RepositoryReset();
        }
        public Options GetConfig
        {
            get { return _optionRepository.GetConfig; }
        }
    }
}
