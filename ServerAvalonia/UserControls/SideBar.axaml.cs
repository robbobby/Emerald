using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ServerAvalonia.UserControls {
    public class SideBar : UserControl {
        public SideBar() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

