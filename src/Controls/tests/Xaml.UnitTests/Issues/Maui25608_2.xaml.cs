using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui25608_2
{
	public Maui25608_2() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		bool enableDiagnosticsInitialState;
		bool bindingFailureReported = false;

		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			enableDiagnosticsInitialState = RuntimeFeature.EnableDiagnostics;
			RuntimeFeature.EnableMauiDiagnostics = true;
			BindingDiagnostics.BindingFailed += BindingFailed;
		}

		public void Dispose()
		{
			BindingDiagnostics.BindingFailed -= BindingFailed;
			RuntimeFeature.EnableMauiDiagnostics = enableDiagnosticsInitialState;
			DispatcherProvider.SetCurrent(null);
			AppInfo.SetCurrent(null);
		}

		void BindingFailed(object sender, BindingBaseErrorEventArgs args)
		{
			bindingFailureReported = true;
			Assert.Equal("Mismatch between the specified x:DataType (Microsoft.Maui.Controls.VerticalStackLayout) and the current binding context (Microsoft.Maui.Controls.Xaml.UnitTests.Maui25608_2).", args.Message);
		}

		[Theory]
		[XamlInflatorData]
		internal void TestInvalidBindingWithRelativeSource(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui25608_2));

				// Skip for sourcegen as it crashes the test runner
				return;
			}


			var page = new Maui25608_2(inflator);

			Assert.NotEqual(25, page.Image.HeightRequest);
			Assert.True(bindingFailureReported);
		}
	}
}
