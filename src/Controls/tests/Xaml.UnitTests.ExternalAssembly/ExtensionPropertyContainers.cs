using Microsoft.Maui;
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

/// <summary>
/// Two applicable receivers for the same extension property, neither more derived than the other.
/// It lives here so the source generator tests can reference a real extension container as metadata.
/// </summary>
public static class AmbiguousExtensions
{
	extension(IView view)
	{
		public string MyAmbiguous { get => string.Empty; set { } }
	}

	extension(BindableObject bindable)
	{
		public string MyAmbiguous { get => string.Empty; set { } }
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

/// <summary>
/// The getter and the setter of an extension property may be declared in different extension blocks, with
/// different receivers and different value types. Only the setter matters when assigning from XAML.
/// </summary>
public static class SplitAccessorExtensions
{
	extension(Label label)
	{
		public int SplitAccessor => label.AutomationId?.Length ?? 0;
	}

	extension(View view)
	{
		public string SplitAccessor
		{
			set => view.AutomationId = "view:" + value;
		}
	}
}
