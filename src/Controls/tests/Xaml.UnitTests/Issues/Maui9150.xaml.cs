using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui9150 : ContentPage
{
	public static IList StaticItems => new List<string> { "Alpha", "Beta", "Gamma" };

	public int SelectedIndexChangedCount { get; private set; }

	public Maui9150() => InitializeComponent();

	void OnSelectedIndexChanged(object sender, EventArgs e) => SelectedIndexChangedCount++;

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
		internal void PickerSelectedIndexBeforeItemsSourceIsApplied(XamlInflator inflator)
		{
			var page = new Maui9150(inflator);

			Assert.Equal(1, page.ItemsSourcePicker.SelectedIndex);
			Assert.Equal("Beta", page.ItemsSourcePicker.SelectedItem);
		}

		[Theory]
		[XamlInflatorData]
		internal void PickerSelectedIndexBeforeInlineItemsIsApplied(XamlInflator inflator)
		{
			var page = new Maui9150(inflator);

			Assert.Equal(2, page.InlineItemsPicker.SelectedIndex);
			Assert.Equal("Blue", page.InlineItemsPicker.SelectedItem);
		}

		[Theory]
		[XamlInflatorData]
		internal void PickerSelectedIndexChangedDoesNotFireDuringConstruction(XamlInflator inflator)
		{
			var page = new Maui9150(inflator);

			Assert.Equal(0, page.SelectedIndexChangedCount);
		}
	}
}
