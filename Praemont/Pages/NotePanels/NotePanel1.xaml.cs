using System.Windows;
using System.Windows.Controls;
using NoteCore.Model;
using NoteCore.ViewModel;

namespace Praemont.Pages.NotePanels
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class NotePanel1
    {
        public NotePanel1()
        {
            InitializeComponent();
        }
        private NoteViewModel ViewModel
        {
            get { return DataContext as NoteViewModel; }
        }

        private void EditNote_Click(object sender, RoutedEventArgs e)
        {
            //var menuItem = e.Source as MenuItem;
            //if (menuItem != null)
            //    ViewModel.EditNoteCommand.Execute(menuItem.DataContext as Note);
        }

        private void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            //var menuItem = e.Source as MenuItem;
            //if (menuItem != null)
            //    ViewModel.DeleteNoteCommand.Execute(menuItem.DataContext as Note);
        }
    }
}
