using NUnit.Framework;
using NUnit.Framework.Constraints;

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

	[TestFixture]
	class Tests
	{
		[TearDown]
		public void TearDown()
		{
			Application.Current = null;
		}

		[Test]
		public void PropertiesWithGenericType([Values] XamlInflator inflator)
		{
			var layout = new Bz53350(inflator);
			Assert.That(layout.content.SomeBP, Is.EqualTo("Foo"));
			Assert.That(layout.content.SomeProperty, Is.EqualTo("Bar"));
		}
	}
}
