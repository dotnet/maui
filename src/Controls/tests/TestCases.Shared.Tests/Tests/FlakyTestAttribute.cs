using NUnit.Framework;

namespace Microsoft.Maui.TestCases.Tests;

public class FlakyTestAttribute : IgnoreAttribute
{
	public FlakyTestAttribute() : base(nameof(FlakyTestAttribute))
	{
	}
	public FlakyTestAttribute(string reason) : base(reason)
	{
	}
}