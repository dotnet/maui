using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSTitleViewPage : ContentPage
	{
		public iOSTitleViewPage()
		{
			InitializeComponent();
			_searchBar.Effects.Add(Effect.Resolve("XamarinDocs.SearchBarEffect"));
		}

		void OnReturnButtonClicked(object sender, EventArgs e)
		{
			Navigation.PopAsync();
		}
	}
}
