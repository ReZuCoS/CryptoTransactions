using System.Windows;
using System.Windows.Navigation;

namespace CryptoTransactions.WinClient.View.Windows
{
    public partial class WindowMain : Window
    {
        public WindowMain() =>
            InitializeComponent();

        private void ClearFrameHistory(object sender, NavigationEventArgs e) =>
            frameMain.NavigationService.RemoveBackEntry();
    }
}
