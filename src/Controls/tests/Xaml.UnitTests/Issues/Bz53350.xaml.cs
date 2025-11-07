using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Bz53350Generic<T> : ContentView
{
	public static readonly BindableProperty SomeBPProperty =
		BindableProperty.Create("SomeBP", typeof(T), typeof(Bz53350Generic<T>), default(T));

	public T SomeBP
	{
		get { return (T)GetValue(SomeBPProperty); }
		set { SetValue(SomeBPProperty, value); }
	}

	public T SomeProperty { get; set; }
}

public class Bz53350String : Bz53350Generic<string>
{

}

public partial class Bz53350
{
	public Bz53350()
	{
	}


	public class Tests : IDisposable
	{

		public void Dispose()
		{
			Application.Current = null;
		}

		[Theory]
		[Values]
		public void PropertiesWithGenericType(XamlInflator inflator)
		{
			var layout = new Bz53350(inflator);
			Assert.Equal("Foo", layout.content.SomeBP);
			Assert.Equal("Bar", layout.content.SomeProperty);
		}
	}
}
