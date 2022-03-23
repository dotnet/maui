using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui2304
	{
		public Maui2304() => InitializeComponent();
		public Maui2304(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[Test]
			public void GenericBaseType([Values(false, true)] bool useCompiledXaml)
			{
				var page = new Maui2304(useCompiledXaml);
				Assert.That(page.Arg, Is.EqualTo("System.String"));
			}
		}
	}

	public class Maui2304Base<T>
	{
		public string Arg => typeof(T).ToString();
	}
}