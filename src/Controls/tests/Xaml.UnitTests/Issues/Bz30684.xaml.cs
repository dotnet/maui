using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz30684 : ContentPage
	{
		public Bz30684()
		{
			InitializeComponent();
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[Test]
			public void XReferenceFindObjectsInParentNamescopes([Values] XamlInflator inflator)
			{
				var layout = new Bz30684(inflator);
				var cell = (TextCell)layout.listView.TemplatedItems.GetOrCreateContent(0, null);
				Assert.AreEqual("Foo", cell.Text);
			}
		}
	}
}