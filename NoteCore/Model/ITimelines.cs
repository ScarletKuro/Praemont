using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using NoteCore.Twitter.Model;

namespace NoteCore.Model
{
    public interface ITimelines : INotifyPropertyChanged
    {
        void UpdateHome();
        void UpdateMentions();
        void UpdateDirectMessages();
        void UpdateFavorites();
        void UpdateTimeStamps();
        void SwitchView(View view);
        void ClearAllTimelines();
        void User(CancellationToken cancelationToken);
        void AddFavorite(Tweet tweet);
        void RemoveFavorite(Tweet tweet);
        void Search(string query);
        void DeleteTweet(Tweet tweet);
        void Retweet(Tweet tweet);
        void SignalCancel();
        CancellationToken CancellationToken { get; }
    }
}