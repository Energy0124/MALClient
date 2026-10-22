using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MALClient.WinUI
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public Frame Frame => RootFrame;
    }
}
