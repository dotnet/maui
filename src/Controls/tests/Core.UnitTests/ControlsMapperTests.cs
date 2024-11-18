using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[Collection("ControlsMapperTests")]
	public class ControlsMapperTests : BaseTestFixture
	{
		MauiContext SetupMauiContext()
		{
			var mauiApp1 = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<ButtonWithControlsMapper, ButtonWithControlsMapperHandler>())
				.Build();

			// reset all the mappings
			foreach (var item in ButtonWithControlsMapperHandler.Mapper.GetKeys())
			{
				ButtonWithControlsMapperHandler.Mapper[item] = ButtonWithControlsMapperHandler.MapperTest;
			}


			return new MauiContext(mauiApp1.Services);
		}

		[Fact]
		public void Append()
		{
			MauiContext mauiContext1 = SetupMauiContext();

			bool appendCalled = false;
			bool originalMapperCalled = false;
			ButtonWithControlsMapperHandler.
				Mapper
				.AppendToMapping("MapperTest", (_, _) =>
				{
					Assert.True(originalMapperCalled);
					appendCalled = true;
				});


			var button = new ButtonWithControlsMapper(MapperCalled);
			button.ToPlatform(mauiContext1);

			Assert.True(appendCalled);

			void MapperCalled()
			{
				originalMapperCalled = true;
			}
		}

		[Fact]
		public void Prepend()
		{
			MauiContext mauiContext1 = SetupMauiContext();

			bool newMapperMethodCalled = false;
			bool originalMapperCalled = false;
			ButtonWithControlsMapperHandler.
				Mapper
				.PrependToMapping("MapperTest", (_, _) =>
				{
					Assert.False(originalMapperCalled);
					newMapperMethodCalled = true;
				});


			var button = new ButtonWithControlsMapper(MapperCalled);
			button.ToPlatform(mauiContext1);

			Assert.True(newMapperMethodCalled);
			Assert.True(originalMapperCalled);

			void MapperCalled()
			{
				originalMapperCalled = true;
			}
		}

		[Fact]
		public void Replace()
		{
			MauiContext mauiContext1 = SetupMauiContext();

			bool newMapperMethodCalled = false;
			bool originalMapperCalled = false;
			ButtonWithControlsMapperHandler.Mapper.ModifyMapping("MapperTest", (handler, view, previous) =>
				{
					Assert.False(originalMapperCalled);
					previous.Invoke(handler, view);
					Assert.True(originalMapperCalled);
					newMapperMethodCalled = true;
				});


			var button = new ButtonWithControlsMapper(MapperCalled);
			button.ToPlatform(mauiContext1);

			Assert.True(newMapperMethodCalled);
			Assert.True(originalMapperCalled);

			void MapperCalled()
			{
				originalMapperCalled = true;
			}
		}

		class ButtonWithControlsMapper : Button
		{

			public Action MapperCalled { get; private set; }
			public ButtonWithControlsMapper(Action mapperCalled)
			{
				MapperCalled = mapperCalled;
			}
		}

		class ButtonWithControlsMapperHandler : ViewHandler<ButtonWithControlsMapper, object>
		{

			public static PropertyMapper<IView> Mapper = new PropertyMapper<IView>()
			{
				["MapperTest"] = MapperTest
			};

			public static void MapperTest(IElementHandler handler, IView view)
			{
				((ButtonWithControlsMapper)view).MapperCalled.Invoke();
			}

			public ButtonWithControlsMapperHandler() : base(Mapper)
			{
			}

			protected override object CreatePlatformView()
			{
				return new object();
			}
		}
	}
}
