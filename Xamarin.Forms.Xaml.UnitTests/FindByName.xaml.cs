using NUnit.Framework;

using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
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

		[TestFixture]
		public class FindByNameTests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void TestRootName(bool useCompiledXaml)
			{
				var page = new FindByName(useCompiledXaml);
				Assert.AreSame(page, ((Forms.Internals.INameScope)page).FindByName("root"));
				Assert.AreSame(page, page.FindByName<FindByName>("root"));
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TestName(bool useCompiledXaml)
			{
				var page = new FindByName(useCompiledXaml);
				Assert.AreSame(page.label0, page.FindByName<Label>("label0"));
			}
		}
	}
}