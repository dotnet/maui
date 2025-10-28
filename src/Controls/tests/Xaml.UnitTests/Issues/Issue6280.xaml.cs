using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Issue6280 : ContentPage
	{
		public Issue6280() => InitializeComponent();
		public Issue6280(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[Theory]
			public void Method([InlineData(false, true)] bool useCompiledXaml)
			{
				var vm = new Issue6280ViewModel();
				var page = new Issue6280(useCompiledXaml) { BindingContext = vm };
				page._entry.SetValueFromRenderer(Entry.TextProperty, 1);
				Assert.Equal(vm.NullableInt, 1);
			}
		}
	}

	public class Issue6280ViewModel
	{
		public int? NullableInt { get; set; }
	}
}