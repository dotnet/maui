using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui25871 : ContentPage
{
	public Maui25871() => InitializeComponent();

	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Fact]
		public void CompilationDoesNotFail()
		{
			MockCompiler.Compile(typeof(Maui25871));
		}

		public void Dispose()
		{
			Application.Current = null;
			DispatcherProvider.SetCurrent(null);
		}
	}
}

#nullable enable
public class Maui25871ViewModel
{
}
