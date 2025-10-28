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
	public Maui25608_2()
	{
		InitializeComponent();
	}

	public Maui25608_2(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}	class Test
	{
		EventHandler<BindingBaseErrorEventArgs> _bindingFailureHandler;

		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown]
		public void TearDown()
		{
			if (_bindingFailureHandler is not null)
			{
				BindingDiagnostics.BindingFailed -= _bindingFailureHandler;
			}

			AppInfo.SetCurrent(null);
		}

		[Theory]
			public void Method([InlineData(false, true)] bool useCompiledXaml)
		{
			bool bindingFailureReported = false;
			_bindingFailureHandler = (sender, args) =>
			{
				bindingFailureReported = true;
				Assert.Equal("Mismatch between the specified x:DataType (Microsoft.Maui.Controls.VerticalStackLayout) and the current binding context (Microsoft.Maui.Controls.Xaml.UnitTests.Maui25608_2).", args.Message);
			};
			BindingDiagnostics.BindingFailed += _bindingFailureHandler;

			var page = new Maui25608_2(useCompiledXaml);

			Assert.NotEqual(25, page.Image.HeightRequest);
			Assert.True(bindingFailureReported);
		}
	}
}
