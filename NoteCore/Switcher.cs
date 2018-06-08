using System;
using System.Windows.Controls;
using FirstFloor.ModernUI.Windows.Controls;
using GalaSoft.MvvmLight.Ioc;
using NoteCore.Service;

namespace NoteCore
{
    public static class Switcher
    {
        private static readonly INavigationService PageSwitcher = SimpleIoc.Default.GetInstance<INavigationService>();
        public static void SlideBack()
        {
            PageSwitcher.SlideBack();
        }

        public static void SlideNext()
        {
            PageSwitcher.SlideNext();
        }
        public static void ShowPage(Uri pageUri)
        {
            PageSwitcher.ShowPage(pageUri);
        }
        public static void ChangeViewCollection(Enumerations.PageCollections selectCollection)
        {
            PageSwitcher.ChangeViewCollection(selectCollection);
        }
        public static void NavigateTo<TView>(OpeningMode openingMode = OpeningMode.ContentFrame) where TView : UserControl, new()
        {
            PageSwitcher.NavigateTo<TView>(openingMode);
        }

        public static void NavigateTo<TView, TParam>(TParam param, OpeningMode openingMode = OpeningMode.ContentFrame) where TView : UserControl, new()
        {
            PageSwitcher.NavigateTo<TView, TParam>(param, openingMode);
        }

        public static void NavigateTo<TView>(Action<ModernDialog> doModernDialog) where TView : UserControl, new()
        {
            PageSwitcher.NavigateTo<TView>(doModernDialog);
        }

    }
}