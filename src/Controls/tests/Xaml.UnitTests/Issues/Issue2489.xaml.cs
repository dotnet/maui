using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue2489 : ContentPage
{
	public Issue2489() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void DataTriggerTargetType(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Issue2489));
			}
			var layout = new Issue2489(inflator);
			Assert.NotNull(layout.wimage);
			Assert.NotNull(layout.wimage.Triggers);
			Assert.True(layout.wimage.Triggers.Any());
			Assert.IsType<DataTrigger>(layout.wimage.Triggers[0]);
			var trigger = (DataTrigger)layout.wimage.Triggers[0];
			Assert.Equal(typeof(WImage), trigger.TargetType);
		}
	}
}

public class WImage : View
{
	public ImageSource Source { get; set; }
	public Aspect Aspect { get; set; }
}