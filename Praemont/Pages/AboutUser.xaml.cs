using System;
using System.Globalization;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using NoteCore.Service;
using NoteCore.Twitter.Model;

namespace Praemont.Pages
{
    /// <summary>
    /// Interaction logic for AboutUser.xaml
    /// </summary>
    public partial class AboutUser : Popup
    {
        public string ScreenName { private get; set; }
        public AboutUser()
        {
            InitializeComponent();
            Opened += async (sender, args) =>
            {
                DataContext = new User();
                var user = await Twitter.GetUserInformation(ScreenName);
                DataContext = user;
                var friendship = await Twitter.Friendship(ScreenName);
                user.Following = friendship.Following;
                user.FollowedBy = friendship.FollowedBy;
                if (user.Entities?.Url?.Urls?[0] != null)
                {
                    user.Url = user.Entities.Url.Urls[0].ExpandedUrl;
                }
            };
        }
    }
    [ValueConversion(typeof(bool), typeof(string))]
    public class BoolToFollowingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value
                ? "Following"
                : "Follow";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(bool), typeof(string))]
    public class BoolToUnfollowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value
                ? "Unfollow"
                : "Follow";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(bool), typeof(string))]
    public class BoolToFollowedByConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "follows you" : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
