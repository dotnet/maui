using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Maui13872VM
{
	private readonly string[] list = new[] { "Bill", "Steve", "John" };
	public IReadOnlyList<string> List => list;
	public int ListCount => List.Count;
}

public partial class Maui13872 : ContentView
{
	public Maui13872() => InitializeComponent();

	public Maui13872(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void CompiledBindingToArrayCount([Values(false, true)] bool useCompiledXaml)
		{
			var page = new Maui13872(useCompiledXaml) { BindingContext = new Maui13872VM() };
			Assert.AreEqual("3", page.label0.Text);
			Assert.AreEqual("3", page.label1.Text);
		}
	}
}