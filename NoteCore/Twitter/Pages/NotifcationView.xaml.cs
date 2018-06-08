using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using NoteCore.Annotations;

namespace NoteCore.Twitter.Pages
{
    /// <summary>
    /// Логика взаимодействия для NotifcationView.xaml
    /// </summary>
    public partial class NotifcationView : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _timer;
        private string _notificationTitle;
        private string _notificationMessage;
        public NotifcationView(string title, string message)
        {
            InitializeComponent();
            NotificationTitle = title;
            NotificationMessage = message;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(7) };
            _timer.Tick += _timer_Tick;
        }

        public string NotificationTitle
        {
            get { return _notificationTitle; }
            set
            {
                _notificationTitle = value;
                OnPropertyChanged();
            }
        }

        public string NotificationMessage
        {
            get { return _notificationMessage; }
            set
            {
                _notificationMessage = value;
                OnPropertyChanged();
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private new void Close()
        {
            _timer.Stop();
            _timer.Tick -= _timer_Tick;

            var s = (Storyboard)Resources["CloseAnim"];
            s.Begin(this);
        }

        private void ResetTimer()
        {
            _timer.Stop();
            _timer.Start();
        }

        private void TrackNotifcationView_OnSourceInitialized(object sender, EventArgs e)
        {
            Left = SystemParameters.WorkArea.Width - Width - 10;
            Top = SystemParameters.WorkArea.Height - Height - 10;

            _timer.Start();

            var s = (Storyboard)Resources["LoadAnim"];
            s.Begin(this);
        }


        void _timer_Tick(object sender, EventArgs e)
        {
            Close();
        }

        private void CloseAnim_OnCompleted(object sender, EventArgs e)
        {
            base.Close();
        }

        private void TrackNotifcationView_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
                Application.Current.MainWindow.WindowState = WindowState.Normal;

            Application.Current.MainWindow.Activate();

            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
