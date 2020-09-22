using System;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Platform.Handlers.Tests
{
	[TestFixture]
	public class PropertyMapperTests
	{
		[Test]
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

		[Test]
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
				[nameof(IButton.Color)] = (r, v) => wasMapper2Called = true
			};

			mapper2.UpdateProperties(null, new Button());

			Assert.True(wasMapper1Called);
			Assert.True(wasMapper2Called);
		}

		[Test]
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
				[nameof(IButton.Color)] = (r, v) => wasMapper2Called = true
			};

			mapper1[nameof(IView.BackgroundColor)] = (r, v) => wasMapper3Called = true;

			mapper2.UpdateProperties(null, new Button());

			Assert.False(wasMapper1Called, "Mapper 1 was called");
			Assert.True(wasMapper2Called, "Mapper 2 was called");
			Assert.True(wasMapper3Called, "Mapper 3 was called");
		}

		[Test]
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

		[Test]
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

			Assert.AreEqual(2, mapper1.Keys.Count);
			Assert.AreEqual(1, mapper1.ActionKeys.Count);
			Assert.AreEqual(1, mapper1.UpdateKeys.Count);

			Assert.AreEqual(2, mapper2.Keys.Count);
			Assert.AreEqual(0, mapper2.ActionKeys.Count);
			Assert.AreEqual(2, mapper2.UpdateKeys.Count);

			mapper2.UpdateProperties(null, new Button());

			Assert.True(wasMapper1Called);
			Assert.False(mapperActionWasCalled);
			Assert.True(wasMapper2Called);
		}



		[Test]
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
				[nameof(IButton.Color)] = (r, v) => wasMapper2Called = true
			};

			mapper2.UpdateProperties(null, new Button());

			Assert.True(wasMapper1Called);
			Assert.True(wasMapper2Called);
		}
	}
}
