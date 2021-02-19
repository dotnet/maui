using System;
using Xamarin.Forms;
using Xamarin.Platform.Handlers.UnitTests;
using Xunit;
using Fact = Xamarin.Platform.Handlers.UnitTests.FactAttribute;

namespace Xamarin.Platform.Handlers.Tests
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
				[nameof(IView.BackgroundColor)] = (r, v) => wasMapper1Called = true
			};

			var mapper2 = new PropertyMapper<IButton>(mapper1)
			{
				[nameof(IView.BackgroundColor)] = (r, v) => wasMapper2Called = true
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
				[nameof(IView.BackgroundColor)] = (r, v) => wasMapper1Called = true
			};

			var mapper2 = new PropertyMapper<IButton>(mapper1)
			{
				[nameof(IButton.TextColor)] = (r, v) => wasMapper2Called = true
			};

			mapper2.UpdateProperties(null, new Button());

			Assert.True(wasMapper1Called);
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
				[nameof(IView.BackgroundColor)] = (r, v) => wasMapper1Called = true
			};

			var mapper2 = new PropertyMapper<IButton>(mapper1)
			{
				[nameof(IButton.TextColor)] = (r, v) => wasMapper2Called = true
			};

			mapper1[nameof(IView.BackgroundColor)] = (r, v) => wasMapper3Called = true;

			mapper2.UpdateProperties(null, new Button());

			Assert.False(wasMapper1Called, "Mapper 1 was called");
			Assert.True(wasMapper2Called, "Mapper 2 was called");
			Assert.True(wasMapper3Called, "Mapper 3 was called");
		}

		[Fact]
		public void MappersActionsAreNotCalledOnUpdateProperties()
		{
			bool wasMapper1Called = false;
			bool mapperActionWasCalled = false;
			const string mapperActionKey = "Fire";
			var mapper1 = new PropertyMapper<IView>
			{
				[nameof(IView.BackgroundColor)] = (r, v) => wasMapper1Called = true,
				Actions = {
					[mapperActionKey] = (r, v) => mapperActionWasCalled = true,
				}
			};
			mapper1.UpdateProperties(null, new Button());

			Assert.True(wasMapper1Called);
			Assert.False(mapperActionWasCalled);

			mapper1.UpdateProperty(null, new Button(), mapperActionKey);
			Assert.True(mapperActionWasCalled);
		}

		[Fact]
		public void ChainedMapperActionsRespectNewUpdate()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;
			bool mapperActionWasCalled = false;
			const string mapperActionKey = "Fire";
			var mapper1 = new PropertyMapper<IView>
			{
				[nameof(IView.BackgroundColor)] = (r, v) => wasMapper1Called = true,
				Actions = {
					[mapperActionKey] = (r, v) => mapperActionWasCalled = true,
				}
			};

			var mapper2 = new PropertyMapper<IButton>(mapper1)
			{
				[mapperActionKey] = (r, v) => wasMapper2Called = true
			};

			Assert.Equal(2, mapper1.Keys.Count);
			Assert.Equal(1, mapper1.ActionKeys.Count);
			Assert.Equal(1, mapper1.UpdateKeys.Count);

			Assert.Equal(2, mapper2.Keys.Count);
			Assert.Equal(0, mapper2.ActionKeys.Count);
			Assert.Equal(2, mapper2.UpdateKeys.Count);

			mapper2.UpdateProperties(null, new Button());

			Assert.True(wasMapper1Called);
			Assert.False(mapperActionWasCalled);
			Assert.True(wasMapper2Called);
		}



		[Fact]
		public void GenericMappersWorks()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;
			var mapper1 = new PropertyMapper<IView, IViewHandler>
			{
				[nameof(IView.BackgroundColor)] = (r, v) => wasMapper1Called = true
			};

			var mapper2 = new PropertyMapper<IButton, ButtonHandler>(mapper1)
			{
				[nameof(IButton.TextColor)] = (r, v) => wasMapper2Called = true
			};

			mapper2.UpdateProperties(null, new Button());

			Assert.True(wasMapper1Called);
			Assert.True(wasMapper2Called);
		}
	}
}
