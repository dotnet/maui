using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17222Style : ResourceDictionary
{
	public Maui17222Style() => InitializeComponent();

	public Maui17222Style(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
}