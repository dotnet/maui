using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// IQualifiedMemberProbe is an interface the target implements, but DerivedOnly is declared by QualifiedMemberProbe alone.
// Resolving it through the named owner would reach a member that owner never declares.
public partial class QualifiedMemberOwnerInterface : ContentPage
{
	public QualifiedMemberOwnerInterface() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : BaseTestFixture
	{
		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.XamlC)]
		[InlineData(XamlInflator.SourceGen)]
		internal void Throw(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(QualifiedMemberOwnerInterface)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new QualifiedMemberOwnerInterface(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public interface IQualifiedMemberProbe
{
	string InterfaceOnly { get; set; }
}

public class QualifiedMemberProbeBase : View
{
	public string BaseOnly { get; set; }
}

public class QualifiedMemberProbe : QualifiedMemberProbeBase, IQualifiedMemberProbe
{
	public string DerivedOnly { get; set; }
	public string InterfaceOnly { get; set; }
}

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class QualifiedMemberOwnerInterface : ContentPage
{
	public QualifiedMemberOwnerInterface() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(QualifiedMemberOwnerInterface));
				Assert.True(result.Diagnostics.Any(d => d.Id == "MAUIX2002"));
				Assert.DoesNotContain(".DerivedOnly =", result.GeneratedInitializeComponent(), System.StringComparison.Ordinal);
			}
		}
	}
}
