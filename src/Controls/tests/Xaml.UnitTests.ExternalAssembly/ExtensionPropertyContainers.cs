using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace Controls.Xaml.UnitTests.ExternalAssembly;

/// <summary>
/// Extension container declared in another assembly, so that the XAML inflators are exercised against
/// metadata symbols/definitions rather than symbols coming from the compilation being generated.
/// </summary>
public static class ExternalLabelExtensions
{
	extension(Label label)
	{
		public string ExternalTag
		{
			get => label.StyleId;
			set => label.StyleId = value;
		}
	}
}

/// <summary>
/// Not visible from the test assembly. Used to verify the inflators refuse inaccessible containers
/// instead of reaching the (public) lowered accessor methods by reflection.
/// </summary>
static class InternalLabelExtensions
{
	extension(Label label)
	{
		public string InternalTag
		{
			get => label.AutomationId;
			set => label.AutomationId = value;
		}
	}
}
