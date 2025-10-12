using ClientChatApp.ViewModels;
using System.Windows;

namespace ClientChatApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}


