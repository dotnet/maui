using System.Collections.Generic;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh5254VM
{
	public string Title { get; set; }
	public List<Gh5254VM> Answer { get; set; }
}

[XamlProcessing(XamlInflator.Default, true)]
public partial class Gh5254 : ContentPage
{
	public Gh5254() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void BindToIntIndexer([Values] XamlInflator inflator)
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
			Assert.That(layout.label.Text, Is.EqualTo("Foo"));
		}
	}
}
