using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh5486VM : IGh5486VM
{
	public Gh5486VM()
	{
		this.Data = new Tag5486()
		{
			Label = "test",
		};
	}

	public ITag5486 Data { get; set; }
}

public class Tag5486 : ITag5486
{
	public string Label { get; set; }
}

public interface ITag5486
{
	string Label { get; set; }
}

public interface IGh5486VM : IGh5486VMBase<ITag5486>
{
}

public interface IGh5486VMBase<T>
{
	T Data { get; }
}

public partial class Gh5486 : ContentPage
{
	public Gh5486() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void GenericBaseInterfaceResolution(XamlInflator inflator)
		{
			var layout = new Gh5486(inflator) { BindingContext = new Gh5486VM() };
			Assert.Equal("test", layout.label.Text);
		}
	}
}