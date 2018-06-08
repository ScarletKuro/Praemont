using System.Diagnostics;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NoteCore.Model;
using NoteCore.Repositories;
using NoteCore.Service;
using NoteCore.Twitter.Model;
using NoteCore.Twitter.Pages;

namespace NoteCore.ViewModel
{
    public class AuthenticateViewModel
        : ViewModelBase
    {
        private OAuthTokens _tokens;
        private readonly IOptionRepository _optionRepository;
        private string _pingtext;
        private TwitterService _twitterService;

        public RelayCommand GetPinCommand { get; private set; }
        public RelayCommand SignInComannd { get; private set; }

        public AuthenticateViewModel(IOptionRepository conf, TwitterService twitterService)
        {

            _twitterService = twitterService;
            _optionRepository = conf;
            GetPinCommand = new RelayCommand(OnGetPin);
            SignInComannd = new RelayCommand(OnSignIn);
        }


        private async void OnGetPin()
        {
            //_twitteService.GetRequestToken();
            //GetConfig.Tokens = new OAuthTokens();
            GetConfig.Tokens = await Service.Twitter.GetRequestToken();
            var url = "https://api.twitter.com/oauth/authenticate?oauth_token=" + GetConfig.Tokens.OAuthToken;
            Process.Start(url);
        }


        private async void OnSignIn()
        {
            var tokens = await Service.Twitter.GetAccessToken(GetConfig.Tokens.OAuthToken, GetConfig.Tokens.OAuthSecret, PinText);
            if (tokens != null)
            {
                GetConfig.Tokens.OAuthSecret = tokens.OAuthSecret;
                GetConfig.Tokens.OAuthToken = tokens.OAuthToken;
                GetConfig.Tokens.ScreenName = tokens.ScreenName;
                GetConfig.Tokens.UserId = tokens.UserId;
                _optionRepository.Save();
                Switcher.ChangeViewCollection(Enumerations.PageCollections.NotePanels);
                _twitterService.ResetTimeline();
            }
            else
            {
                //var notificationView = Application.Current.Windows.OfType<NotifcationView>().FirstOrDefault();
                var notificationView = new NotifcationView("Error", "There was error during login");
                notificationView.Show();
            }
        }

        public string PinText
        {
            get { return _pingtext; }
            set
            {
                _pingtext = value;
                RaisePropertyChanged(()=> PinText);
            }
        }

        public Options GetConfig
        {
            get { return _optionRepository.GetConfig; }
        }
    }
}
