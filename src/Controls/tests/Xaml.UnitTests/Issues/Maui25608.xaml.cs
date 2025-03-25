using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui25608
{
	public Maui25608()
	{
		InitializeComponent();
	}

	public Maui25608(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
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
		public void TestValidBindingWithRelativeSource([Values(false, true)] bool useCompiledXaml)
		{
			bool bindingFailureReported = false;
			_bindingFailureHandler = (sender, args) => bindingFailureReported = true;
			BindingDiagnostics.BindingFailed += _bindingFailureHandler;

			var page = new Maui25608(useCompiledXaml);

			Assert.AreEqual(25, page.Image.HeightRequest);
			Assert.IsFalse(bindingFailureReported);
		}
	}
}
