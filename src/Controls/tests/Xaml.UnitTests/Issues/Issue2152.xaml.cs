using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue2152 : ContentPage
{
	public Issue2152() => InitializeComponent();

	int clickcount;
	public void OnButtonClicked(object sender, EventArgs e) => clickcount++;

	[TestFixture]
	class Tests
	{
		[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Test]
		public void TestEventConnection([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Issue2152));
			}
			Issue2152 layout = null;
			Assert.DoesNotThrow(() => layout = new Issue2152(inflator));
			Cell cell = null;
			Assert.DoesNotThrow(() => cell = layout.listview.TemplatedItems.GetOrCreateContent(0, null));
			var button = cell.FindByName<Button>("btn") as IButtonController;
			Assert.AreEqual(0, layout.clickcount);
			button.SendClicked();
			Assert.AreEqual(1, layout.clickcount);
		}
	}
}