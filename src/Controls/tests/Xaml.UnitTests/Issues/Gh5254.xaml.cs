using System.Collections.Generic;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh5254VM
{
	public string Title { get; set; }
	public List<Gh5254VM> Answer { get; set; }
}

public partial class Gh5254 : ContentPage
{
	public Gh5254() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void BindToIntIndexer(XamlInflator inflator)
		{
			var layout = new Gh5254(inflator)
			{
				BindingContext = new Gh5254VM
				{
					Answer = new List<Gh5254VM> {
						new() { Title = "Foo"},
						new() { Title = "Bar"},
					}
				}
			};
			Assert.Equal("Foo", layout.label.Text);
		}
	}
}
