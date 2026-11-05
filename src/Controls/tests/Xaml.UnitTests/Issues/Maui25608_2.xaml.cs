using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
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
		readonly bool enableDiagnosticsInitialState;
		readonly ConcurrentDictionary<BindingBase, TaskCompletionSource<BindingBaseErrorEventArgs>> bindingFailures = new();

		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			enableDiagnosticsInitialState = RuntimeFeature.EnableMauiDiagnostics;
			RuntimeFeature.EnableMauiDiagnostics = true;
			BindingDiagnostics.BindingFailed += BindingFailed;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				BindingDiagnostics.BindingFailed -= BindingFailed;
				RuntimeFeature.EnableMauiDiagnostics = enableDiagnosticsInitialState;
			}
			base.Dispose(disposing);
		}

		void BindingFailed(object sender, BindingBaseErrorEventArgs args)
		{
			GetBindingFailure(args.Binding).TrySetResult(args);
		}

		TaskCompletionSource<BindingBaseErrorEventArgs> GetBindingFailure(BindingBase binding) =>
			bindingFailures.GetOrAdd(binding, _ => new(TaskCreationOptions.RunContinuationsAsynchronously));

		[Theory]
		[XamlInflatorData]
		internal async Task TestInvalidBindingWithRelativeSource(XamlInflator inflator)
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
			var failure = await GetBindingFailure(binding).Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
			Assert.Same(binding, failure.Binding);
			Assert.Equal("Mismatch between the specified x:DataType (Microsoft.Maui.Controls.VerticalStackLayout) and the current binding context (Microsoft.Maui.Controls.Xaml.UnitTests.Maui25608_2).", failure.Message);
		}
	}
}
