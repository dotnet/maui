using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using AActivity = Android.App.Activity;
using AApplication = Android.App.Application;
using ASoftInput = Android.Views.SoftInput;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Application)]
	public partial class ApplicationTests : ControlsHandlerTestBase
	{
		[Category(TestCategory.Application)]
		[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
		public class SoftInputModeTests : ControlsHandlerTestBase
		{
			[Fact]
			public async Task SoftInputModeDefaultsToAdjustPan()
			{
				await RunTests((_, windowHandler) =>
				{
					// Validate that the Soft Input initializes to AdjustPan
					Assert.Equal(ASoftInput.AdjustPan, windowHandler.LastASoftInputSet);
					return Task.CompletedTask;
				});
			}

			[Fact]
			public async Task SoftInputModeSetOnApplicationPropagatesToWindowHandlers()
			{
				await RunTests((app, windowHandler) =>
				{
					// Set to Resize
					Controls.PlatformConfiguration.AndroidSpecific.Application.SetWindowSoftInputModeAdjust(
						app,
						Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust.Resize);

					// Validate the mapper on the window handler is called with correct value
					Assert.Equal(ASoftInput.AdjustResize, windowHandler.LastASoftInputSet);

					// Set to Pan
					Controls.PlatformConfiguration.AndroidSpecific.Application.SetWindowSoftInputModeAdjust(
						app,
						Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust.Pan);

					// Validate the mapper on the window handler is called with correct value
					Assert.Equal(ASoftInput.AdjustPan, windowHandler.LastASoftInputSet);
					return Task.CompletedTask;
				});
			}


			async Task RunTests(Func<SoftInputModeApplication, SoftInputWindowHandlerStub, Task> runTests)
			{
				EnsureHandlerCreated((builder) =>
				{
					builder.Services.AddSingleton<IApplication>((_) => new SoftInputModeApplication());
					builder.ConfigureMauiHandlers(handler =>
					{
						handler.AddHandler<SoftInputModeWindow, SoftInputWindowHandlerStub>();
						handler.AddHandler<SoftInputModeApplication, SoftInputApplicationHandlerStub>();
					});
				});

				await InvokeOnMainThreadAsync(async () =>
				{
					var handlers = new List<IElementHandler>();

					try
					{
						// Setup application stub
						var app = MauiContext.Services.GetService<IApplication>() as SoftInputModeApplication;
						app.Handler = app.ToHandler(MauiContext);

						handlers.Add(app.Handler);

						// Setup window
						var windowHandler = (SoftInputWindowHandlerStub)app.Window.ToHandler(MauiContext);
						app.Window.Handler = windowHandler;

						handlers.Insert(0, app.Window.Handler);

						await runTests(app, windowHandler);
					}
					finally
					{
						foreach (var handler in handlers)
						{
							handler.DisconnectHandler();
						}
					}
				});
			}

			class SoftInputApplicationHandlerStub : ApplicationHandler
			{
				public SoftInputApplicationHandlerStub() : base(ApplicationHandler.Mapper)
				{
				}

				protected override AApplication CreatePlatformElement()
				{
					return new AApplication();
				}
			}

			class SoftInputModeApplication : Application, IApplication
			{
				public SoftInputModeWindow Window { get; } = new SoftInputModeWindow();

				public SoftInputModeApplication() : base(false)
				{
					Window.Parent = this;
				}

				IReadOnlyList<IWindow> IApplication.Windows
				{
					get
					{
						return new List<IWindow>() { Window };
					}
				}
			}

			class SoftInputModeWindow : Window
			{

			}

			class SoftInputWindowHandlerStub : ElementHandler<IWindow, AActivity>, IWindowHandler
			{
				public ASoftInput LastASoftInputSet => PlatformView?.Window?.Attributes?.SoftInputMode ??
					ASoftInput.AdjustUnspecified;

				public static IPropertyMapper<IWindow, SoftInputWindowHandlerStub> StubMapper =
					new PropertyMapper<IWindow, SoftInputWindowHandlerStub>()
					{
						[Controls.PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty.PropertyName] = MapWindowSoftInputModeAdjust
					};

				public static void MapWindowSoftInputModeAdjust(SoftInputWindowHandlerStub arg1, IWindow arg2)
				{
					Window.MapWindowSoftInputModeAdjust(arg1, arg2);
				}

				public SoftInputWindowHandlerStub() : base(StubMapper, null)
				{

				}

				protected override AActivity CreatePlatformElement()
				{
					return MauiProgramDefaults.DefaultContext.GetActivity();
				}
			}
		}
	}
}
