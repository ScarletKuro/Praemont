﻿using System;
using System.IO;
using System.Net;

namespace NoteCore.Twitter.Model
{
    public interface IWebResponse : IDisposable
    {
        Uri ResponseUri { get; }
        Stream GetResponseStream();
    }

    public sealed class WebResponseWrapper : IWebResponse
    {
        private bool _disposed;
        private WebResponse _response;

        public WebResponseWrapper(WebResponse response)
        {
            _response = response;
        }

        public Stream GetResponseStream() => _response.GetResponseStream();

        public Uri ResponseUri => _response.ResponseUri;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _response.Dispose();
            _response = null;
        }
    }
}