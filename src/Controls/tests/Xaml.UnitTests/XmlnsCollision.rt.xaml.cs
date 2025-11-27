using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

[assembly: XmlnsDefinition("http://companyone.com/schemas/toolkit", "CompanyOne.Controls")]
[assembly: XmlnsPrefix("http://companyone.com/schemas/toolkit", "c1")]
[assembly: XmlnsDefinition("http://companytwo.com/schemas/toolkit", "CompanyTwo.Controls")]
[assembly: XmlnsPrefix("http://companytwo.com/schemas/toolkit", "c2")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "http://companyone.com/schemas/toolkit")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "http://companytwo.com/schemas/toolkit")]

namespace CompanyOne.Controls
{
	public class ConflictingLabel : Label
	{
	}
}

namespace CompanyTwo.Controls
{
	public class ConflictingLabel : Label
	{
	}
}

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class XmlnsCollision : ContentPage
	{
		public XmlnsCollision() => InitializeComponent();

		[TestFixture]
		class Test
		{
			[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
			[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

			[Test]
			public void ConflictInXmlns([Values] XamlInflator inflator)
			{
				switch (inflator)
				{
					case XamlInflator.XamlC:
						Assert.Throws<BuildException>(() =>
						{
							MockCompiler.Compile(typeof(XmlnsCollision), out var hasLoggedErrors);
							Assert.IsTrue(hasLoggedErrors);
						});
						break;
					case XamlInflator.SourceGen:
						var result = CreateMauiCompilation()
							.WithAdditionalSource(
"""
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

[assembly: XmlnsDefinition("http://companyone.com/schemas/toolkit", "CompanyOne.Controls")]
[assembly: XmlnsPrefix("http://companyone.com/schemas/toolkit", "c1")]
[assembly: XmlnsDefinition("http://companytwo.com/schemas/toolkit", "CompanyTwo.Controls")]
[assembly: XmlnsPrefix("http://companytwo.com/schemas/toolkit", "c2")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "http://companyone.com/schemas/toolkit")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "http://companytwo.com/schemas/toolkit")]

namespace CompanyOne.Controls
{
	public class ConflictingLabel : Label
	{
	}
}

namespace CompanyTwo.Controls
{
	public class ConflictingLabel : Label
	{
	}
}

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlProcessing(XamlInflator.Runtime, true)]
	public partial class XmlnsCollision : ContentPage
	{
		public XmlnsCollision() => InitializeComponent();
	}
}
""")
							.RunMauiSourceGenerator(typeof(XmlnsCollision));
						Assert.That(result.Diagnostics.Any());
						return;
					case XamlInflator.Runtime:
						Assert.Throws<XamlParseException>(() =>
						{
							var layout = new XmlnsCollision(inflator);
						});
						break;
				}
			}
		}
	}
}