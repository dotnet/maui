using Xunit;

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

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void GenericsIssue(XamlInflator inflator)
		{
			var layout = new Gh1497(inflator);
			Assert.IsType<Gh1497EntryValidationBehavior<Entry>>(layout.entry.Behaviors[0]);
		}
	}
}
