using System;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;
using Xunit.Sdk;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui20244 : ContentPage
{
	public Maui20244() => InitializeComponent();

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
		internal void RowDefStaticResource(XamlInflator inflator)
		{
			var page = new Maui20244(inflator);
			var grid = page.grid;

			Assert.Equal(6, grid.RowDefinitions.Count);
			Assert.Equal(new GridLength(1, GridUnitType.Star), grid.RowDefinitions[0].Height);
			Assert.Equal(new GridLength(1, GridUnitType.Star), grid.RowDefinitions[1].Height);
			Assert.Equal(new GridLength(1, GridUnitType.Star), grid.RowDefinitions[2].Height);
			Assert.Equal(new GridLength(1, GridUnitType.Star), grid.RowDefinitions[3].Height);
			Assert.Equal(new GridLength(1, GridUnitType.Star), grid.RowDefinitions[4].Height);
			Assert.Equal(new GridLength(1, GridUnitType.Auto), grid.RowDefinitions[5].Height);

			Assert.Equal(3, grid.ColumnDefinitions.Count);

		}
	}
}