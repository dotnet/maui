using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class FindByName : ContentPage
	{
		public FindByName()
		{
			InitializeComponent();
		}

		public FindByName(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class FindByNameTests
		{
			[InlineData(false)]
			[InlineData(true)]
			public void TestRootName(bool useCompiledXaml)
			{
				var page = new FindByName(useCompiledXaml);
				Assert.Same(page, ((Maui.Controls.Internals.INameScope)page).FindByName("root"));
				Assert.Same(page, page.FindByName<FindByName>("root"));
			}

			[InlineData(false)]
			[InlineData(true)]
			public void TestName(bool useCompiledXaml)
			{
				var page = new FindByName(useCompiledXaml);
				Assert.Same(page.label0, page.FindByName<Label>("label0"));
			}
		}
	}
}