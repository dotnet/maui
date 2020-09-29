using System;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Issue2152 : ContentPage
	{
		public Issue2152()
		{
			InitializeComponent();
		}

		public Issue2152(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		int clickcount;
		public void OnButtonClicked(object sender, EventArgs e)
		{
			clickcount++;
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TestEventConnection(bool useCompiledXaml)
			{
				Issue2152 layout = null;
				Assert.DoesNotThrow(() => layout = new Issue2152(useCompiledXaml));
				Cell cell = null;
				Assert.DoesNotThrow(() => cell = layout.listview.TemplatedItems.GetOrCreateContent(0, null));
				var button = cell.FindByName<Button>("btn") as IButtonController;
				Assert.AreEqual(0, layout.clickcount);
				button.SendClicked();
				Assert.AreEqual(1, layout.clickcount);
			}
		}
	}
}