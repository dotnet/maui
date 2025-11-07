using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh2632Base : ContentPage
{
	public new Gh2632Context BindingContext
	{
		get => base.BindingContext as Gh2632Context;
		set => base.BindingContext = value;
	}

	public class Gh2632Context
	{
		public string Foo { get; set; }
	}
}

public partial class Gh2632 : Gh2632Base
{
	public Gh2632() => InitializeComponent();


	public class Tests : IDisposable
	{

		public void Dispose() { }
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());

		[Theory]
		[Values]
		public void BindingDoesNotThrowOnRedefinedProperty(XamlInflator inflator)
		{
			var layout = new Gh2632(inflator);
			layout.BindingContext = new Gh2632Base.Gh2632Context { Foo = "foo" };
		}
	}
}
