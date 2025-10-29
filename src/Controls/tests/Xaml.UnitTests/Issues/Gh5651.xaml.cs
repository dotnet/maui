using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh5651VM : IViewModel5651
{
	public Gh5651VM() => this.SelectedItem = "test";

	public string SelectedItem { get; set; }
}

public interface IViewModel5651 : IEditViewModel5651<string> { }

public interface IEditViewModel5651<T> : IBaseViewModel5651<T> { }

public interface IBaseViewModel5651<T>
{
	T SelectedItem { get; set; }
}

public partial class Gh5651 : ContentPage
{
	public Gh5651() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void GenericBaseInterfaceResolution([Values] XamlInflator inflator)
		{
			var layout = new Gh5651(inflator) { BindingContext = new Gh5651VM() };
			Assert.That(layout.label.Text, Is.EqualTo("test"));
		}
	}
}