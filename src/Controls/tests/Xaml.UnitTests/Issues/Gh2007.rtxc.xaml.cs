using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh2007 : ContentPage
{
	public Gh2007() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void UsefullxResourceErrorMessages(XamlInflator inflator)
		{
			if (inflator == XamlInflator.Runtime || inflator == XamlInflator.XamlC)
				Assert.Throws<XamlParseException>(() => new Gh2007(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime | XamlInflator.XamlC, true)]
public partial class Gh2007 : ContentPage
{
	public Gh2007() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Gh2007));
				Assert.NotEmpty(result.Diagnostics);
			}
			else
			// TODO: Convert to [Theory(Skip="reason")] or use conditional Skip attribute
			{
				// TODO: This branch was using NUnit Assert.Skip, needs proper handling
			}

		}
	}
}
