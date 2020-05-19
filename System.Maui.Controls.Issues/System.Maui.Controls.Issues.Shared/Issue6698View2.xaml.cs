#if !UITEST
using System.Maui.Xaml;

namespace System.Maui.Controls.Issues
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Issue6698View2
    {
        public Issue6698View2()
        {
            InitializeComponent();
        }
    }
}
#endif