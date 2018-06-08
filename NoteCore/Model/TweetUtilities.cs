﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NoteCore.Twitter.Model;

namespace NoteCore.Model
{
    public static class TweetUtilities
    {
        public static Tweet CreateTweet(this Status status, TweetClassification tweetType)
        {
            var isMention = false;
            var isDirectMessage = false;
            var username = new OAuth().ScreenName;
            var displayStatus = status.RetweetedStatus ?? status;

            // Direct messages don't have a User. Instead, dm's use sender and recipient collections.
            if (displayStatus.User == null)
            {
                isDirectMessage = true;
                displayStatus.User = status.Recipient.ScreenName == username ? status.Sender : status.Recipient;
            }

            if (status.Entities?.Mentions != null && status.Entities.Mentions.Any(m => m.ScreenName == username))
            {
                isMention = true;
            }

            var createdAt = DateTime.ParseExact(status.CreatedAt, "ddd MMM dd HH:mm:ss zzz yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

            var tweet = new Tweet
            {
                StatusId = status.Id,
                Name = displayStatus.User.Name,
                ScreenName = displayStatus.User.ScreenName,
                ProfileImageUrl = displayStatus.User.ProfileImageUrl,
                Text = displayStatus.Text,
                MarkupNodes = BuildMarkupNodes(displayStatus.Text, displayStatus.Entities),
                CreatedAt = createdAt,
                TimeAgo = TimeAgo(createdAt),
                IsRetweet = status.Retweeted,
                RetweetedBy = RetweetedBy(status, username),
                RetweetedByScreenName = RetweetedByScreenName(status, username),
                RetweetStatusId = status.RetweetedStatus != null ? status.RetweetedStatus.Id : string.Empty,
                MediaLinks = status.Entities?.Media?.Select(m => m.MediaUrl).ToArray() ?? new string[0],
                Urls = status.Entities?.Urls.Select(u => u.ExpandedUrl).ToArray() ?? new string[0],
                IsMyTweet = displayStatus.User.ScreenName == username,
                IsHome = tweetType == TweetClassification.Home,
                IsMention = tweetType == TweetClassification.Mention | isMention,
                IsDirectMessage = tweetType == TweetClassification.DirectMessage | isDirectMessage,
                IsFavorite = tweetType == TweetClassification.Favorite | status.Favorited,
                IsSearch = tweetType == TweetClassification.Search
            };

            return tweet;
        }

        private static string RetweetedBy(Status status, string username)
        {
            if (status.RetweetedStatus == null) return string.Empty;
            return username != status.User.ScreenName ? status.User.Name : string.Empty;
        }

        private static string RetweetedByScreenName(Status status, string username)
        {
            if (status.RetweetedStatus == null) return string.Empty;
            return username != status.User.ScreenName ? status.User.ScreenName : string.Empty;
        }

        private static MarkupNode[] BuildMarkupNodes(string text, Entities entities)
        {
            var markupItems = new List<MarkupItem>();

            if (entities.Urls != null)
            {
                markupItems.AddRange(entities.Urls.Select(url => new MarkupItem
                {
                    MarkupNodeType = MarkupNodeType.Url,
                    Text = url.Url,
                    Start = url.Indices[0],
                    End = url.Indices[1]
                }));
            }

            if (entities.Mentions != null)
            {
                markupItems.AddRange(entities.Mentions.Select(mention => new MarkupItem
                {
                    MarkupNodeType = MarkupNodeType.Mention,
                    Text = mention.ScreenName,
                    Start = mention.Indices[0],
                    End = mention.Indices[1]
                }));
            }

            if (entities.HashTags != null)
            {
                markupItems.AddRange(entities.HashTags.Select(hashtag => new MarkupItem
                {
                    MarkupNodeType = MarkupNodeType.HashTag,
                    Text = hashtag.Text,
                    Start = hashtag.Indices[0],
                    End = hashtag.Indices[1]
                }));
            }

            if (entities.Media != null)
            {
                markupItems.AddRange(entities.Media.Select(media => new MarkupItem
                {
                    MarkupNodeType = MarkupNodeType.Media,
                    Text = media.Url,
                    Start = media.Indices[0],
                    End = media.Indices[1]
                }));
            }

            var start = 0;
            var nodes = new List<MarkupNode>();
            markupItems.Sort((l, r) => l.Start - r.Start);
            foreach (var item in markupItems)
            {
                if (item.Start >= start)
                {
                    var len = item.Start - start;
                    if (start + len > text.Length) len = text.Length - start;
                    nodes.Add(new MarkupNode(MarkupNodeType.Text, text.Substring(start, len)));
                }
                nodes.Add(new MarkupNode(item.MarkupNodeType, item.Text));
                start = item.End;
            }
            if (start < text.Length) nodes.Add(new MarkupNode(MarkupNodeType.Text, text.Substring(start)));
            return nodes.ToArray();
        }

        public static string TimeAgo(this DateTime time)
        {
            var timespan = DateTime.UtcNow - time;
            if (timespan.TotalSeconds < 60) return string.Format("{0}s", timespan.TotalSeconds);
            if (timespan.TotalMinutes < 60) return string.Format("{0}m", timespan.TotalMinutes);
            if (timespan.TotalHours < 24) return string.Format("{0}h", timespan.TotalHours);
            if (timespan.TotalDays < 3) return string.Format("{0}d", timespan.TotalDays);
            return time.ToString("MMM d");
        }

        private class MarkupItem
        {
            public MarkupNodeType MarkupNodeType { get; set; }
            public string Text { get; set; }
            public int Start { get; set; }
            public int End { get; set; }
        }
    }
}