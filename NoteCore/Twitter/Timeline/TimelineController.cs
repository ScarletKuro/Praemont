using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using NoteCore.Model;

namespace NoteCore.Twitter.Timeline
{
    public sealed class TimelineController : IDisposable
    {
        private readonly ITimelines _timelinesModel;
        private bool UseStreamingApi { get; set; }
        private bool _disposed;
        private SysTimer.Timers _timers = new SysTimer.Timers();

        public TimelineController(ITimelines timelinesModel)
        {
            _timelinesModel = timelinesModel;
            UseStreamingApi = true;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _timers.Dispose();
            _timers = null;
        }

        public void StartTimelines()
        {
            _timers.Add(UseStreamingApi ? 180 : 70, (s, e) =>
            {
                try
                {
                    _timelinesModel.UpdateHome();
                    _timelinesModel.UpdateMentions();
                    _timelinesModel.UpdateDirectMessages();
                    _timelinesModel.UpdateFavorites();
                }
                catch (WebException ex)
                {
                    Trace.TraceError(ex.Message);
                }
            });

            _timers.Add(30, (s, e) => _timelinesModel.UpdateTimeStamps());

            if (UseStreamingApi)
            {
                _timelinesModel.User(_timelinesModel.CancellationToken);
            }
        }

        public void StopTimelines()
        {
            _timers.Dispose();
            _timers = new SysTimer.Timers();
            _timelinesModel.SignalCancel();
            _timelinesModel.ClearAllTimelines();
        }

        public static void CopyTweetToClipboard(Tweet tweet)
        {
            Clipboard.SetText(tweet.Text);
        }

        public static void CopyLinkToClipboard(Tweet tweet)
        {
            Clipboard.SetText(TweetLink(tweet));
        }

        public static string TweetLink(Tweet tweet) => $"https://twitter.com/{tweet.ScreenName}/status/{tweet.StatusId}";

        public void SwitchTimeline(View view)
        {
            _timelinesModel.SwitchView(view);
        }

        public void DeleteTweet(Tweet tweet)
        {
            _timelinesModel.DeleteTweet(tweet);
        }

        public void AddFavorite(Tweet tweet)
        {
            _timelinesModel.AddFavorite(tweet);
        }

        public void RemoveFavorite(Tweet tweet)
        {
            _timelinesModel.RemoveFavorite(tweet);
        }

        public void Search(string query)
        {
            _timelinesModel.Search(query);
        }

        public void Retweet(Tweet tweet)
        {
            _timelinesModel.Retweet(tweet);
        }
    }
}