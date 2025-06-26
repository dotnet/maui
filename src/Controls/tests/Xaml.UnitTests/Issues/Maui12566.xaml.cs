using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;
#pragma warning disable CS0067 // The event 'event' is never used

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Maui12566View : ContentView
	{
		internal event EventHandler MyEvent;
	}

	public partial class Maui12566 : ContentPage
	{
		public Maui12566() => InitializeComponent();
		public Maui12566(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		void Maui12566View_MyEvent(System.Object sender, System.EventArgs e)
		{
		}
		class Tests
		{
			[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
			[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

			[Fact]
			public void AccessInternalEvent([Values(false, true)] bool useCompiledXaml)
			{
				//shouldn't throw
				new Maui12566(useCompiledXaml);
			}
		}
	}
}
