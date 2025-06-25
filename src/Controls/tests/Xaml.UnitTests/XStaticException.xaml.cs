using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class XStaticException : ContentPage
	{
		public XStaticException()
		{
			InitializeComponent();
		}

		public XStaticException(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			//{x:Static Member=prefix:typeName.staticMemberName}
			//{x:Static prefix:typeName.staticMemberName}

			//The code entity that is referenced must be one of the following:
			// - A constant
			// - A static property
			// - A field
			// - An enumeration value
			// All other cases should throw

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void ThrowOnInstanceProperty(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws(new BuildExceptionConstraint(7, 6), () => MockCompiler.Compile(typeof(XStaticException)));
				else
					Assert.Throws(new XamlParseExceptionConstraint(7, 6), () => new XStaticException(useCompiledXaml));
			}
		}
	}
}