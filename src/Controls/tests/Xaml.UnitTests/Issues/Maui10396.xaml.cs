using System;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui10396 : ContentView
{
	public Maui10396() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => AppInfo.SetCurrent(new MockAppInfo());
		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void CompiledBindingToArray(XamlInflator inflator)
		{
			var page = new Maui10396(inflator) { BindingContext = new Maui10396VM() };

			Assert.Equal("1", page.card0.label.Text);
			Assert.Equal("2", page.card1.label.Text);
			Assert.Equal("3", page.card2.label.Text);
			Assert.Equal("4", page.card3.label.Text);
			Assert.Equal("5", page.card4.label.Text);
			Assert.Equal("6", page.card5.label.Text);
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

	public Maui10396CardVM(int number) => Number = number;
}