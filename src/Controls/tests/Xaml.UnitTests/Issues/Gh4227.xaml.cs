using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public interface IGh4227Level0
{
	string Level0 { get; }
}

public interface IGh4227Level1 : IGh4227Level0
{
	string Level1 { get; }
}

public class Gh4227VM : IGh4227Level1
{
	public string Level0 => "level0";
	public string Level1 => "level1";
}

public partial class Gh4227 : ContentPage
{
	public Gh4227() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void FindMemberOnInterfaces(XamlInflator inflator)
		{
			var layout = new Gh4227(inflator) { BindingContext = new Gh4227VM() };
			Assert.Equal("level0", layout.label0.Text);
			Assert.Equal("level1", layout.label1.Text);
		}
	}
}