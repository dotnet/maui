using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public partial class Maui25608_2
{
	public Maui25608_2() => InitializeComponent();

	class Test
	{
		EventHandler<BindingBaseErrorEventArgs> _bindingFailureHandler;

		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown]
		public void TearDown()
		{
			if (_bindingFailureHandler is not null)
			{
				BindingDiagnostics.BindingFailed -= _bindingFailureHandler;
			}

			AppInfo.SetCurrent(null);
		}

		[Test]
		public void TestInvalidBindingWithRelativeSource([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
				Assert.Ignore("not testing for sourcegen, it crashes the test runner");

			bool bindingFailureReported = false;
			_bindingFailureHandler = (sender, args) =>
			{
				bindingFailureReported = true;
				Assert.AreEqual("Mismatch between the specified x:DataType (Microsoft.Maui.Controls.VerticalStackLayout) and the current binding context (Microsoft.Maui.Controls.Xaml.UnitTests.Maui25608_2).", args.Message);
			};
			BindingDiagnostics.BindingFailed += _bindingFailureHandler;

			var page = new Maui25608_2(inflator);

			Assert.AreNotEqual(25, page.Image.HeightRequest);
			Assert.IsTrue(bindingFailureReported);
		}
	}
}
