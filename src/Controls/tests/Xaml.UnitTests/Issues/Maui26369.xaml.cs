using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui26369 : ContentPage
{
	public static readonly BindableProperty NullableGridLengthProperty =
		BindableProperty.Create(nameof(NullableGridLength), typeof(GridLength?), typeof(Maui26369), null);

	[TypeConverter(typeof(GridLengthTypeConverter))]
	public GridLength? NullableGridLength
	{
		get => (GridLength?)GetValue(NullableGridLengthProperty);
		set => SetValue(NullableGridLengthProperty, value);
	}

	public Maui26369() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
		}

		[Theory]
		[XamlInflatorData]
		internal void CompilationDoesNotFail(XamlInflator inflator)
		{

			var page = new Maui26369(inflator);
			Assert.Equal(new GridLength(30), page.NullableGridLength);
		}
	}
}

