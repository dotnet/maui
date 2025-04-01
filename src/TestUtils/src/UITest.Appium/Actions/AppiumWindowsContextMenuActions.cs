using System.Drawing;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Interactions;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumWindowsContextMenuActions : ICommandExecutionGroup
	{
		const string ActivateContextMenuCommand = "activateContextMenu";
		const string DismissContextMenuCommand = "dismissContextMenu";

		protected readonly AppiumApp _app;
		readonly List<string> _commands = new()
		{
			ActivateContextMenuCommand,
			DismissContextMenuCommand,
		};

		public AppiumWindowsContextMenuActions(AppiumApp app)
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

			var appiumElement = GetAppiumElement(element);

			if (appiumElement is not null)
			{
				OpenQA.Selenium.Appium.Interactions.PointerInputDevice touchDevice = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
				var longPress = new ActionSequence(touchDevice, 0);

				longPress.AddAction(touchDevice.CreatePointerMove(appiumElement, 0, 0, TimeSpan.FromMilliseconds(0)));
				longPress.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
				longPress.AddAction(touchDevice.CreatePointerMove(appiumElement, 0, 0, TimeSpan.FromMilliseconds(2000)));
				longPress.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
				_app.Driver.PerformActions(new List<ActionSequence> { longPress });

				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
		}

		protected CommandResponse DismissContextMenu(IDictionary<string, object> parameters)
		{
			try
			{
				var screenBounds = GetRootViewRect(_app);

				var centerX = screenBounds.Width / 2;
				var centerY = screenBounds.Height / 2;

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