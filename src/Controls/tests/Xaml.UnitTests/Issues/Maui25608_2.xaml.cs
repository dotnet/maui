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

	public class Test
	{
		bool enableDiagnosticsInitialState;

		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			enableDiagnosticsInitialState = RuntimeFeature.EnableDiagnostics;
			RuntimeFeature.EnableMauiDiagnostics = true;
			BindingDiagnostics.BindingFailed += BindingFailed;
			bindingFailureReported = false;
		}

		public void Dispose()
		{
			BindingDiagnostics.BindingFailed -= BindingFailed;
			RuntimeFeature.EnableMauiDiagnostics = enableDiagnosticsInitialState;

			AppInfo.SetCurrent(null);
		}

		bool bindingFailureReported = false;

		void BindingFailed(object sender, BindingBaseErrorEventArgs args)
		{
			bindingFailureReported = true;
			Assert.Equal("Mismatch between the specified x:DataType (Microsoft.Maui.Controls.VerticalStackLayout) and the current binding context (Microsoft.Maui.Controls.Xaml.UnitTests.Maui25608_2).", args.Message);
		}

		[Theory]
		[Values]
		public void TestInvalidBindingWithRelativeSource(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui25608_2));

				// TODO: Convert to [Theory(Skip="reason")] or use conditional Skip attribute

			}

			var page = new Maui25608_2(inflator);

			Assert.NotEqual(25, page.Image.HeightRequest);
			Assert.True(bindingFailureReported);
		}
	}
}
