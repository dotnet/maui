using System.Collections.ObjectModel;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh4438VM : Gh4438VMBase<string>
{
	public Gh4438VM()
	{
		Add("test");
		SelectedItem = this.First();
	}
}

public class Gh4438VMBase<T> : Collection<string>
{
	public virtual T SelectedItem { get; set; }
}

public partial class Gh4438 : ContentPage
{
	public Gh4438() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void GenericBaseClassResolution(XamlInflator inflator)
		{
			var layout = new Gh4438(inflator) { BindingContext = new Gh4438VM() };
			Assert.Equal("test", layout.label.Text);
		}
	}
}
