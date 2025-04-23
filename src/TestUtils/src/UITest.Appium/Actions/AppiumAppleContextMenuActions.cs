using System.Drawing;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Interactions;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumAppleContextMenuActions : ICommandExecutionGroup
	{
		const string ActivateContextMenuCommand = "activateContextMenu";
		const string DismissContextMenuCommand = "dismissContextMenu";

		protected readonly AppiumApp _app;
		readonly List<string> _commands = new()
		{
			ActivateContextMenuCommand,
			DismissContextMenuCommand,
		};

		public AppiumAppleContextMenuActions(AppiumApp app)
		{
			_app = app;
		}

		public bool IsCommandSupported(string commandName)
		{
			return _commands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
		}

		public CommandResponse Execute(string commandName, IDictionary<string, object> parameters)
		{
			return commandName switch
			{
				ActivateContextMenuCommand => ActivateContextMenu(parameters),
				DismissContextMenuCommand => DismissContextMenu(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		protected CommandResponse ActivateContextMenu(IDictionary<string, object> parameters)
		{
			parameters.TryGetValue("element", out var element);

			if (element is null)
				return CommandResponse.FailedEmptyResponse;

			var target = element as IUIElement;

			if (target is not null)
			{
				var rect = target.GetRect();
				var rootViewRect = GetRootViewRect(_app);

				var centerX = rect.Width / 2;
				var centerY = rect.Height / 2;
				var width = Math.Max(250, rect.Width);

				if ((rect.X + width) > rootViewRect.Width)
				{
					width = rootViewRect.Width - rect.X;
				}

				int fromX = (int)(rect.X + (0.95f * width));
				int fromY = centerY;
				int toX = (int)(rect.X + (0.05f * width));
				int toY = centerY;

				OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
				var dragSequence = new ActionSequence(touchDevice, 0);
				dragSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, fromX, fromY, TimeSpan.Zero));
				dragSequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
				dragSequence.AddAction(touchDevice.CreatePointerMove(CoordinateOrigin.Viewport, toX, toY, TimeSpan.FromMilliseconds(250)));
				dragSequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
				_app.Driver.PerformActions([dragSequence]);

				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
		}

		protected CommandResponse DismissContextMenu(IDictionary<string, object> parameters)
		{
			try
			{
				var rootViewRect = GetRootViewRect(_app);

				var centerX = rootViewRect.Width / 2;
				var centerY = rootViewRect.Height / 2;

				_app.TapCoordinates(centerX, centerY);

				return CommandResponse.SuccessEmptyResponse;
			}
			catch
			{
				return CommandResponse.FailedEmptyResponse;
			}
		}

		static AppiumElement? GetAppiumElement(object element)
		{
			if (element is AppiumElement appiumElement)
			{
				return appiumElement;
			}
			else if (element is AppiumDriverElement driverElement)
			{
				return driverElement.AppiumElement;
			}

			return null;
		}

		static Rectangle GetRootViewRect(AppiumApp app)
		{
			var rootElement = app.FindElement(AppiumQuery.ByXPath("/*"));
			var rootViewRect = rootElement.GetRect();

			return rootViewRect;
		}
	}
}