using Microsoft.Maui.Controls;

namespace Controls.Xaml.UnitTests.ExternalAssembly.UsingOnlyExtensions;

public static class UsingOnlyExtensions
{
	extension(Label label)
	{
		public string UsingOnlyTag
		{
			set => label.AutomationId = value;
		}
	}
}
