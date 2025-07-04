using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

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

	// [TestFixture] - removed for xUnit
	class Test
	{
		EventHandler<BindingBaseErrorEventArgs> _bindingFailureHandler;

		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void TearDown()
		{
			if (_bindingFailureHandler is not null)
			{
				BindingDiagnostics.BindingFailed -= _bindingFailureHandler;
			}

			AppInfo.SetCurrent(null);
		}

		[Fact]
		public void TestValidBindingWithRelativeSource([Values(false, true)] bool useCompiledXaml)
		{
			bool bindingFailureReported = false;
			_bindingFailureHandler = (sender, args) => bindingFailureReported = true;
			BindingDiagnostics.BindingFailed += _bindingFailureHandler;

			var page = new Maui25608(useCompiledXaml);

			Assert.Equal(25, page.Image.HeightRequest);
			Assert.False(bindingFailureReported);
		}
	}
}
