using Microsoft.Maui.Controls;

namespace Controls.Xaml.UnitTests.ExternalAssembly;

/// <summary>
/// A real extension container in another assembly. It declares no Text, so naming it for one is an error
/// rather than a fallback to Label.Text.
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
/// Not an extension container at all: a plain static class whose members merely have the shape the C#
/// compiler gives to lowered extension property accessors.
/// </summary>
public static class LookalikeExtensions
{
	public static string get_Lookalike(Label label) => label.AutomationId;

	public static void set_Lookalike(Label label, string value) => label.AutomationId = value;
}
