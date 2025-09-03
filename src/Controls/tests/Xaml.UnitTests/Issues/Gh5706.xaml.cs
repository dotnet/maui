using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh5706 : Shell
{
	class VM
	{
		public VM() => FilterCommand = new Command((p) => Param = p);

		public Command FilterCommand { get; set; }

		public object Param { get; set; }
	}

	public Gh5706() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Test]
		public void ReportSyntaxError([Values] XamlInflator inflator)
		{
			var layout = new Gh5706(inflator);
			layout.searchHandler.BindingContext = new VM();

			Assert.That(layout.searchHandler.CommandParameter, Is.Null);
			layout.searchHandler.Query = "Foo";
			Assert.That(layout.searchHandler.CommandParameter, Is.EqualTo("Foo"));
		}
	}

	class Gh5706VM
	{
		public Gh5706VM()
		{
			FilterCommand = new Command((p) => Param = p);
		}

		public Command FilterCommand { get; set; }

		public object Param { get; set; }
	}
}
