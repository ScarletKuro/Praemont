using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Command;
using NoteCore.Model;
using NoteCore.Repositories;
using NoteCore.Twitter.Timeline;
using NoteCore.Utils;

namespace NoteCore.Service
{
    public class TwitterService
    {
        private TweetTimeline _timeline;
        private static Popup _popup;
        private readonly IOptionRepository _config;

        public RelayCommand<string> ShowUserInformationCommand { get; private set; }
        public RelayCommand<Tweet> OpenTweetLinkCommand { get; private set; }
        public RelayCommand<string> ImageViewCommand { get; private set; }
        public RelayCommand<Tweet> ReplyCommand { get; private set; }
        public RelayCommand<Tweet> RetweetCommand { get; private set; }
        public RelayCommand<Tweet> DeleteTweetCommand { get; private set; }
        public RelayCommand<Tweet> FavoritesCommand { get; private set; }
        public RelayCommand<Tweet> CopyCommand { get; private set; }
        public RelayCommand<View> SwitchTimelinesCommand { get; private set; }

        public TwitterService(IOptionRepository conf)
        {
            TweetTimeline timeline = new TweetTimeline();
            _timeline = timeline;
            _config = conf;
            ShowUserInformationCommand = new RelayCommand<string>(OnShowUserInformation);
            OpenTweetLinkCommand = new RelayCommand<Tweet>(OnOpenTweetLink);
            ImageViewCommand = new RelayCommand<string>(OnImageView);
            ReplyCommand = new RelayCommand<Tweet>(OnReply);
            RetweetCommand = new RelayCommand<Tweet>(OnRetweet);
            DeleteTweetCommand = new RelayCommand<Tweet>(OnDeleteTweet);
            FavoritesCommand = new RelayCommand<Tweet>(OnLikeTweet);
            CopyCommand = new RelayCommand<Tweet>(OnCopy);
            SwitchTimelinesCommand = new RelayCommand<View>(OnSwitchTimelines);
        }

        private void OnSwitchTimelines(View view)
        {
            _timeline.GeTimelines.SwitchView(view);
        }

        public void ResetTimeline()
        {
            _timeline = new TweetTimeline();
        }

        public Timelines Timeline { get { return _timeline.GeTimelines; } }

        private void OnCopy(Tweet tweet)
        {
            TimelineController.CopyTweetToClipboard(tweet);
        }

        private void OnLikeTweet(Tweet tweet)
        {
            if (tweet.IsFavorite)
                _timeline.Controller.RemoveFavorite(tweet);
            else
                _timeline.Controller.AddFavorite(tweet);
        }

        private void OnDeleteTweet(Tweet tweet)
        {
            _timeline.Controller.DeleteTweet(tweet);
        }

        private void OnRetweet(Tweet tweet)
        {
            _timeline.Controller.Retweet(tweet);
        }

        private void OnReply(Tweet tweet)
        {
            var mainWindow = (IMainWindow)Application.Current.MainWindow;
            mainWindow.OnReply(tweet, _config.GetConfig.Tokens.ScreenName);
        }

        private void OnImageView(string obj)
        {
            if (_popup != null) _popup.IsOpen = false;
            _popup = CreatePopup(Application.Current.MainWindow, obj);
        }

        private void OnOpenTweetLink(Tweet tweet)
        {
            var link = TimelineController.TweetLink(tweet);
            Process.Start(new ProcessStartInfo(link));
        }

        private void OnShowUserInformation(string name)
        {
            var mainWindow = (IMainWindow)Application.Current.MainWindow;
            mainWindow.ShowUserInfo(name);
        }

        private static Popup CreatePopup(Window window, string url)
        {
            var popup = new Popup
            {
                AllowsTransparency = true,
                Child = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(2),
                    Child = new Image
                    {
                        Source = new BitmapImage(new Uri(url)),
                        Stretch = Stretch.UniformToFill
                    }
                },
                Placement = PlacementMode.Center,
                PlacementRectangle = Screen.ScreenRectFromWindow(window),
                PopupAnimation = PopupAnimation.Fade
            };

            popup.KeyDown += (o, args) => popup.IsOpen = false;
            popup.MouseDown += (o, args) => popup.IsOpen = false;
            popup.IsOpen = true;
            return popup;
        }

        public RangeObservableCollection<Tweet> Tweets { get { return _timeline.GeTimelines.Timeline; } }
    }
}
