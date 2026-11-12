using Microsoft.Maui.Controls;

// Same CLR namespace as containers the Xaml.UnitTests assembly maps to the MAUI xmlns, but that mapping only
// covers the Xaml.UnitTests assembly, so this container is not in scope.
namespace Microsoft.Maui.Controls.Xaml.UnitTests.ScopedExtensions;

public static class LeakedExtensions
{
	extension(Label label)
	{
		public string LeakedTag
		{
			set => label.StyleId = value;
		}
	}
}
