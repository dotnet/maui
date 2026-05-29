using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.Issues;

public partial class Issue9978 : ContentPage
{
	public Issue9978()
	{
		InitializeComponent();
	}

	public Issue9978(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	public class Tests
	{
		[TestCase(false)]
		[TestCase(true)]
		public void OnIdiomReturnsBindableDefaultIfNotSet(bool useCompiledXaml)
		{
			var idioms = new[]
			{
				DeviceIdiom.Tablet,   
				DeviceIdiom.Phone,     
				DeviceIdiom.Desktop,
				DeviceIdiom.Watch,
				DeviceIdiom.TV         
			};

			foreach (var idiom in idioms)
			{
				DeviceInfo.SetCurrent(new MockDeviceInfo { Idiom = idiom });
				var page = new Issue9978(useCompiledXaml);

				if (idiom == DeviceIdiom.Tablet)
				{
					Assert.AreEqual(50, page.mauiButton.HeightRequest, $"Expected 50 for idiom Tablet");
				}
				else
				{
					Assert.AreEqual(-1, page.mauiButton.HeightRequest, $"Expected -1 for idiom {idiom}");
				}
			}
		}

	}
}