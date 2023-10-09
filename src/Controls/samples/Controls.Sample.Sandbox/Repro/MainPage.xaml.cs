namespace BorderIssue
{
	using Microsoft.Maui;
	using Microsoft.Maui.Controls;
	using Microsoft.Maui.Controls.Hosting;
	using Microsoft.Maui.Hosting;

	public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();

            testCoverFlow.ItemsSource = new int[] { 1, 2, 3, 4 };
            testCoverFlow2.ItemsSource = new int[] { 1, 2, 3, 4 };
        }

    }

}
