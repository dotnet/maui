using Microsoft.Maui.Controls;
using System;
using System.Windows.Input;

namespace Maui.Controls.Sample.Pages
{
    public partial class AndroidTitleViewPage : ContentPage
    {
        ICommand _returnToPlatformSpecificsPage;

        public AndroidTitleViewPage(ICommand restore)
        {
            InitializeComponent();
            _returnToPlatformSpecificsPage = restore;
        }

        void OnReturnButtonClicked(object sender, EventArgs e)
        {
            _returnToPlatformSpecificsPage.Execute(null);
        }
    }
}
