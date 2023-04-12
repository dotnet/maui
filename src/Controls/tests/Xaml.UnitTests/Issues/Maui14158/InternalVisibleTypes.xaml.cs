using Microsoft.Maui.Controls.Xaml.UnitTests.Issues.Maui14158;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.Maui14158;

public partial class InternalVisibleTypes : ContentPage
{
	public InternalVisibleTypes()
	{
		InitializeComponent();
	}

	public InternalVisibleTypes(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Tests
	{
		[TestCase(true)]
		[TestCase(false)]
		public void VerifyCorrectTypesUsed(bool useCompiledXaml)
		{
			if (useCompiledXaml)
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(InternalVisibleTypes)));

			var page = new InternalVisibleTypes(useCompiledXaml);

			Assert.IsInstanceOf<InternalButVisible>(page.internalButVisible);
		}
	}
}
