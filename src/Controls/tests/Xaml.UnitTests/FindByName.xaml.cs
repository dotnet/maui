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
		}
		public class FindByNameTests
		{
			[Theory]
			[InlineData(true)]
			public void TestRootName(bool useCompiledXaml)
			{
				var page = new FindByName(useCompiledXaml);
				Assert.AreSame(page, ((Maui.Controls.Internals.INameScope)page).FindByName("root"));
				Assert.AreSame(page, page.FindByName<FindByName>("root"));
			}

			[Theory]
			[InlineData(true)]
			public void TestName(bool useCompiledXaml)
			{
				var page = new FindByName(useCompiledXaml);
				Assert.AreSame(page.label0, page.FindByName<Label>("label0"));
			}
		}
	}
}