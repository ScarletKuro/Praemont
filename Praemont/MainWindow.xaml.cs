using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using NoteCore;
using NoteCore.Feature.StickyWindow;
using NoteCore.Model;
using NoteCore.ViewModel;

namespace Praemont
{
    /// <summary>
    ///     Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IMainWindow

    {
        private StickyWindow _stickyWindow;

        public MainWindow()
        {
            InitializeComponent();
            Title = "Twitter Client";
            Loaded += Window1Loaded;
            Closing += (s, e) => ViewModelLocator.Cleanup();

        }

        private void Window1Loaded(object sender, RoutedEventArgs e)
        {
            _stickyWindow = new StickyWindow(this) {StickToScreen = true, StickOnResize = true, StickOnMove = true};
            Compose.Visibility = Visibility.Collapsed;
            SizeChanged += OnSizeChanged;
            LoadedForm = true;
            Messenger.Default.Send<IMainWindow>(this);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }

        private void ComposeOnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            OnRenderSizeChanged(new SizeChangedInfo(this, new Size(Width, Height), true, true));
            if (Compose.IsVisible) Compose.Focus();
        }

        private void OnCreateTweet(object sender, RoutedEventArgs e)
        {
            Compose.Visibility = Compose.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        public bool LoadedForm { get; set; }

        public void ShowUserInfo(string name)
        {
            UserInformationPopup.ScreenName = name;
            UserInformationPopup.IsOpen = true;
        }

        public void OnReply(Tweet tweet, string screenName)
        {
            if (tweet == null) return;
            if (tweet.IsDirectMessage)
            {
               Compose.ShowDirectMessage(tweet.ScreenName);
            }
            else
            {
                var matches = new Regex(@"(?<=^|(?<=[^a-zA-Z0-9-_\.]))@([A-Za-z]+[A-Za-z0-9_]+)")
                    .Matches(tweet.Text);

                var names = matches
                    .Cast<Match>()
                    .Where(m => m.Groups[1].Value != tweet.ScreenName)
                    .Where(m => m.Groups[1].Value != screenName)
                    .Select(m => "@" + m.Groups[1].Value)
                    .Distinct();

                var replyTos = string.Join(" ", names);
                var message = string.Format("@{0} {1}{2}", tweet.ScreenName, replyTos, (replyTos.Length > 0) ? " " : "");
                Compose.Show(message, tweet.StatusId);
            }
        }
    }
}