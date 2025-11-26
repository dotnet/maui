using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui21434
{
	public Maui21434() => InitializeComponent();

	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void BindingsDoNotResolveStaticProperties([Values] XamlInflator inflator)
		{
			var page = new Maui21434(inflator);
			Assert.That(page.ParentTextLabel?.Text, Is.EqualTo("ParentText"));
			Assert.That(page.ChildTextLabel?.Text, Is.EqualTo("ChildText"));
		}
	}
}

public class ParentViewModel21434
{
	public string Text => "ParentText";
	public ChildViewModel21434 Child { get; } = new();
}

public class ChildViewModel21434
{
	public string Text => "ChildText";
}
