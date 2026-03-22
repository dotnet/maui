using System;
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh7830 : ContentPage
{
	public static string StaticText = "Foo";
	public Gh7830() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void CanResolvexStaticWithShortName(XamlInflator inflator)
		{
			var layout = new Gh7830(inflator);
			var cell = layout.listView.ItemTemplate.CreateContent() as ViewCell;
			Assert.Equal(StaticText, (cell.View as Label).Text);
		}
	}
}
