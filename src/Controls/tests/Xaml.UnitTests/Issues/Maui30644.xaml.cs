using System;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Maui30644ViewModel
{
	public List<string> Values { get; } = new List<string> { "Alpha", "Beta", "Gamma" };

	public string FirstSelectedValue { get; set; } = "Beta";

	public string SecondSelectedValue { get; set; } = "Beta";
}

public partial class Maui30644 : ContentPage
{
	public Maui30644() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void PickerSelectedItemBeforeItemsSourceIsSelected(XamlInflator inflator)
		{
			var viewModel = new Maui30644ViewModel();
			var page = new Maui30644(inflator) { BindingContext = viewModel };

			Assert.Equal(1, page.ItemsSourceFirstPicker.SelectedIndex);
			Assert.Equal("Beta", page.ItemsSourceFirstPicker.SelectedItem);

			Assert.Equal(1, page.SelectedItemFirstPicker.SelectedIndex);
			Assert.Equal("Beta", page.SelectedItemFirstPicker.SelectedItem);
			Assert.Equal("Beta", viewModel.FirstSelectedValue);
		}
	}
}
