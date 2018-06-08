using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NoteCore.Model;
using NoteCore.Repositories;
using NoteCore.Service;
using NoteCore.Twitter.Timeline;
using NoteCore.Utils;

namespace NoteCore.ViewModel
{
    /// <summary>
    /// This is the VM for the Note Window.
    /// This class contains properties that the View can data bind to.
    /// </summary>
    public sealed class NoteViewModel
        : ViewModelBase
    {
        private readonly TwitterService _twitterService;
        /// <summary>
        /// Initializes a new instance of the NoteViewModel class.
        /// </summary>
        public NoteViewModel(TwitterService twitterService)
        {
            _twitterService = twitterService;
            _twitterService.Timeline.SwitchView(View.Home);
            //_twitterService.Timeline.SearchVisibility = Visibility.Visible;
        }
        public TwitterService TwitterService { get { return _twitterService; } }
    }
}
