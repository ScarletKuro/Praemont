﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Practices.ServiceLocation;
using NoteCore.Repositories;
using NoteCore.ViewModel;

namespace NoteCore.Twitter.Model
{
    public class OAuth
    {

        //public OAuth(IOptionRepository optionRepository)
        //{
        //    //ServicePointManager.Expect100Continue = false;
        //    //ConsumerKey = consumerKey;
        //    //ConsumerSecret = consumerSecret;
        //    AccessToken = optionRepository.GetConfig.Tokens.OAuthToken;
        //    AccessTokenSecret = optionRepository.GetConfig.Tokens.OAuthSecret;
        //    ScreenName = optionRepository.GetConfig.Tokens.ScreenName;

        //}
        static OAuth()
        {
            ServicePointManager.Expect100Continue = false;
            ConsumerKey = "MMMH3qjFho2gKrztxSnu9Ye8r";
            ConsumerSecret = "ZMWMMcHGtCGhD4kQTWtLuotfQNM89H9mZCxkfZb8lKUY2mldng";
        }

        public OAuth()
        {
            var opt = ServiceLocator.Current.GetInstance<IOptionRepository>();
            AccessToken = opt.GetConfig.Tokens.OAuthToken;
            AccessTokenSecret = opt.GetConfig.Tokens.OAuthSecret;
            ScreenName = TestUserName ?? opt.GetConfig.Tokens.ScreenName;
        }

        private static string ConsumerKey { get; set; }
        private static string ConsumerSecret { get; set; }
        public string AccessTokenSecret { get; private set; }
        public string AccessToken { get; private set; }
        public string ScreenName { get; private set; }
        public static string TestUserName { private get; set; }

        public static string UrlEncode(string value)
        {
            return Uri.EscapeDataString(value);
        }

        public static string Nonce()
        {
            return Guid.NewGuid().ToString();
        }

        public static string TimeStamp()
        {
            var timespan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64(timespan.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        public static string Signature(string httpMethod, string url, string nonce, string timestamp, string accessToken, string accessTokenSecret, IEnumerable<string[]> parameters)
        {
            var parameterList = OrderedParameters(nonce, timestamp, accessToken, null, parameters);
            var parameterStrings = parameterList.Select(p => $"{p.Item1}={p.Item2}");
            var parameterString = string.Join("&", parameterStrings);
            var signatureBaseString = $"{httpMethod}&{UrlEncode(url)}&{UrlEncode(parameterString)}";
            var compositeKey = $"{UrlEncode(ConsumerSecret)}&{UrlEncode(accessTokenSecret)}";
            using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(compositeKey)))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureBaseString)));
            }
        }

        public static string AuthorizationHeader(string nonce, string timestamp, string accessToken, string signature, IEnumerable<string[]> parameters = null)
        {
            var parameterList = OrderedParameters(nonce, timestamp, accessToken, signature, parameters);
            var parameterStrings = parameterList.Select(p => $"{p.Item1}=\"{p.Item2}\"");
            var header = "OAuth " + string.Join(",", parameterStrings);
            return header;
        }

        private static IEnumerable<Tuple<string, string>> OrderedParameters(string nonce, string timestamp, string accessToken, string signature, IEnumerable<string[]> parameters)
        {
            var components = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("oauth_version", "1.0"),
                new Tuple<string, string>("oauth_nonce", UrlEncode(nonce)),
                new Tuple<string, string>("oauth_timestamp", UrlEncode(timestamp)),
                new Tuple<string, string>("oauth_signature_method", "HMAC-SHA1"),
                new Tuple<string, string>("oauth_consumer_key", UrlEncode(ConsumerKey))
            };

            if (string.IsNullOrWhiteSpace(signature) == false)
            {
                components.Add(new Tuple<string, string>("oauth_signature", UrlEncode(signature)));
            }
            if (string.IsNullOrWhiteSpace(accessToken) == false)
            {
                components.Add(new Tuple<string, string>("oauth_token", UrlEncode(accessToken)));
            }
            if (parameters != null)
            {
                components.AddRange(parameters.Select(par => new Tuple<string, string>(UrlEncode(par[0]), UrlEncode(par[1]))));
            }
            return components.OrderBy(c => c.Item1);
        }
    }
}