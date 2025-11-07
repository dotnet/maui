using System;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui20244 : ContentPage
{
	public Maui20244() => InitializeComponent();

	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			Application.SetCurrentApplication(null);
			DispatcherProvider.SetCurrent(null);
			Application.Current = null;
		}

		[Theory]
		[Values]
		public void RowDefStaticResource(XamlInflator inflator)
		{
			var page = new Maui20244(inflator);
			var grid = page.grid;

			Assert.Equal(6, grid.RowDefinitions.Count);
			// TODO: Fix - needs actual value Assert.Equal(new GridLength(1, GridUnitType.Star), grid.RowDefinitions[?].Height);
			// TODO: Fix - needs actual value Assert.Equal(new GridLength(1, GridUnitType.Star), grid.RowDefinitions[?].Height);
			// TODO: Fix - needs actual value Assert.Equal(new GridLength(1, GridUnitType.Star), grid.RowDefinitions[?].Height);
			// TODO: Fix - needs actual value Assert.Equal(new GridLength(1, GridUnitType.Star), grid.RowDefinitions[?].Height);
			// TODO: Fix - needs actual value Assert.Equal(new GridLength(1, GridUnitType.Star), grid.RowDefinitions[?].Height);
			// TODO: Fix - needs actual value Assert.Equal(new GridLength(1, GridUnitType.Auto), grid.RowDefinitions[?].Height);

			Assert.Equal(3, grid.ColumnDefinitions.Count);

		}
	}
}
