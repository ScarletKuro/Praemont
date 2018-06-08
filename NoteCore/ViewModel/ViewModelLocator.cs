/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:MvvmLight1.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using NoteCore.Model;
using NoteCore.Repositories;
using NoteCore.Service;
using NoteCore.Twitter.Pages;
using ProtoBuf.Meta;

namespace NoteCore.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            RuntimeTypeModel.Default.Add(typeof(System.Windows.Media.Color), false).Add("R", "G", "B", "A");
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                 
            }
            else
            {

                SimpleIoc.Default.Register<IOptionRepository>(()=> new OptionRepository("Config"));
                SimpleIoc.Default.Register<TwitterService>();
                SimpleIoc.Default.Register<AppearanceViewModel>();
                SimpleIoc.Default.Register<OptionsOneViewModel>();
                SimpleIoc.Default.Register<NoteViewModel>();
                SimpleIoc.Default.Register<SearchViewModel>();
                SimpleIoc.Default.Register<AuthenticateViewModel>();
                SimpleIoc.Default.Register<MainViewModel>();
                SimpleIoc.Default.Register<INavigationService, NavigationService>();
            }
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        public AppearanceViewModel Appearance
        {
            get { return ServiceLocator.Current.GetInstance<AppearanceViewModel>(); }
        }
        public OptionsOneViewModel OptionOne
        {
            get { return ServiceLocator.Current.GetInstance<OptionsOneViewModel>();}
        }
        /// <summary>
        /// Gets the Note Property
        /// </summary>
        public NoteViewModel Note
        {
            get { return ServiceLocator.Current.GetInstance<NoteViewModel>(); }
        }

        public AuthenticateViewModel Authenticate
        {
            get { return ServiceLocator.Current.GetInstance<AuthenticateViewModel>(); }
        }

        public SearchViewModel Search
        {
            get { return ServiceLocator.Current.GetInstance<SearchViewModel>(); }
        }

        public static ViewModelLocator Instance
        {
            get { return Application.Current.Resources["Locator"] as ViewModelLocator; }
        }


        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
        }
    }
}