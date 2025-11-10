using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17950 : ContentPage
{
	public bool CorrectMethodCalled { get; private set; } = false;

	public Maui17950() => InitializeComponent();

	void TestBtn(object sender, EventArgs e)
	{
		Console.WriteLine("event called");
		CorrectMethodCalled = true;
	}

	void TestBtn(object sender, string e)
	{
		Console.WriteLine("wrong event called");
		// TODO: XUnit doesn't have Assert.Fail, use Assert.True(false, ...) or throw
		throw new InvalidOperationException("wrong method invoked");
	}

	void TestBtn()
	{
		Console.WriteLine("normal method called");
		// TODO: XUnit doesn't have Assert.Fail, use Assert.True(false, ...) or throw
		throw new InvalidOperationException("wrong method invoked");
	}

	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			AppInfo.SetCurrent(new MockAppInfo());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

		[Theory]
		[Values]
		public void AmbiguousMatch(XamlInflator inflator)
		{
			Maui17950 page = new Maui17950(inflator);
			page.button.SendClicked();
			Assert.True(page.CorrectMethodCalled, "Expected correct TestBtn(object sender, EventArgs e) to be called");
		}
	}
}