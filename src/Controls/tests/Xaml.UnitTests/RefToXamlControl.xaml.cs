using System;
using System.IO;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class RefToXamlControl : ContentPage
{
	public RefToXamlControl() => InitializeComponent();


	public class Test
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[Values]
		public void CanRefToXamlControlsWithoutBaseClass(XamlInflator inflator)
		{
			var page = new RefToXamlControl(inflator);
			Assert.IsType<CustomButtonNoBaseClass>(page.Content);

		}
		static string GetThisFilePath([System.Runtime.CompilerServices.CallerFilePath] string path = null) => path ?? string.Empty;

	}
}
