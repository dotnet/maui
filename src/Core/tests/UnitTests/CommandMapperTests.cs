using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core, TestCategory.CommandMapping)]
	public class CommandMapperTests
	{
		[Fact]
		public void ChainingMappersOverrideBase()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;
			var mapper1 = new CommandMapper<IView>
			{
				[nameof(IView.Focus)] = (r, v, a) => wasMapper1Called = true
			};

			var mapper2 = new CommandMapper<IButton>(mapper1)
			{
				[nameof(IView.Focus)] = (r, v, a) => wasMapper2Called = true
			};

			mapper2.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.False(wasMapper1Called);
			Assert.True(wasMapper2Called);
		}

		[Fact]
		public void ChainingMappersStillAllowReplacingChainedRoot()
		{
			bool wasMapper1Called = false;
			bool wasMapper3Called = false;

			var mapper1 = new CommandMapper<IView>
			{
				[nameof(IView.Focus)] = (r, v, a) => wasMapper1Called = true
			};

			var mapper2 = new CommandMapper<ITextButton>(mapper1);

			mapper1[nameof(IView.Focus)] = (r, v, a) => wasMapper3Called = true;

			mapper2.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.False(wasMapper1Called, "Mapper 1 was called");
			Assert.True(wasMapper3Called, "Mapper 3 was called");
		}

		[Fact]
		public void GenericMappersWorks()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;

			var mapper1 = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.Focus)] = (r, v, a) => wasMapper1Called = true
			};

			var mapper2 = new CommandMapper<IButton, ButtonHandler>(mapper1)
			{
				[nameof(IView.Focus)] = (r, v, a) => wasMapper2Called = true
			};

			mapper2.Invoke(null, new Button(), nameof(IView.Focus), null);

			Assert.False(wasMapper1Called);
			Assert.True(wasMapper2Called);
		}
	}
}
