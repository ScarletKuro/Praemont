using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using GalaSoft.MvvmLight.Messaging;
using NoteCore.Utils;

namespace NoteCore.Service
{
    public enum OpeningMode
    {
        ContentFrame,
        Window
    }
    public class NavigationService : INavigationService
    {
        private ModernFrame _frame;
        private readonly IMessenger _messenger;
        private readonly DefaultLinkNavigator _linkNavigator = new DefaultLinkNavigator();
        private readonly Tuple<Uri[], Uri[], Uri[], Uri[]> _panelsArray;
        private CollectionView _view;
        public NavigationService()
        {
            _panelsArray = Tuple.Create(new[]
            {
                new Uri("/Pages/NotePanels/NotePanel3.xaml", UriKind.Relative)
            }
                , new[]
                {
                    new Uri("/Pages/OptionsPanels/SettingLayout.xaml", UriKind.Relative)
                }
                , new[]
                {
                    new Uri("/Pages/AuthenticateView.xaml", UriKind.Relative)
                }
                , new[]
                {
                    new Uri("/Pages/AddNotePanel.xaml", UriKind.Relative)
                }
                );
            _frame = Application.Current.MainWindow.FindChild<ModernFrame>("ModernFrame");
            _messenger = Messenger.Default;
            _messenger.Register<IMainWindow>(this, messenger =>
            {
                if (messenger.LoadedForm)
                {
                    _frame = Application.Current.MainWindow.FindChild<ModernFrame>("ModernFrame");
                }
            });
            _view = CollectionViewSource.GetDefaultView(_panelsArray.Item1) as CollectionView;
        }

        public void ChangeViewCollection(Enumerations.PageCollections selectCollection)
        {
            switch (selectCollection)
            {
                case Enumerations.PageCollections.NotePanels:
                    _view = CollectionViewSource.GetDefaultView(_panelsArray.Item1) as CollectionView;
                    break;
                case Enumerations.PageCollections.SettingsPanel:
                    _view = CollectionViewSource.GetDefaultView(_panelsArray.Item2) as CollectionView;
                    break;
                case Enumerations.PageCollections.LoginPanel:
                    _view = CollectionViewSource.GetDefaultView(_panelsArray.Item3) as CollectionView;
                    break;
                case Enumerations.PageCollections.AddNotePanel:
                    _view = CollectionViewSource.GetDefaultView(_panelsArray.Item4) as CollectionView;
                    break;
            }
            if (_view != null)
            {
                _view.MoveCurrentToPosition(0);
                ShowPage(_view.CurrentItem as Uri);
            }
        }
        public void NavigateTo<TView>(OpeningMode openingMode = OpeningMode.ContentFrame) where TView : UserControl, new()
        {
            NavigateToView<TView>(openingMode);
        }

        public void NavigateTo<TView, TParam>(TParam param, OpeningMode openingMode = OpeningMode.ContentFrame) where TView : UserControl, new()
        {
            NavigateToView<TView>(openingMode);
            _messenger.Send(param);
        }

        private void NavigateToView<TView>(OpeningMode openingMode) where TView : UserControl, new()
        {
            var view = new TView();
            switch (openingMode)
            {
                case OpeningMode.Window:
                    var newWindow = new ModernDialog { Content = view };
                    newWindow.ShowDialog();
                    break;
                default:
                    _frame.Content = view;
                    break;
            }
        }
        public void NavigateTo<TView>(Action<ModernDialog> doModernDialog) where TView : UserControl, new()
        {
            var view = new TView();
            var newWindow = new ModernDialog { Content = view };
            doModernDialog(newWindow);
        }

        public void SlideNext()
        {
            int maxposition = _view.Count - 1;
            if (_view.CurrentPosition.Equals(maxposition))
                _view.MoveCurrentToFirst();
            else
                _view.MoveCurrentToNext();
            ShowPage(_view.CurrentItem as Uri);
        }

        public void SlideBack()
        {
            if (_view.CurrentPosition.Equals(0))
                _view.MoveCurrentToLast();
            else
                _view.MoveCurrentToPrevious();
            ShowPage(_view.CurrentItem as Uri);
        }

        public void ShowPage(Uri uriPage)
        {
            _linkNavigator.Navigate(uriPage, _frame, "ModernFrame");
        }
    }
}