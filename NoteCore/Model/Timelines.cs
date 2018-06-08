using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using NoteCore.Service;
using NoteCore.Twitter.Model;

namespace NoteCore.Model
{
    public sealed class Timelines : NotifyPropertyChanged, ITimelines, IDisposable
    {
        private readonly Collection<Tweet> _search = new Collection<Tweet>();

        private readonly Dictionary<View, Predicate<Tweet>> _timelineFilters = new Dictionary<View, Predicate<Tweet>>
        {
            {View.Unified, t => true},
            {View.Home, t => t.IsHome},
            {View.Mentions, t => t.IsMention},
            {View.Messages, t => t.IsDirectMessage},
            {View.Favorites, t => t.IsFavorite},
            {View.Search, t => t.IsSearch}
        };

        private readonly Collection<Tweet> _tweets = new Collection<Tweet>();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private bool _disposed;

        private ulong _favoritesSinceId = 1;

        private ulong _homeSinceId = 1;
        private bool _isSearching;

        private ulong _mentionsSinceId = 1;

        private ulong _messagesSinceId = 1;
        private Visibility _searchVisibility = Visibility.Collapsed;
        private RangeObservableCollection<Tweet> _timeline = new RangeObservableCollection<Tweet>();
        private View _view;

        private Predicate<Tweet> TimelineFilter
        {
            get
            {
                Predicate<Tweet> filter;
                if (_timelineFilters.TryGetValue(_view, out filter)) return filter;
                return t => false;
            }
        }

        public RangeObservableCollection<Tweet> Timeline
        {
            get { return _timeline; }
            set { SetProperty(ref _timeline, value); }
        }

        public Visibility SearchVisibility
        {
            get { return _searchVisibility; }
            set { SetProperty(ref _searchVisibility, value); }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public bool IsSearching
        {
            // ReSharper disable once UnusedMember.Global
            get { return _isSearching; }
            set { SetProperty(ref _isSearching, value); }
        }

        public Action<Action> DispatchInvokerOverride { private get; set; }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_cancellationTokenSource == null) return;
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
        public void User(CancellationToken cancelationToken)
        {
            Task.Run(() =>
            {
                while (cancelationToken.IsCancellationRequested == false)
                {
                    //var delay = Task.Delay(30*1000, cancelationToken);
                    //delay.Wait(cancelationToken);
                    //if (delay.IsCanceled || delay.IsFaulted) break;

                    Trace.TraceInformation("{ Start Twitter User Stream }");
                    const string url = "https://userstream.twitter.com/1.1/user.json";
                    var oauth = new OAuth();
                    var nonce = OAuth.Nonce();
                    var timestamp = OAuth.TimeStamp();
                    var signature = OAuth.Signature("GET", url, nonce, timestamp, oauth.AccessToken, oauth.AccessTokenSecret, null);
                    var authorizeHeader = OAuth.AuthorizationHeader(nonce, timestamp, oauth.AccessToken, signature);

                    var request = WebRequestWrapper.Create(new Uri(url));
                    request.Headers.Add("Authorization", authorizeHeader);

                    try
                    {
                        using (var response = request.GetResponse())
                        using (var stream = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                        {
                            stream.BaseStream.ReadTimeout = 60 * 1000;
                            while (true)
                            {
                                var json = stream.ReadLine();
                                if (json == null)
                                {
                                    Trace.TraceInformation("{ null }");
                                    break;
                                }
                                if (cancelationToken.IsCancellationRequested) break;
                                Trace.TraceInformation(string.IsNullOrWhiteSpace(json) ? "{ Blankline }" : json);

                                var serializer = new JavaScriptSerializer();
                                var reply = serializer.Deserialize<Dictionary<string, object>>(json);
                                if (reply != null && reply.ContainsKey("user"))
                                {
                                    Trace.TraceInformation("{ tweet identified }");
                                    var statuses = Status.ParseJson("[" + json + "]");
                                    Application.Current.Dispatcher.InvokeAsync(() =>
                                    {
                                        UpdateTimelines(statuses, TweetClassification.Home);
                                    });
                                    //Application.Current.Dispatcher.InvokeAsync
                                    //    (() => UpdateStatusHomeTimelineCommand.Command.Execute(statuses, Application.Current.MainWindow));
                                }
                            }
                        }
                    }

                    catch (WebException ex)
                    {
                        Trace.TraceError(ex.ToString());
                    }

                    catch (ArgumentNullException ex)
                    {
                        Trace.TraceError(ex.ToString());
                    }

                    catch (ArgumentException ex)
                    {
                        Trace.TraceError(ex.ToString());
                    }

                    catch (InvalidOperationException ex)
                    {
                        Trace.TraceError(ex.ToString());
                    }

                    catch (IOException ex)
                    {
                        Trace.TraceError(ex.ToString());
                    }
                }

                Trace.TraceInformation("{ Stream task ends }");
            }, cancelationToken);
        }

        public void SwitchView(View view)
        {
            if (_view == view) return;
            _view = view;
            Timeline.Clear();
            var tweets = _view == View.Search ? _search : _tweets;
            SearchVisibility = _view == View.Search ? Visibility.Visible : Visibility.Collapsed;
            Timeline.AddRange(tweets.Where(t => TimelineFilter(t)).OrderByDescending(t => t.CreatedAt).Take(200));
        }

        public void ClearAllTimelines()
        {
            Timeline.Clear();
            _tweets.Clear();
            _search.Clear();
            _homeSinceId = 1;
            _mentionsSinceId = 1;
            _messagesSinceId = 1;
            _favoritesSinceId = 1;
        }

        public async void UpdateHome()
        {
            var statuses = await Service.Twitter.HomeTimeline(_homeSinceId);
            _homeSinceId = MaxSinceId(_homeSinceId, statuses);
            await Application.Current.Dispatcher.InvokeAsync(() => { UpdateTimelines(statuses, TweetClassification.Home); });
        }

        public async void UpdateMentions()
        {
            var statuses = await Service.Twitter.MentionsTimeline(_mentionsSinceId);
            _mentionsSinceId = MaxSinceId(_mentionsSinceId, statuses);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                UpdateTimelines(statuses, TweetClassification.Mention);
            });
        }

        public async void UpdateFavorites()
        {
            var statuses = await Service.Twitter.Favorites(_favoritesSinceId);
            _favoritesSinceId = MaxSinceId(_favoritesSinceId, statuses);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                UpdateTimelines(statuses, TweetClassification.Favorite);
            });
        }

        public async void UpdateDirectMessages()
        {
            var recieved = await Service.Twitter.DirectMessages(_messagesSinceId);
            var sent = await Service.Twitter.DirectMessagesSent(_messagesSinceId);
            var statuses = recieved.Concat(sent).ToArray();
            _messagesSinceId = MaxSinceId(_favoritesSinceId, statuses);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                UpdateTimelines(statuses, TweetClassification.DirectMessage);
            });

        }

        public void UpdateTimeStamps()
        {
            DispatchInvoker(() =>
            {
                if (Timeline == null) return;
                foreach (var tweet in Timeline) tweet.TimeAgo = tweet.CreatedAt.TimeAgo();
            });
        }

        public async void AddFavorite(Tweet tweet)
        {
            if (tweet.IsFavorite) return;
            await Service.Twitter.CreateFavorite(tweet.StatusId);
            tweet.IsFavorite = true;
        }

        public async void RemoveFavorite(Tweet tweet)
        {
            if (tweet.IsFavorite == false) return;
            await Service.Twitter.DestroyFavorite(tweet.StatusId);
            tweet.IsFavorite = false;
        }

        public async void Search(string query)
        {
            try
            {
                IsSearching = true;
                _search.Clear();
                Timeline.Clear();
                var json = await Service.Twitter.Search(query);
                var statuses = SearchStatuses.ParseJson(json);
                foreach (var status in statuses.Where(s => s.RetweetedStatus == null)) _search.Add(status.CreateTweet(TweetClassification.Search));
                Timeline.AddRange(_search);
            }
            finally
            {
                IsSearching = false;
            }
        }

        public async void DeleteTweet(Tweet tweet)
        {
            await Service.Twitter.DestroyStatus(tweet.StatusId);
            _tweets.Remove(tweet);
            Timeline.Remove(tweet);
        }

        public async void Retweet(Tweet tweet)
        {
            if (tweet.IsRetweet)
            {
                var id = string.IsNullOrWhiteSpace(tweet.RetweetStatusId) ? tweet.StatusId : tweet.RetweetStatusId;
                var json = Service.Twitter.GetTweet(id);
                var status = Status.ParseJson("[" + json + "]")[0];
                var retweetStatusId = status.CurrentUserRetweet.Id;
                await Service.Twitter.DestroyStatus(retweetStatusId);
                tweet.IsRetweet = false;
            }
            else
            {
                await Service.Twitter.RetweetStatus(tweet.StatusId);
                tweet.IsRetweet = true;
            }
        }

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        public void SignalCancel()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private bool UpdateTimelines(IEnumerable<Status> statuses, TweetClassification tweetType)
        {
            var updated = false;
            foreach (var status in statuses)
            {
                var tweet = status.CreateTweet(tweetType);
                var index = _tweets.IndexOf(tweet);
                if (index == -1)
                {
                    _tweets.Add(tweet);
                }
                else
                {
                    var t = _tweets[index];
                    t.IsHome |= tweet.IsHome;
                    t.IsMention |= tweet.IsMention;
                    t.IsDirectMessage |= tweet.IsDirectMessage;
                    t.IsFavorite |= tweet.IsFavorite;
                    tweet = t;
                }

                if (TimelineFilter(tweet) && _timeline.Contains(tweet) == false)
                {
                    Timeline.Add(tweet);
                    updated = true;
                }
            }
            if (updated) SortTweetCollection(Timeline);
            return updated;
        }

        private void DispatchInvoker(Action callback)
        {
            var invoker = DispatchInvokerOverride ?? (action => Application.Current.Dispatcher.InvokeAsync(action));
            invoker(callback);
        }

        private static ulong MaxSinceId(ulong currentSinceId, ICollection<Status> statuses)
        {
            return statuses.Count > 0
                ? Math.Max(currentSinceId, statuses.Max(s => ulong.Parse(s.Id)))
                : currentSinceId;
        }

        private static void SortTweetCollection(ObservableCollection<Tweet> collection)
        {
            var i = 0;
            foreach (var item in collection.OrderByDescending(s => s.CreatedAt))
            {
                // Move will trigger a properychanged event even if the indexes are the same.
                var indexOfItem = collection.IndexOf(item);
                if (indexOfItem != i) collection.Move(indexOfItem, i);
                i += 1;
            }
        }
    }
}