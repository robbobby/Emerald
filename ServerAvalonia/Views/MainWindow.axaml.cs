using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ServerAvalonia.Views {
    public class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
        private void OnOpenClicked(object? sender, EventArgs e) {
            throw new NotImplementedException();
        }
        private void OnCloseClicked(object? sender, EventArgs e) {
            throw new NotImplementedException();
        }
    }
}
