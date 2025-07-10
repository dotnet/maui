using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui10396 : ContentView
{

	public Maui10396() => InitializeComponent();

	public Maui10396(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void CompiledBindingToArray([Values(false, true)] bool useCompiledXaml)
		{
			if (useCompiledXaml)
				MockCompiler.Compile(typeof(Maui10396));

			var page = new Maui10396(useCompiledXaml) { BindingContext = new Maui10396VM() };

			Assert.AreEqual("1", page.card0.label.Text);
			Assert.AreEqual("2", page.card1.label.Text);
			Assert.AreEqual("3", page.card2.label.Text);
			Assert.AreEqual("4", page.card3.label.Text);
			Assert.AreEqual("5", page.card4.label.Text);
			Assert.AreEqual("6", page.card5.label.Text);
		}
	}
}

public class Maui10396VM
{
	//public List<Maui10396CardVM> Cards { get; } = Enumerable.Range(1, 6).Select(i => new Maui10396CardVM(i)).ToList();
	public Maui10396CardVM[] Cards { get; } = Enumerable.Range(1, 6).Select(i => new Maui10396CardVM(i)).ToArray();
}

public class Maui10396CardVM
{
	public int Number { get; set; }

	public Maui10396CardVM(int number)
	{
		Number = number;
	}
}