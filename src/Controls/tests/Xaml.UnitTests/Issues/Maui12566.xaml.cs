using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Maui12566View : ContentView
{
#pragma warning disable 67
	internal event EventHandler MyEvent;
#pragma warning restore 67
}

public partial class Maui12566 : ContentPage
{
	public Maui12566() => InitializeComponent();

	void Maui12566View_MyEvent(System.Object sender, System.EventArgs e)
	{
	}

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => AppInfo.SetCurrent(new MockAppInfo());
		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void AccessInternalEvent(XamlInflator inflator)
		{
			//shouldn't throw
			new Maui12566(inflator);
		}
	}
}
