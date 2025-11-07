using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

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


	public class Tests : IDisposable
	{

		public void Dispose() { }
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());

		[Theory]
		[Values]
		public void ReportSyntaxError(XamlInflator inflator)
		{
			var layout = new Gh5706(inflator);
			layout.searchHandler.BindingContext = new VM();

			Assert.Null(layout.searchHandler.CommandParameter);
			layout.searchHandler.Query = "Foo";
			Assert.Equal("Foo", layout.searchHandler.CommandParameter);
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
