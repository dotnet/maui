using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17950 : ContentPage
{
	bool _correctEventCalled = false;

	public Maui17950() => InitializeComponent();

	void TestBtn(object sender, EventArgs e)
	{
		Console.WriteLine("event called");
		_correctEventCalled = true;
	}

	void TestBtn(object sender, string e)
	{
		Console.WriteLine("wrong event called");
	}

	void TestBtn()
	{
		Console.WriteLine("normal method called");
	}

	[Collection("Issue")]
	public class Test : IDisposable
	{
		public Test() => AppInfo.SetCurrent(new MockAppInfo());
		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void AmbiguousMatch(XamlInflator inflator)
		{
			Maui17950 page = new Maui17950(inflator);
			page.button.SendClicked();
			Assert.True(page._correctEventCalled);
		}
	}
}