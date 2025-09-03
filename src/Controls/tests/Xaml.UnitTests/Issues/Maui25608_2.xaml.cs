using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui25608_2
{
	public Maui25608_2() => InitializeComponent();

	class Test
	{
		bool enableDiagnosticsInitialState;

		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			enableDiagnosticsInitialState = RuntimeFeature.EnableDiagnostics;
			RuntimeFeature.EnableMauiDiagnostics = true;
			BindingDiagnostics.BindingFailed += BindingFailed;
			bindingFailureReported = false;
		}

		[TearDown]
		public void TearDown()
		{
			BindingDiagnostics.BindingFailed -= BindingFailed;
			RuntimeFeature.EnableMauiDiagnostics = enableDiagnosticsInitialState;

			AppInfo.SetCurrent(null);
		}

		bool bindingFailureReported = false;

		void BindingFailed(object sender, BindingBaseErrorEventArgs args)
		{
			bindingFailureReported = true;
			Assert.AreEqual("Mismatch between the specified x:DataType (Microsoft.Maui.Controls.VerticalStackLayout) and the current binding context (Microsoft.Maui.Controls.Xaml.UnitTests.Maui25608_2).", args.Message);
		}

		[Test]
		public void TestInvalidBindingWithRelativeSource([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui25608_2));

				Assert.Ignore("not testing for sourcegen, it crashes the test runner");
			}


			var page = new Maui25608_2(inflator);

			Assert.AreNotEqual(25, page.Image.HeightRequest);
			Assert.IsTrue(bindingFailureReported);
		}
	}
}
