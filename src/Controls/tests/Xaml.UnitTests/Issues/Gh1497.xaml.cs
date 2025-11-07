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


	public class Tests
	{
		[Theory]
		[Values]
		public void GenericsIssue(XamlInflator inflator)
		{
			var layout = new Gh1497(inflator);
			Assert.TypeOf(layout.entry.Behaviors[0](typeof(Gh1497EntryValidationBehavior<Entry>)));
		}
	}
}
