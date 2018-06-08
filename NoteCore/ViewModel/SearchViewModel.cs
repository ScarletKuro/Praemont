using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NoteCore.Model;
using NoteCore.Service;

namespace NoteCore.ViewModel
{
    public class SearchViewModel : ViewModelBase
    {
        private readonly TwitterService  _twitterService;
        public RelayCommand<string> SearchCommand { get; private set; }
        public SearchViewModel(TwitterService twitterService)
        {
            _twitterService = twitterService;
            SearchCommand = new RelayCommand<string>(OnSearch);
        }
        public TwitterService TwitterService { get { return _twitterService; } }
        private void OnSearch(string query)
        {
            _twitterService.Timeline.Search(query);
        }

        //public Timelines Timeline { get { return _noteView.Timeline; } }
    }
}
