using System.Linq;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh3821 : ContentPage
{
	public Gh3821() => InitializeComponent();

	string _text;
	public string Text
	{
		get => _text;
		set
		{
			_text = value;
			OnPropertyChanged();
		}
	}

	[TestFixture]
	class Tests
	{
		[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Test]
		public void NoConflictsInNamescopes([Values] XamlInflator inflator)
		{
			var layout = new Gh3821(inflator) { Text = "root" };
			var label0 = (Label)((Gh3821View)((StackLayout)layout.Content).Children[0]).Content;
			Assert.That(label0.Text, Is.EqualTo("root"));
		}
	}
}