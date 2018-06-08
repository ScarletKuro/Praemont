using System.Linq;
using System.Windows;

namespace Praemont.Pages.NotePanels
{
    /// <summary>
    /// Логика взаимодействия для NotePanel3.xaml
    /// </summary>
    public partial class NotePanel3
    {
        private Thickness _tweetMargin;
        public NotePanel3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //var notificationView = Application.Current.Windows.OfType<NotifcationView>().FirstOrDefault();
            //notificationView = new NotifcationView();
            //notificationView.Show();
        }
        public Thickness TweetMargin
        {
            get { return _tweetMargin; }
            set
            {
                if (_tweetMargin == value) return;
                _tweetMargin = value;
                //OnPropertyChanged();
            }
        }
    }
}
