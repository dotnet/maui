using System;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

public partial class Maui29334 : ContentPage
{

	public Maui29334() => InitializeComponent();

	public Maui29334(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}


	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Fact]
		public void OnIdiomGridLength()
		{
			var page = new Maui29334(true); // TODO: This test needs useCompiledXaml parameter

		}

		public void Dispose()
		{
			Application.Current = null;
			DispatcherProvider.SetCurrent(null);
		}
	}
}