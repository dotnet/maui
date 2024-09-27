using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using UIKit;
#if MACCATALYST
using AppKit;
#endif

namespace Microsoft.Maui.Platform
{
	public static partial class WindowExtensions
	{
		internal static void UpdateTitle(this UIWindow platformWindow, IWindow window)
		{
			// If you set the title to null the app will crash
			// If you set it to an empty string the title reverts back to the 
			// default app title.
			if (OperatingSystem.IsIOSVersionAtLeast(13) && platformWindow.WindowScene is not null)
				platformWindow.WindowScene.Title = window.Title ?? String.Empty;
		}

#if MACCATALYST
		internal static void UpdateTitleBar(this UIWindow platformWindow, IWindow window, IMauiContext? mauiContext)
		{
			if (mauiContext is null)
			{
				return;
			}

			var mauiTitleBar = window.TitleBar?.ToPlatform(mauiContext);
			var platformTitleBar = platformWindow?.WindowScene?.Titlebar;

			if (mauiTitleBar is null || platformTitleBar is null)
			{
				return;
			}
			
			// remove the platform ToolBar
			platformTitleBar.Toolbar = null;
			platformTitleBar.TitleVisibility = UITitlebarTitleVisibility.Hidden;

			var w = window.Content;
			var rootView = platformWindow?.RootViewController?.View;
			if (rootView is null)
			{
				return;
			}


			// rootView.AddSubview(mauiTitleBar);

			// // Disable autoresizing mask translation to use Auto Layout
			// mauiTitleBar.TranslatesAutoresizingMaskIntoConstraints = false;

			// // Set Auto Layout constraints
			// NSLayoutConstraint.ActivateConstraints(new[]
			// {
			// 	mauiTitleBar.TopAnchor.ConstraintEqualTo(rootView.TopAnchor),
			// 	mauiTitleBar.LeadingAnchor.ConstraintEqualTo(rootView.LeadingAnchor),
			// 	mauiTitleBar.TrailingAnchor.ConstraintEqualTo(rootView.TrailingAnchor),
			// 	mauiTitleBar.HeightAnchor.ConstraintEqualTo(200) // Set the desired height
			// 	// mauiTitleBar.HeightAnchor.ConstraintEqualTo(mauiTitleBar.HeightRequest == -1 ? 50 : mauiTitleBar.HeightRequest) // Set the desired height
			// });

			// // Adjust the existing subviews to move them down
			// foreach (var subview in rootView.Subviews)
			// {
			// 	if (subview == mauiTitleBar)
			// 	{
			// 		continue;
			// 	}

			// 	subview.TranslatesAutoresizingMaskIntoConstraints = false;

			// 	// Adjust the top constraint of each subview
			// 	NSLayoutConstraint.ActivateConstraints(new[]
			// 	{
			// 		subview.TopAnchor.ConstraintEqualTo(mauiTitleBar.BottomAnchor)
			// 	});
			// }






			// var stackView = new UIStackView();

			// var row1 = new UIStackView();
			// var row2 = new UIStackView();

			// // Add cells to row stack views
			// row1.AddArrangedSubview(mauiTitleBar);
			// row2.AddArrangedSubview(rootView);

			// // Add row stack views to main stack view
			// stackView.AddArrangedSubview(row1);
			// stackView.AddArrangedSubview(row2);

			// foreach (var subview in rootView.Subviews)
			// {
			// 	subview.RemoveFromSuperview();
			// }
			
			
			// // Add stackView to the view hierarchy
			// rootView.AddSubview(stackView);

			// // Set constraints to position the stackView
			// stackView.TranslatesAutoresizingMaskIntoConstraints = false;
			// NSLayoutConstraint.ActivateConstraints(new[]
			// {
			// 	stackView.TopAnchor.ConstraintEqualTo(rootView.TopAnchor),
			// 	stackView.LeadingAnchor.ConstraintEqualTo(rootView.LeadingAnchor),
			// 	stackView.TrailingAnchor.ConstraintEqualTo(rootView.TrailingAnchor),
			// 	stackView.BottomAnchor.ConstraintEqualTo(rootView.SafeAreaLayoutGuide.BottomAnchor),
			// });



			// var stackView = new UIStackView
			// {
			// 	Axis = UILayoutConstraintAxis.Vertical,
			// 	Distribution = UIStackViewDistribution.Fill,
			// 	Alignment = UIStackViewAlignment.Fill,
			// 	Spacing = 0
			// };

			// var row1 = new UIStackView
			// {
			// 	Axis = UILayoutConstraintAxis.Horizontal,
			// 	Distribution = UIStackViewDistribution.Fill,
			// 	Alignment = UIStackViewAlignment.Fill,
			// 	Spacing = 0
			// };

			// var row2 = new UIStackView
			// {
			// 	Axis = UILayoutConstraintAxis.Horizontal,
			// 	Distribution = UIStackViewDistribution.Fill,
			// 	Alignment = UIStackViewAlignment.Fill,
			// 	Spacing = 0
			// };

			// // Add cells to row stack views
			// row1.AddArrangedSubview(mauiTitleBar);

			// // Create a container view for the original content of rootView
			// var contentView = new UIView();
			// foreach (var subview in rootView.Subviews)
			// {
			// 	subview.RemoveFromSuperview();
			// 	contentView.AddSubview(subview);
			// }

			// // Add contentView to row2
			// row2.AddArrangedSubview(contentView);

			// // Add row stack views to main stack view
			// stackView.AddArrangedSubview(row1);
			// stackView.AddArrangedSubview(row2);

			// // Add stackView to the view hierarchy
			// rootView.AddSubview(stackView);

			// // Set Auto Layout constraints for stackView
			// stackView.TranslatesAutoresizingMaskIntoConstraints = false;
			// NSLayoutConstraint.ActivateConstraints(new[]
			// {
			// 	stackView.TopAnchor.ConstraintEqualTo(rootView.TopAnchor),
			// 	stackView.LeadingAnchor.ConstraintEqualTo(rootView.LeadingAnchor),
			// 	stackView.TrailingAnchor.ConstraintEqualTo(rootView.TrailingAnchor),
			// 	stackView.BottomAnchor.ConstraintEqualTo(rootView.SafeAreaLayoutGuide.BottomAnchor)
			// });

			// // Set Auto Layout constraints for contentView
			// contentView.TranslatesAutoresizingMaskIntoConstraints = false;
			// NSLayoutConstraint.ActivateConstraints(new[]
			// {
			// 	contentView.WidthAnchor.ConstraintEqualTo(stackView.WidthAnchor),
			// 	// contentView.HeightAnchor.ConstraintEqualTo(stackView.HeightAnchor, multiplier: 1.0f, constant: -60) // Adjust height based on mauiTitleBar height
			// 	contentView.HeightAnchor.ConstraintEqualTo(stackView.HeightAnchor, multiplier: 1.0f, constant: -200) // Adjust height based on mauiTitleBar height
			// });
		}
#endif

		internal static void UpdateX(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateUnsupportedCoordinate(window);

		internal static void UpdateY(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateUnsupportedCoordinate(window);

		internal static void UpdateWidth(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateUnsupportedCoordinate(window);

		internal static void UpdateHeight(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateUnsupportedCoordinate(window);

		internal static void UpdateUnsupportedCoordinate(this UIWindow platformWindow, IWindow window) =>
			window.FrameChanged(platformWindow.Bounds.ToRectangle());

		public static void UpdateMaximumWidth(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateMaximumSize(window.MaximumWidth, window.MaximumHeight);

		public static void UpdateMaximumHeight(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateMaximumSize(window.MaximumWidth, window.MaximumHeight);

		public static void UpdateMaximumSize(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateMaximumSize(window.MaximumWidth, window.MaximumHeight);

		internal static void UpdateMaximumSize(this UIWindow platformWindow, double width, double height)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(13))
				return;

			var restrictions = platformWindow.WindowScene?.SizeRestrictions;
			if (restrictions is null)
				return;

			if (!Primitives.Dimension.IsExplicitSet(width) || !Primitives.Dimension.IsMaximumSet(width))
				width = double.MaxValue;
			if (!Primitives.Dimension.IsExplicitSet(height) || !Primitives.Dimension.IsMaximumSet(height))
				height = double.MaxValue;

			restrictions.MaximumSize = new CoreGraphics.CGSize(width, height);
		}

		public static void UpdateMinimumWidth(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateMinimumSize(window.MinimumWidth, window.MinimumHeight);

		public static void UpdateMinimumHeight(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateMinimumSize(window.MinimumWidth, window.MinimumHeight);

		public static void UpdateMinimumSize(this UIWindow platformWindow, IWindow window) =>
			platformWindow.UpdateMinimumSize(window.MinimumWidth, window.MinimumHeight);

		internal static void UpdateMinimumSize(this UIWindow platformWindow, double width, double height)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(13))
				return;

			var restrictions = platformWindow.WindowScene?.SizeRestrictions;
			if (restrictions is null)
				return;

			if (!Primitives.Dimension.IsExplicitSet(width) || !Primitives.Dimension.IsMinimumSet(width))
				width = 0;
			if (!Primitives.Dimension.IsExplicitSet(height) || !Primitives.Dimension.IsMinimumSet(height))
				height = 0;

			restrictions.MinimumSize = new CoreGraphics.CGSize(width, height);
		}

		internal static IWindow? GetHostedWindow(this UIWindow? uiWindow)
		{
			if (uiWindow is null)
				return null;

			var windows = WindowExtensions.GetWindows();
			foreach (var window in windows)
			{

				if (window.Handler?.PlatformView is UIWindow win)
				{
					if (win == uiWindow)
						return window;
				}
			}

			return null;
		}

		public static float GetDisplayDensity(this UIWindow uiWindow) =>
			(float)(uiWindow.Screen?.Scale ?? new nfloat(1.0f));

		internal static DisplayOrientation GetOrientation(this IWindow? window) =>
			DeviceDisplay.Current.MainDisplayInfo.Orientation;
	}

#if MACCATALYST
	// Delegate for managing toolbar items
#pragma warning disable RS0016 // Add public types and members to the declared API
	public class MyToolbarDelegate : NSToolbarDelegate
	{
#pragma warning disable MEM0002
		public UIView? TitleBar { get; set; }
#pragma warning restore MEM0002

		public override NSToolbarItem WillInsertItem(NSToolbar toolbar, string itemIdentifier, bool willBeInserted)
		{
			NSToolbarItem toolbarItem = new NSToolbarItem(itemIdentifier);

			switch (itemIdentifier)
			{
				case "AddItem":
					toolbarItem.Label = "Add";
					toolbarItem.ToolTip = "Secret Add";
					// toolbarItem.Image = NSImage.GetSystemSymbol("dotnet_bot.png", null);
					toolbarItem.Image = LoadImage("mario.png");
					// toolbarItem.Activated += (sender, e) => Console.WriteLine("Add clicked");
					break;
				default:
					toolbarItem.Label = "Edit";
					toolbarItem.ToolTip = "Secret Edit";
					// toolbarItem.Image = NSImage.GetSystemSymbol("dotnet_bot.png", null);
					toolbarItem.Image = LoadImage("mario.png");
					// toolbarItem.Activated += (sender, e) => Console.WriteLine("Edit clicked");
					break;
			}


			// Convert MAUI View to Native UIView
			// var mauiToolbar = new MyMauiCustomToolbar(); // Your custom MAUI toolbar
			var mauiToolbar = TitleBar;
			
			// var nsToolbarView = new AppKit.NSView
			// {
			// 	// Attach the native view to NSView or wrap it inside an NSHostingView
			// 	// Frame = new CoreGraphics.CGRect(0, 0, nativeToolbar.Frame.Width, nativeToolbar.Frame.Height)
			// 	Frame = new CoreGraphics.CGRect(0, 0, 10,10)
			// };

			// // Convert the UIView to an NSView
			// nsToolbarView.AddSubview(nativeToolbar);

			// Set the toolbar item view to the native MAUI toolbar view
			// toolbarItem.View = TitleBar;

			if (OperatingSystem.IsMacCatalystVersionAtLeast(16) && TitleBar is UIView titlebarView)
			{
				var identifier = new Foundation.NSString(itemIdentifier);
				var customViewItem = new NSUIViewToolbarItem(identifier, titlebarView);
				// var entry = new UITextField(){ Text = "TJ Text From WindowExt" };
				// var customViewItem = new NSUIViewToolbarItem(identifier, entry);

				// titlebarView.Frame = new CoreGraphics.CGRect(0, 0, toolbar.Bounds.Width, toolbar.Bounds.Height);



				// titlebarView.TranslatesAutoresizingMaskIntoConstraints = false;

				// // Set constraints to fill the toolbar
				// NSLayoutConstraint.ActivateConstraints(new[]
				// {
				// 	titlebarView.WidthAnchor.ConstraintEqualTo(toolbar.WidthAnchor, 1.0f),
				// 	titlebarView.HeightAnchor.ConstraintEqualTo(toolbar.HeightAnchor),
				// });

				// titlebarView.TranslatesAutoresizingMaskIntoConstraints = false;

				// NSLayoutConstraint.ActivateConstraints(new[]
				// {
				// 	titlebarView.WidthAnchor.ConstraintEqualTo(toolbarItem.View.Superview.WidthAnchor),
				// 	titlebarView.HeightAnchor.ConstraintEqualTo(toolbarItem.View.Superview.HeightAnchor)
				// });

				// return customViewItem;
			}

			return toolbarItem;
		}


		private static NSImage LoadImage(string imageName)
        {
            var imagePath = Foundation.NSBundle.MainBundle.PathForResource(imageName, null);
            return new NSImage(imagePath);
        }


		public override string[] DefaultItemIdentifiers(NSToolbar toolbar)
		{
			// return new[] { "AddItem", "NSToolbarFlexibleSpaceItem", "EditItem" };
			return new[] { "AddItem", "Edit", "number3", "number4" };
		}

		public override string[] AllowedItemIdentifiers(NSToolbar toolbar)
		{
			// return new[] { "AddItem", "EditItem", "NSToolbarFlexibleSpaceItem" };
			return new[] { "AddItem", "Edit", "number3", "number4" };
			// return Array.Empty<string>();
		}
	}
#endif


}
