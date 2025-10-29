using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

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

	class Tests
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void AccessInternalEvent([Values] XamlInflator inflator)
		{
			//shouldn't throw
			new Maui12566(inflator);
		}
	}
}
