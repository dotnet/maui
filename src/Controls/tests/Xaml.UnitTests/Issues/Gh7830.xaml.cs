// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh7830 : ContentPage
{
	public static string StaticText = "Foo";
	public Gh7830() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Test]
		public void CanResolvexStaticWithShortName([Values] XamlInflator inflator)
		{
			var layout = new Gh7830(inflator);
			var cell = layout.listView.ItemTemplate.CreateContent() as ViewCell;
			Assert.That((cell.View as Label).Text, Is.EqualTo(StaticText));
		}
	}
}
