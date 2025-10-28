using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz60788 : ContentPage
	{
		public Bz60788()
		{
			InitializeComponent();
		}

		public Bz60788(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{

			[InlineData(true), TestCase(false)]
			public void KeyedRDWithImplicitStyles(bool useCompiledXaml)
			{
				var layout = new Bz60788(useCompiledXaml);
				Assert.Equal(2, layout.Resources.Count);
				Assert.Equal(3, ((ResourceDictionary)layout.Resources["RedTextBlueBackground"]).Count);
				Assert.Equal(3, ((ResourceDictionary)layout.Resources["BlueTextRedBackground"]).Count);
			}
		}
	}
}