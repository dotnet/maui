using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue1306 : ListView
{
	public Issue1306() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Test]
		public void AssignBindingMarkupToBindingBase([Values] XamlInflator inflator)
		{
			var listView = new Issue1306(inflator);

			Assert.NotNull(listView.GroupDisplayBinding);
			Assert.NotNull(listView.GroupShortNameBinding);
			Assert.That(listView.GroupDisplayBinding, Is.TypeOf<Binding>());
			Assert.That(listView.GroupShortNameBinding, Is.TypeOf<Binding>());
			Assert.AreEqual("Key", (listView.GroupDisplayBinding as Binding).Path);
			Assert.AreEqual("Key", (listView.GroupShortNameBinding as Binding).Path);
		}
	}
}

