using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17950 : ContentPage
{
	public Maui17950() => InitializeComponent();

	void TestBtn(object sender, EventArgs e)
	{
		Console.WriteLine("event called");
		Assert.Pass();
	}

	void TestBtn(object sender, string e)
	{
		Console.WriteLine("wrong event called");
		Assert.Fail("wrong method invoked");
	}

	void TestBtn()
	{
		Console.WriteLine("normal method called");
		Assert.Fail("wrong method invoked");
	}

	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void AmbiguousMatch([Values] XamlInflator inflator)
		{
			Maui17950 page = new Maui17950(inflator);
			page.button.SendClicked();
			Assert.Fail("no method invoked");
		}
	}
}