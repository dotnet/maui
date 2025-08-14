using System.Linq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue2489 : ContentPage
{
	public Issue2489() => InitializeComponent();


	[TestFixture]
	class Tests
	{
		[Test]
		public void DataTriggerTargetType([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Issue2489));
			}
			var layout = new Issue2489(inflator);
			Assert.NotNull(layout.wimage);
			Assert.NotNull(layout.wimage.Triggers);
			Assert.True(layout.wimage.Triggers.Any());
			Assert.That(layout.wimage.Triggers[0], Is.TypeOf<DataTrigger>());
			var trigger = (DataTrigger)layout.wimage.Triggers[0];
			Assert.AreEqual(typeof(WImage), trigger.TargetType);
		}
	}
}

public class WImage : View
{
	public ImageSource Source { get; set; }
	public Aspect Aspect { get; set; }
}