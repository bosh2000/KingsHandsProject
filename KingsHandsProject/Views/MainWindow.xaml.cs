using KingsHandsProject.Services;
using KingsHandsProject.ViewModels;
using System.Windows;

namespace KingsHandsProject.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel(
                new FolderDialogService(),
                new PokerLogScannerService(new JsonPokerHandParser()),
                new WpfUiDispatcher());
        }
    }
}