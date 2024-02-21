using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class AndroidTitleViewPage : ContentPage
	{
		readonly ICommand? _returnToPlatformSpecificsPage;

		public AndroidTitleViewPage()
		{
			InitializeComponent();
		}

		public AndroidTitleViewPage(ICommand restore)
		{
			InitializeComponent();
			_returnToPlatformSpecificsPage = restore;
		}

		void OnReturnButtonClicked(object sender, EventArgs e)
		{
			if (_returnToPlatformSpecificsPage == null)
				return;

			_returnToPlatformSpecificsPage.Execute(null);
		}
	}
}