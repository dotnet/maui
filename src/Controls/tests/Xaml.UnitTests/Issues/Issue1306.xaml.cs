using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue1306 : ListView
{
	public Issue1306() => InitializeComponent();


	public class Tests : IDisposable
	{

		public void Dispose() { }
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());

		[Theory]
		[Values]
		public void AssignBindingMarkupToBindingBase(XamlInflator inflator)
		{
			var listView = new Issue1306(inflator);

			Assert.NotNull(listView.GroupDisplayBinding);
			Assert.NotNull(listView.GroupShortNameBinding);
			Assert.IsType<Binding>(listView.GroupDisplayBinding);
			Assert.IsType<Binding>(listView.GroupShortNameBinding);
			Assert.Equal("Key", (listView.GroupDisplayBinding as Binding).Path);
			Assert.Equal("Key", (listView.GroupShortNameBinding as Binding).Path);
		}
	}
}

