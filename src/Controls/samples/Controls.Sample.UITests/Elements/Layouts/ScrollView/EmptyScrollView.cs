using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Controls.Sample.UITests.Elements
{
	// Issue1538.cs
	public class EmptyScrollView : ContentPage
	{
		readonly ScrollView _sv;

		public EmptyScrollView()
		{
			StackLayout sl = new StackLayout() { VerticalOptions = LayoutOptions.Fill };
			sl.Children.Add(_sv = new ScrollView() { HeightRequest = 100 });
			Content = sl;

			AddContentDelayed();
		}

		async void AddContentDelayed()
		{
			await Task.Delay(1000);
			_sv.Content = new Label { Text = "Foo" };
		}
	}
}