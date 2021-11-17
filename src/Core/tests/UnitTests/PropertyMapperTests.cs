using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core, TestCategory.PropertyMapping)]
	public class PropertyMapperTests
	{
		[Fact]
		public void ChainingMappersOverrideBase()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;
			var mapper1 = new PropertyMapper<IView>
			{
				[nameof(IView.Background)] = (r, v) => wasMapper1Called = true
			};

			var mapper2 = new PropertyMapper<IButton>(mapper1)
			{
				[nameof(IView.Background)] = (r, v) => wasMapper2Called = true
			};

			mapper2.UpdateProperties(null, new Button());

			Assert.False(wasMapper1Called);
			Assert.True(wasMapper2Called);
		}

		[Fact]
		public void ChainingMappersWorks()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;
			var mapper1 = new PropertyMapper<IView>
			{
				[nameof(IView.Background)] = (r, v) => wasMapper1Called = true
			};

			var mapper2 = new PropertyMapper<ITextButton>(mapper1)
			{
				[nameof(ITextButton.TextColor)] = (r, v) => wasMapper2Called = true
			};

			mapper2.UpdateProperties(null, new Button());

			Assert.True(wasMapper1Called);
			Assert.True(wasMapper2Called);
		}


		[Fact]
		public void ConstructorChainingMappersWorks()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;
			var mapper1 = new PropertyMapper<IView>
			{
				[nameof(IView.Background)] = (r, v) => wasMapper1Called = true
			};

			var mapper2 = new PropertyMapper<ITextButton>()
			{
				[nameof(ITextButton.TextColor)] = (r, v) => wasMapper2Called = true
			};


			new PropertyMapper<ITextButton>(mapper2, mapper1)
				.UpdateProperties(null, new Button());

			Assert.True(wasMapper1Called);
			Assert.True(wasMapper2Called);
		}

		[Fact]
		public void ConstructorChainingMappersOverrideBase()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;
			var mapper1 = new PropertyMapper<IView>
			{
				[nameof(IView.Background)] = (r, v) => wasMapper1Called = true
			};

			var mapper2 = new PropertyMapper<IButton>()
			{
				[nameof(IView.Background)] = (r, v) => wasMapper2Called = true
			};

			new PropertyMapper<ITextButton>(mapper2, mapper1)
				.UpdateProperties(null, new Button());

			Assert.False(wasMapper1Called);
			Assert.True(wasMapper2Called);
		}


		[Fact]
		public void ChainingMappersStillAllowReplacingChainedRoot()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;
			bool wasMapper3Called = false;
			var mapper1 = new PropertyMapper<IView>
			{
				[nameof(IView.Background)] = (r, v) => wasMapper1Called = true
			};

			var mapper2 = new PropertyMapper<ITextButton>(mapper1)
			{
				[nameof(ITextButton.TextColor)] = (r, v) => wasMapper2Called = true
			};

			mapper1[nameof(IView.Background)] = (r, v) => wasMapper3Called = true;

			mapper2.UpdateProperties(null, new Button());

			Assert.False(wasMapper1Called, "Mapper 1 was called");
			Assert.True(wasMapper2Called, "Mapper 2 was called");
			Assert.True(wasMapper3Called, "Mapper 3 was called");
		}

		[Fact]
		public void GenericMappersWorks()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;
			var mapper1 = new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IView.Background)] = (r, v) => wasMapper1Called = true
			};

			var mapper2 = new PropertyMapper<IButton, ButtonHandler>(mapper1)
			{
				[nameof(ITextStyle.TextColor)] = (r, v) => wasMapper2Called = true
			};

			mapper2.UpdateProperties(null, new Button());

			Assert.True(wasMapper1Called);
			Assert.True(wasMapper2Called);
		}
	}
}
