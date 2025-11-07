// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh7830 : ContentPage
{
	public static string StaticText = "Foo";
	public Gh7830() => InitializeComponent();


	public class Tests : IDisposable
	{

		public void Dispose() { }
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());

		[Theory]
		[Values]
		public void CanResolvexStaticWithShortName(XamlInflator inflator)
		{
			var layout = new Gh7830(inflator);
			var cell = layout.listView.ItemTemplate.CreateContent() as ViewCell;
			Assert.Equal(StaticText, (cell.View as Label).Text);
		}
	}
}
