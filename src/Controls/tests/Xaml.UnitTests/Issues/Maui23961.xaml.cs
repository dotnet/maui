using System;
using System.ComponentModel;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui23961 : ContentPage
{
	public Maui23961() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests()
		{
			AppInfo.SetCurrent(new MockAppInfo());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
		}

		[Theory]
		[XamlInflatorData]
		internal void ObsoleteTypesAndPropertiesCanBeLoaded(XamlInflator inflator)
		{
			var page = new Maui23961(inflator);
			Assert.NotNull(page);
		}
	}
}

// Test classes for obsolete attribute handling
[Obsolete("[Test Message]: Maui23961ObsoleteClass2 is deprecated", error: false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class Maui23961ObsoleteClass2 : Grid { }

public class Maui23961Class1 : Grid
{
	[Obsolete("[Test Message]: BP1Property is deprecated", error: false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly BindableProperty BP1Property =
		BindableProperty.Create(nameof(BP1), typeof(string), typeof(Maui23961Class1), null);

	[Obsolete("[Test Message]: BP1 property is deprecated", error: false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public string BP1
	{
		get => (string)GetValue(BP1Property);
		set => SetValue(BP1Property, value);
	}

	[Obsolete("[Test Message]: P1 is deprecated", error: false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public string P1 { get; set; }
}
