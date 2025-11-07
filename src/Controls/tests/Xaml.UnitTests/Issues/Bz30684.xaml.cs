using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz30684 : ContentPage
	{
		public Bz30684()
		{
			InitializeComponent();
		}


		public class Tests
		{
			// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());

			[Theory]
			[Values]
			public void XReferenceFindObjectsInParentNamescopes(XamlInflator inflator)
			{
				var layout = new Bz30684(inflator);
				var cell = (TextCell)layout.listView.TemplatedItems.GetOrCreateContent(0, null);
				Assert.Equal("Foo", cell.Text);
			}
		}
	}
}