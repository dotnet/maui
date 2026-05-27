using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue1306 : ListView
{
	public Issue1306() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void AssignBindingMarkupToBindingBase(XamlInflator inflator)
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

