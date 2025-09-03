using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

abstract class Gh1497BaseValidationBehavior<TBindable, TModel> : Behavior<TBindable> where TBindable : BindableObject
{
}

sealed class Gh1497EntryValidationBehavior<TModel> : Gh1497BaseValidationBehavior<Entry, TModel>
{
}

public partial class Gh1497 : ContentPage
{
	public Gh1497()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[Test]
		public void GenericsIssue([Values] XamlInflator inflator)
		{
			var layout = new Gh1497(inflator);
			Assert.That(layout.entry.Behaviors[0], Is.TypeOf(typeof(Gh1497EntryValidationBehavior<Entry>)));
		}
	}
}
