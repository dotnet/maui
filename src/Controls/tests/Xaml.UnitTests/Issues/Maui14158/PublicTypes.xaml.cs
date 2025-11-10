using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml.UnitTests.Issues.Maui14158;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.Maui14158;

public partial class PublicTypes : ContentPage
{
	public PublicTypes() => InitializeComponent();


	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}
		[Theory]
		[Values]
		public void VerifyCorrectTypesUsed(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				MockCompiler.Compile(typeof(PublicTypes));

			var page = new PublicTypes(inflator);

			Assert.IsType<PublicInExternal>(page.publicInExternal);
			Assert.IsType<PublicInHidden>(page.publicInHidden);
			Assert.IsType<PublicInVisible>(page.publicInVisible);
		}
	}
}
