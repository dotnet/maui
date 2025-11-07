using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17950 : ContentPage
{
	public Maui17950() => InitializeComponent();

	void TestBtn(object sender, EventArgs e)
	{
		Console.WriteLine("event called");
		// TODO: XUnit has no // TODO: XUnit has no Assert.Pass() - test passes if no exception is thrown - test passes if no exception is thrown
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
			 AppInfo.SetCurrent(new MockAppInfo());
		}

		public void Dispose()
		{
			 AppInfo.SetCurrent(null);
		}
		[Theory]
		[Values]
		public void AmbiguousMatch(XamlInflator inflator)
		{
			Maui17950 page = new Maui17950(inflator);
			page.button.SendClicked();
			// TODO: XUnit doesn't have Assert.Fail, use Assert.True(false, ...) or throw
			throw new InvalidOperationException("no method invoked");
		}
	}
}