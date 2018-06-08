using System;
using System.Windows.Controls;
using FirstFloor.ModernUI.Windows.Controls;
using NoteCore.Model;

namespace NoteCore.Service
{
    public interface INavigationService
    {
        void SlideNext();
        void SlideBack();
        void ShowPage(Uri pageUri);
        void ChangeViewCollection(Enumerations.PageCollections selectCollection);
        void NavigateTo<TView>(OpeningMode openingMode = OpeningMode.ContentFrame) where TView : UserControl, new();
        void NavigateTo<TView, TParam>(TParam param, OpeningMode openingMode = OpeningMode.ContentFrame)
            where TView : UserControl, new();
        void NavigateTo<TView>(Action<ModernDialog> doModernDialog) where TView : UserControl, new();
    }
}
