using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

partial class AutomationProperties : ContentPage
{
	public AutomationProperties() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp] public void Setup() => Application.Current = new MockApplication();
		[TearDown] public void TearDown() => Application.Current = null;

		[Test]
		public void AutomationPropertiesName([Values] XamlInflator inflator)
		{
			var layout = new AutomationProperties(inflator);

			Assert.AreEqual("Name", (string)layout.entry.GetValue(Microsoft.Maui.Controls.AutomationProperties.NameProperty));
		}

		[Test]
		public void AutomationPropertiesHelpText([Values] XamlInflator inflator)
		{
			var layout = new AutomationProperties(inflator);

			Assert.AreEqual("Sets your name", (string)layout.entry.GetValue(Microsoft.Maui.Controls.AutomationProperties.HelpTextProperty));
		}

		[Test]
		public void AutomationPropertiesIsInAccessibleTree([Values] XamlInflator inflator)
		{
			var layout = new AutomationProperties(inflator);
			Application.Current.LoadPage(layout);

			Assert.AreEqual(true, (bool?)layout.entry.GetValue(Microsoft.Maui.Controls.AutomationProperties.IsInAccessibleTreeProperty));
		}

		[Test]
		public void AutomationPropertiesLabeledBy([Values] XamlInflator inflator)
		{
			var layout = new AutomationProperties(inflator);
			Application.Current.LoadPage(layout);

			Assert.AreEqual(layout.label, (Element)layout.entry.GetValue(Microsoft.Maui.Controls.AutomationProperties.LabeledByProperty));
		}
	}
}
