using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class ColorConverterVM
{
	public string ButtonBackground => "#fc87ad";
}

public partial class ColorConverter : ContentPage
{

	public ColorConverter() => InitializeComponent();


	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

		[Theory]
		[Values]
		public void StringsAreValidAsColor(XamlInflator inflator)
		{
			var page = new ColorConverter(inflator);
			page.BindingContext = new ColorConverterVM();

			var expected = Color.FromArgb("#fc87ad");
			Assert.Equal(expected, page.Button0.BackgroundColor);
		}
	}
}
