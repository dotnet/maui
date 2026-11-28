using System;
using System.Collections.Concurrent;
using System.Linq;
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
	public class Test : BaseTestFixture
	{
		const string SourceCompilationSwitch = "Microsoft.Maui.RuntimeFeature.IsXamlCBindingWithSourceCompilationEnabled";
		readonly bool enableDiagnosticsInitialState;
		readonly bool sourceCompilationInitialState;
		readonly ConcurrentQueue<BindingBaseErrorEventArgs> bindingFailures = new();

		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			enableDiagnosticsInitialState = RuntimeFeature.EnableMauiDiagnostics;
			RuntimeFeature.EnableMauiDiagnostics = true;
			sourceCompilationInitialState = RuntimeFeature.IsXamlCBindingWithSourceCompilationEnabled;
			AppContext.SetSwitch(SourceCompilationSwitch, true);
			BindingDiagnostics.BindingFailed += BindingFailed;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				BindingDiagnostics.BindingFailed -= BindingFailed;
				RuntimeFeature.EnableMauiDiagnostics = enableDiagnosticsInitialState;
				AppContext.SetSwitch(SourceCompilationSwitch, sourceCompilationInitialState);
			}
			base.Dispose(disposing);
		}

		void BindingFailed(object sender, BindingBaseErrorEventArgs args)
		{
			bindingFailures.Enqueue(args);
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
			var binding = page.Image.GetContext(VisualElement.HeightRequestProperty).Bindings.GetValue();
			// FindAncestor resolves synchronously as the page's visual tree is assembled.
			var failures = bindingFailures.Where(failure => ReferenceEquals(binding, failure.Binding)).ToArray();
			Assert.NotEmpty(failures);
			Assert.All(failures, failure =>
				Assert.Equal("Mismatch between the specified x:DataType (Microsoft.Maui.Controls.VerticalStackLayout) and the current binding context (Microsoft.Maui.Controls.Xaml.UnitTests.Maui25608_2).", failure.Message));
		}
	}
}
