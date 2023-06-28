using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui11833 : ContentPage
	{
		public Maui11833() => InitializeComponent();
		public Maui11833(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[Test]
			public void xStaticBinding([Values(false, true)] bool useCompiledXaml)
			{
				var page = new Maui11833(useCompiledXaml);
			}
		}
	}

}