using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class FindByName : ContentPage
{
	public FindByName() => InitializeComponent();


	public class FindByNameTests : IDisposable
	{
		public FindByNameTests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

		[Theory]
		[Values]
		public void TestRootName(XamlInflator inflator)
		{
			var page = new FindByName(inflator);
			Assert.Same(page, ((Maui.Controls.Internals.INameScope)page).FindByName("root"));
			Assert.Same(page, page.FindByName<FindByName>("root"));
		}

		[Theory]
		[Values]
		public void TestName(XamlInflator inflator)
		{
			var page = new FindByName(inflator);
			Assert.Same(page.label0, page.FindByName<Label>("label0"));
		}
	}
}