using System;
using System.Linq;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using Xunit.Sdk;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public static partial class AssertionExtensions
	{
		public static async Task WaitForKeyboardToShow(this UIView view, int timeout = 1000)
		{
			await AssertEventually(() => KeyboardAutoManagerScroll.IsKeyboardShowing, timeout: timeout, message: $"Timed out waiting for {view} to show keyboard");
		}

		public static async Task WaitForKeyboardToHide(this UIView view, int timeout = 1000)
		{
			await AssertEventually(() => !KeyboardAutoManagerScroll.IsKeyboardShowing, timeout: timeout, message: $"Timed out waiting for {view} to hide keyboard");
		}

		public static Task SendValueToKeyboard(this UIView view, char value, int timeout = 1000)
		{
			throw new NotImplementedException();
		}

		public static Task SendKeyboardReturnType(this UIView view, ReturnType returnType, int timeout = 1000)
		{
			throw new NotImplementedException();
		}

		public static async Task WaitForFocused(this UIView view, int timeout = 1000)
		{
			await AssertEventually(view.IsFocused, timeout: timeout, message: $"Timed out waiting for {view} to become focused");
		}

		public static async Task WaitForUnFocused(this UIView view, int timeout = 1000)
		{
			await AssertEventually(() => !view.IsFocused(), timeout: timeout, message: $"Timed out waiting for {view} to become unfocused");
		}

		static bool IsFocused(this UIView view) => view.Focused || view.IsFirstResponder;

		public static Task FocusView(this UIView view, int timeout = 1000)
		{
			view.Focus(new FocusRequest());
			return WaitForFocused(view, timeout);
		}

		public static Task ShowKeyboardForView(this UIView view, int timeout = 1000)
		{
			throw new NotImplementedException();
		}

		public static Task HideKeyboardForView(this UIView view, int timeout = 1000, string? message = null)
		{
			throw new NotImplementedException();
		}

		public static string CreateColorAtPointError(this UIImage bitmap, UIColor expectedColor, int x, int y) =>
			CreateColorError(bitmap, $"Expected {expectedColor} at point {x},{y} in renderered view.");

		public static string CreateColorError(this UIImage bitmap, string message) =>
			$"{message} This is what it looked like:<img>{bitmap.ToBase64String()}</img>";

		public static string CreateEqualError(this UIImage bitmap, UIImage other, string message) =>
			$"{message} This is what it looked like: <img>{bitmap.ToBase64String()}</img> and <img>{other.ToBase64String()}</img>";

		public static string CreateScreenshotError(this UIImage bitmap, string message) =>
			$"{message} This is what it looked like:<img>{bitmap.ToBase64String()}</img>";

		public static string ToBase64String(this UIImage bitmap)
		{
			var data = bitmap.AsPNG();

			ArgumentNullException.ThrowIfNull(data);

			return data.GetBase64EncodedString(NSDataBase64EncodingOptions.None);
		}

		public static Task AttachAndRun(this UIView view, Action action) =>
			view.AttachAndRun(() =>
			{
				action();
				return Task.FromResult(true);
			});

		public static Task<T> AttachAndRun<T>(this UIView view, Func<T> action) =>
			view.AttachAndRun(() =>
			{
				var result = action();
				return Task.FromResult(result);
			});

		public static Task AttachAndRun(this UIView view, Func<Task> action) =>
			view.AttachAndRun(async () =>
			{
				await action();
				return true;
			});

		public static async Task<T> AttachAndRun<T>(this UIView view, Func<Task<T>> action)
		{
			var currentView = FindContentView();

			// MauiView has optimization code that won't fire a remeasure of the child view
			// Check LayoutSubviews inside Mauiveiw.cs for more details. 
			// If the parent is a MauiView, the expectation is that the parent will call
			// measure on all the children. But this view that we're "attaching" is unknown to MauiView
			// so the optimization code causes the attached view to not remeasure when it actually should. 
			// So we add a UIView in the middle to force our attached view to not optimize itself and actually
			// remeasure when requested
			// This middle view is also helpful so we can make sure the attached view isn't inside the safe area
			// which can have some unexpected results
			var safeAreaInsets = currentView.SafeAreaInsets;
			var attachedView = new UIView()
			{
				Frame = new CGRect(
					safeAreaInsets.Right,
					safeAreaInsets.Top,
					currentView.Frame.Width - safeAreaInsets.Right,
					currentView.Frame.Height - safeAreaInsets.Top)
			};

			attachedView.AddSubview(view);
			currentView.AddSubview(attachedView);

			// Give the UI time to refresh
			await Task.Delay(100);

			T result;

			try
			{
				result = await action();
			}
			finally
			{
				view.RemoveFromSuperview();
				attachedView.RemoveFromSuperview();

				// Give the UI time to refresh
				await Task.Delay(100);
			}

			return result;
		}

		public static UIViewController FindContentViewController()
		{
			if (GetKeyWindow(UIApplication.SharedApplication) is not UIWindow window)
			{
				throw new InvalidOperationException("Could not attach view - unable to find UIWindow");
			}

			if (window.RootViewController is not UIViewController viewController)
			{
				throw new InvalidOperationException("Could not attach view - unable to find RootViewController");
			}

			while (viewController.PresentedViewController is not null)
			{
				if (viewController is ModalWrapper || viewController.PresentedViewController is ModalWrapper)
					throw new InvalidOperationException("Modal Window Is Still Present");

				viewController = viewController.PresentedViewController;
			}

			if (viewController == null)
			{
				throw new InvalidOperationException("Could not attach view - unable to find presented ViewController");
			}

			if (viewController is UINavigationController nav)
			{
				viewController = nav.VisibleViewController;
			}

			return viewController;
		}

		public static UIView FindContentView()
		{
			var currentView = FindContentViewController().View;

			if (currentView == null)
			{
				throw new InvalidOperationException("Could not attach view - unable to find visible view");
			}

			var attachParent = currentView.FindDescendantView<ContentView>() as UIView;

			if (attachParent == null)
			{
				attachParent = currentView.FindDescendantView<UIView>();
			}

			return attachParent ?? currentView;
		}

		public static Task<UIImage> ToBitmap(this UIView view, IMauiContext mauiContext)
		{
			if (view.Superview is WrapperView wrapper)
				view = wrapper;


			var imageRect = new CGRect(0, 0, view.Frame.Width, view.Frame.Height);

			if (view.Frame.Width == 0 && view.Frame.Height == 0)
			{
				UIGraphicsImageRenderer renderer = new UIGraphicsImageRenderer(imageRect.Size);
				return Task.FromResult(renderer.CreateImage(c =>
				{
					view.Layer.RenderInContext(c.CGContext);
				}));
			}

			UIGraphics.BeginImageContext(imageRect.Size);

			var context = UIGraphics.GetCurrentContext();
			view.Layer.RenderInContext(context);
			var image = UIGraphics.GetImageFromCurrentImageContext();

			UIGraphics.EndImageContext();

			return Task.FromResult(image);
		}

		public static UIColor ColorAtPoint(this UIImage bitmap, int x, int y)
		{
			var pixel = bitmap.GetPixel(x, y);

			var color = new UIColor(
				pixel[0] / 255.0f,
				pixel[1] / 255.0f,
				pixel[2] / 255.0f,
				pixel[3] / 255.0f);

			return color;
		}

		public static byte[] GetPixel(this UIImage bitmap, int x, int y)
		{
			var cgImage = bitmap.CGImage!;
			var width = cgImage.Width;
			var height = cgImage.Height;
			var colorSpace = CGColorSpace.CreateDeviceRGB();
			var bitsPerComponent = 8;
			var bytesPerRow = 4 * width;
			var componentCount = 4;

			var dataBytes = new byte[width * height * componentCount];

			using var context = new CGBitmapContext(
				dataBytes,
				width, height,
				bitsPerComponent, bytesPerRow,
				colorSpace,
				CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast);

			context.DrawImage(new CGRect(0, 0, width, height), cgImage);

			var pixelLocation = (bytesPerRow * y) + componentCount * x;

			var pixel = new byte[]
			{
				dataBytes[pixelLocation],
				dataBytes[pixelLocation + 1],
				dataBytes[pixelLocation + 2],
				dataBytes[pixelLocation + 3],
			};

			return pixel;
		}

		public static UIImage AssertColorAtPoint(this UIImage bitmap, UIColor expectedColor, int x, int y, double? tolerance = null)
		{
			var cap = bitmap.ColorAtPoint(x, y);

			if (!ColorComparison.ARGBEquivalent(cap, expectedColor, tolerance))
				throw new XunitException(CreateColorAtPointError(bitmap, expectedColor, x, y));

			return bitmap;
		}

		public static UIImage AssertColorAtCenter(this UIImage bitmap, UIColor expectedColor)
		{
			AssertColorAtPoint(bitmap, expectedColor, (int)bitmap.Size.Width / 2, (int)bitmap.Size.Height / 2);
			return bitmap;
		}

		public static UIImage AssertColorAtBottomLeft(this UIImage bitmap, UIColor expectedColor)
		{
			return bitmap.AssertColorAtPoint(expectedColor, 0, 0);
		}

		public static UIImage AssertColorAtBottomRight(this UIImage bitmap, UIColor expectedColor)
		{
			return bitmap.AssertColorAtPoint(expectedColor, (int)bitmap.Size.Width - 1, 0);
		}

		public static UIImage AssertColorAtTopLeft(this UIImage bitmap, UIColor expectedColor)
		{
			return bitmap.AssertColorAtPoint(expectedColor, 0, (int)bitmap.Size.Height - 1);
		}

		public static UIImage AssertColorAtTopRight(this UIImage bitmap, UIColor expectedColor)
		{
			return bitmap.AssertColorAtPoint(expectedColor, (int)bitmap.Size.Width - 1, (int)bitmap.Size.Height - 1);
		}

		public static async Task<UIImage> AssertColorAtPointAsync(this UIView view, UIColor expectedColor, int x, int y, IMauiContext mauiContext)
		{
			var bitmap = await view.ToBitmap(mauiContext);
			return bitmap.AssertColorAtPoint(expectedColor, x, y);
		}

		public static async Task<UIImage> AssertColorsAtPointsAsync(this UIView view, Graphics.Color[] colors, Graphics.Point[] points, IMauiContext mauiContext)
		{
			var bitmap = await view.ToBitmap(mauiContext);

			for (int i = 0; i < points.Length; i++)
			{
				bitmap.AssertColorAtPoint(colors[i].ToPlatform(), (int)points[i].X, (int)points[i].Y);
			}

			return bitmap;
		}

		public static async Task<UIImage> AssertColorAtCenterAsync(this UIView view, UIColor expectedColor, IMauiContext mauiContext)
		{
			var bitmap = await view.ToBitmap(mauiContext);
			return bitmap.AssertColorAtCenter(expectedColor);
		}

		public static async Task<UIImage> AssertColorAtBottomLeft(this UIView view, UIColor expectedColor, IMauiContext mauiContext)
		{
			var bitmap = await view.ToBitmap(mauiContext);
			return bitmap.AssertColorAtBottomLeft(expectedColor);
		}

		public static async Task<UIImage> AssertColorAtBottomRight(this UIView view, UIColor expectedColor, IMauiContext mauiContext)
		{
			var bitmap = await view.ToBitmap(mauiContext);
			return bitmap.AssertColorAtBottomRight(expectedColor);
		}

		public static async Task<UIImage> AssertColorAtTopLeft(this UIView view, UIColor expectedColor, IMauiContext mauiContext)
		{
			var bitmap = await view.ToBitmap(mauiContext);
			return bitmap.AssertColorAtTopLeft(expectedColor);
		}

		public static async Task<UIImage> AssertColorAtTopRight(this UIView view, UIColor expectedColor, IMauiContext mauiContext)
		{
			var bitmap = await view.ToBitmap(mauiContext);
			return bitmap.AssertColorAtTopRight(expectedColor);
		}

		public static async Task<UIImage> AssertContainsColor(this UIView view, UIColor expectedColor, IMauiContext mauiContext, double? tolerance = null)
		{
			var bitmap = await view.ToBitmap(mauiContext);
			return bitmap.AssertContainsColor(expectedColor, tolerance: tolerance);
		}

		public static async Task<UIImage> AssertDoesNotContainColor(this UIView view, UIColor unexpectedColor, IMauiContext mauiContext)
		{
			var bitmap = await view.ToBitmap(mauiContext);
			return bitmap.AssertDoesNotContainColor(unexpectedColor);
		}

		public static Task<UIImage> AssertContainsColor(this UIView view, Microsoft.Maui.Graphics.Color expectedColor, IMauiContext mauiContext, double? tolerance = null) =>
			AssertContainsColor(view, expectedColor.ToPlatform(), mauiContext, tolerance: tolerance);

		public static Task<UIImage> AssertDoesNotContainColor(this UIView view, Microsoft.Maui.Graphics.Color unexpectedColor, IMauiContext mauiContext) =>
			AssertDoesNotContainColor(view, unexpectedColor.ToPlatform(), mauiContext);

		public static Task<UIImage> AssertContainsColor(this UIImage image, Graphics.Color expectedColor, Func<Graphics.RectF, Graphics.RectF>? withinRectModifier = null, double? tolerance = null)
			=> Task.FromResult(image.AssertContainsColor(expectedColor.ToPlatform(), withinRectModifier, tolerance: tolerance));

		public static UIImage AssertContainsColor(this UIImage bitmap, UIColor expectedColor, Func<Graphics.RectF, Graphics.RectF>? withinRectModifier = null, double? tolerance = null)
		{
			var imageRect = new Graphics.RectF(0, 0, (float)bitmap.Size.Width.Value, (float)bitmap.Size.Height.Value);

			if (withinRectModifier is not null)
				imageRect = withinRectModifier.Invoke(imageRect);

			for (int x = (int)imageRect.X; x < (int)imageRect.Width; x++)
			{
				for (int y = (int)imageRect.Y; y < (int)imageRect.Height; y++)
				{
					if (ColorComparison.ARGBEquivalent(bitmap.ColorAtPoint(x, y), expectedColor, tolerance))
					{
						return bitmap;
					}
				}
			}

			throw new XunitException(CreateColorError(bitmap, $"Color {expectedColor} not found."));
		}

		public static UIImage AssertDoesNotContainColor(this UIImage bitmap, UIColor unexpectedColor, Func<Graphics.RectF, Graphics.RectF>? withinRectModifier = null)
		{
			var imageRect = new Graphics.RectF(0, 0, (float)bitmap.Size.Width.Value, (float)bitmap.Size.Height.Value);

			if (withinRectModifier is not null)
				imageRect = withinRectModifier.Invoke(imageRect);

			for (int x = (int)imageRect.X; x < (int)imageRect.Width; x++)
			{
				for (int y = (int)imageRect.Y; y < (int)imageRect.Height; y++)
				{
					if (ColorComparison.ARGBEquivalent(bitmap.ColorAtPoint(x, y), unexpectedColor))
					{
						throw new XunitException(CreateColorError(bitmap, $"Color {unexpectedColor} was found at point {x}, {y}."));
					}
				}
			}

			return bitmap;
		}


		public static Task AssertEqualAsync(this UIImage bitmap, UIImage other)
		{
			Assert.NotNull(bitmap);
			Assert.NotNull(other);

			Assert.Equal(bitmap.Size, other.Size);

			Assert.True(IsMatching(), CreateEqualError(bitmap, other, $"Images did not match."));

			return Task.CompletedTask;

			bool IsMatching()
			{
				for (int x = 0; x < bitmap.Size.Width; x++)
				{
					for (int y = 0; y < bitmap.Size.Height; y++)
					{
						var first = bitmap.ColorAtPoint(x, y);
						var second = other.ColorAtPoint(x, y);

						if (!ColorComparison.ARGBEquivalent(first, second))
							return false;
					}
				}
				return true;
			}
		}

		public static async Task ThrowScreenshot(this UIView view, IMauiContext mauiContext, string? message = null, Exception? ex = null)
		{
			var bitmap = await view.ToBitmap(mauiContext);
			if (ex is null)
				throw new XunitException(CreateScreenshotError(bitmap, message ?? "There was an error."));
			else
				throw new XunitException(CreateScreenshotError(bitmap, message ?? "There was an error: " + ex.Message), ex);
		}

		public static UILineBreakMode ToPlatform(this LineBreakMode mode) =>
			mode switch
			{
				LineBreakMode.NoWrap => UILineBreakMode.Clip,
				LineBreakMode.WordWrap => UILineBreakMode.WordWrap,
				LineBreakMode.CharacterWrap => UILineBreakMode.CharacterWrap,
				LineBreakMode.HeadTruncation => UILineBreakMode.HeadTruncation,
				LineBreakMode.TailTruncation => UILineBreakMode.TailTruncation,
				LineBreakMode.MiddleTruncation => UILineBreakMode.MiddleTruncation,
				_ => throw new ArgumentOutOfRangeException(nameof(mode))
			};

		public static double GetCharacterSpacing(this NSAttributedString text)
		{
			if (text == null)
				return 0;

			if (text.Length == 0)
				return 0;

			var value = text.GetAttribute(UIStringAttributeKey.KerningAdjustment, 0, out var range);
			if (value == null)
				return 0;

			Assert.Equal(0, range.Location);
			Assert.Equal(text.Length, range.Length);

			var kerning = Assert.IsType<NSNumber>(value);

			return kerning.DoubleValue;
		}

		public static double GetLineHeight(this NSAttributedString text)
		{
			if (text == null)
				return 0;

			if (text.Length == 0)
				return 0;

			var value = text.GetAttribute(UIStringAttributeKey.ParagraphStyle, 0, out var range);
			if (value == null)
				return 0;

			Assert.Equal(0, range.Location);
			Assert.Equal(text.Length, range.Length);

			var paragraphStyle = Assert.IsType<NSMutableParagraphStyle>(value);

			return paragraphStyle.LineHeightMultiple;
		}

		public static TextDecorations GetTextDecorations(this NSAttributedString text)
		{
			var textDecorations = TextDecorations.None;

			if (text == null)
				return textDecorations;

			if (text.Length == 0)
				return textDecorations;

			var valueUnderline = text.GetAttribute(UIStringAttributeKey.UnderlineStyle, 0, out var rangeUnderline);
			var valueStrikethrough = text.GetAttribute(UIStringAttributeKey.StrikethroughStyle, 0, out var rangeStrikethrough);

			Assert.Equal(0, rangeUnderline.Location);
			Assert.Equal(text.Length, rangeUnderline.Length);

			Assert.Equal(0, rangeStrikethrough.Location);
			Assert.Equal(text.Length, rangeStrikethrough.Length);

			if (NSNumber.FromInt32((int)NSUnderlineStyle.Single) == (NSNumber)valueUnderline)
			{
				textDecorations = TextDecorations.Underline;
			}
			else if (NSNumber.FromInt32((int)NSUnderlineStyle.Single) == (NSNumber)valueStrikethrough)
			{
				textDecorations = TextDecorations.Strikethrough;
			}

			return textDecorations;
		}

		public static void AssertHasUnderline(this NSAttributedString attributedString)
		{
			var value = attributedString.GetAttribute(UIStringAttributeKey.UnderlineStyle, 0, out var range);

			if (value == null)
			{
				throw new XunitException("Label does not have the UnderlineStyle attribute");
			}
		}

		public static UIColor GetForegroundColor(this NSAttributedString text)
		{
			if (text == null)
				return UIColor.Clear;

			var value = text.GetAttribute(UIStringAttributeKey.ForegroundColor, 0, out var range);

			if (value == null)
				return UIColor.Clear;

			Assert.Equal(0, range.Location);
			Assert.Equal(text.Length, range.Length);

			var kerning = Assert.IsType<UIColor>(value);

			return kerning;
		}

		public static void AssertEqual(this CATransform3D expected, CATransform3D actual, int precision = 4)
		{
			Assert.Equal((double)expected.M11, (double)actual.M11, precision);
			Assert.Equal((double)expected.M12, (double)actual.M12, precision);
			Assert.Equal((double)expected.M13, (double)actual.M13, precision);
			Assert.Equal((double)expected.M14, (double)actual.M14, precision);
			Assert.Equal((double)expected.M21, (double)actual.M21, precision);
			Assert.Equal((double)expected.M22, (double)actual.M22, precision);
			Assert.Equal((double)expected.M23, (double)actual.M23, precision);
			Assert.Equal((double)expected.M24, (double)actual.M24, precision);
			Assert.Equal((double)expected.M31, (double)actual.M31, precision);
			Assert.Equal((double)expected.M32, (double)actual.M32, precision);
			Assert.Equal((double)expected.M33, (double)actual.M33, precision);
			Assert.Equal((double)expected.M34, (double)actual.M34, precision);
			Assert.Equal((double)expected.M41, (double)actual.M41, precision);
			Assert.Equal((double)expected.M42, (double)actual.M42, precision);
			Assert.Equal((double)expected.M43, (double)actual.M43, precision);
			Assert.Equal((double)expected.M44, (double)actual.M44, precision);
		}

		static UIWindow? GetKeyWindow(UIApplication application)
		{
			if (OperatingSystem.IsIOSVersionAtLeast(15))
			{
				foreach (var scene in application.ConnectedScenes)
				{
					if (scene is UIWindowScene windowScene
						&& windowScene.ActivationState == UISceneActivationState.ForegroundActive)
					{
						foreach (var window in windowScene.Windows)
						{
#if MACCATALYST
							// When running headless (on CI or local) Mac Catalyst has trouble finding the window through the method below.
							// Added an env variable to accommodate for this and just return the first window found.
							if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("headlessrunner")))
							{	
								return window;
							}
#endif
							if (window.IsKeyWindow)
							{
								return window;
							}
						}
					}
				}

				return null;
			}

			var windows = application.Windows;

			for (int i = 0; i < windows.Length; i++)
			{
				var window = windows[i];
				if (window.IsKeyWindow)
					return window;
			}

			return null;
		}

		/// <summary>
		/// If VoiceOver is off iOS just leaves IsAccessibilityElement set to false
		/// This applies the default value to each control type so we can have some level
		/// of testing inside simulators and when the VO is turned off.
		/// These default values were all validated inside Xcode
		/// </summary>
		/// <param name="platformView"></param>
		public static void SetupAccessibilityExpectationIfVoiceOverIsOff(this UIView platformView)
		{
			if (!UIAccessibility.IsVoiceOverRunning)
			{
				platformView = platformView.GetAccessiblePlatformView();
				// even though UIStepper/UIPageControl inherits from UIControl
				// iOS sets it to not be important for accessibility
				// most likely because the children elements need to be reachable
				if (platformView is UIStepper || platformView is UIPageControl)
					return;

				// UILabel will only be an accessibility element if it has text
				if (platformView is UILabel label && !String.IsNullOrWhiteSpace(label.Text))
				{
					platformView.IsAccessibilityElement = true;
					return;
				}

				// AFAICT on iOS when you read IsAccessibilityElement it's always false
				// unless you have VoiceOver turned on.
				// So, though not ideal, the main think we test on iOS is that elements
				// that should stay false remain false. 
				// According to the Apple docs anything that inherits from UIControl
				// has isAccessibilityElement set to true by default so we're just
				// validating that everything that doesn't inherit from UIControl isn't
				// getting set to true
				if (platformView is UIControl)
				{
					platformView.IsAccessibilityElement = true;
					return;
				}

				// These are UIViews that don't inherit from UIControl but
				// iOS will mark them as Accessibility Elements
				// I tested each of these controls inside Xcode away from any MAUI tampering
				if (platformView is UITextView || platformView is UIProgressView)
				{
					platformView.IsAccessibilityElement = true;
					return;
				}
			}
		}

		public static bool IsAccessibilityElement(this UIView platformView)
		{
			platformView = platformView.GetAccessiblePlatformView();
			return platformView.IsAccessibilityElement;
		}

		public static bool IsExcludedWithChildren(this UIView platformView)
		{
			return platformView.AccessibilityElementsHidden;
		}

		public static UIView GetAccessiblePlatformView(this UIView platformView)
		{
			if (platformView is UISearchBar searchBar)
				platformView = searchBar.GetSearchTextField()!;

			if (platformView is WrapperView wrapperView)
			{
				Assert.False(wrapperView.IsAccessibilityElement);
				return wrapperView.Subviews[0];
			}

			return platformView;
		}

		public static bool HasBackButton(this UINavigationBar uINavigationBar)
		{
			var currentNavItem = uINavigationBar.Items.LastOrDefault();

			return
				uINavigationBar.BackItem is not null &&
				currentNavItem is not null &&
				currentNavItem.LeftBarButtonItem is null &&
				!currentNavItem.HidesBackButton;
		}

		public static UIView GetBackButton(this UINavigationBar uINavigationBar)
		{
			var item = uINavigationBar.FindDescendantView<UIView>(result =>
			{
				return result.Class.Name?.Contains("UIButtonBarButton", StringComparison.OrdinalIgnoreCase) == true;
			});

			return item ?? throw new Exception("Unable to locate back button view");
		}

		public static void TapBackButton(this UINavigationBar uINavigationBar)
		{
			var item = uINavigationBar.GetBackButton();

			var recognizer = item?.GestureRecognizers?.OfType<UITapGestureRecognizer>()?.FirstOrDefault();
			if (recognizer is null && item is UIControl control)
			{
				control.SendActionForControlEvents(UIControlEvent.TouchUpInside);
			}
			else
			{
				_ = recognizer ?? throw new Exception("Unable to figure out how to tap back button");
				recognizer.State = UIGestureRecognizerState.Ended;
			}
		}

		public static string? GetToolbarTitle(this UINavigationBar uINavigationBar)
		{
			var item = uINavigationBar.FindDescendantView<UIView>(result =>
			{
				return result.Class.Name?.Contains("UINavigationBarTitleControl", StringComparison.OrdinalIgnoreCase) == true;
			});

			//Pre iOS 15
			item = item ?? uINavigationBar.FindDescendantView<UIView>(result =>
			{
				return result.Class.Name?.Contains("UINavigationBarContentView", StringComparison.OrdinalIgnoreCase) == true;
			});

			_ = item ?? throw new Exception("Unable to locate TitleBar Control");

			var titleLabel = item.FindDescendantView<UILabel>();

			_ = item ?? throw new Exception("Unable to locate UILabel Inside UINavigationBar");
			return titleLabel?.Text;
		}

		public static string? GetBackButtonText(this UINavigationBar uINavigationBar)
		{
			var item = uINavigationBar.GetBackButton();

			var titleLabel = item.FindDescendantView<UILabel>();

			_ = item ?? throw new Exception("Unable to locate BackButton UILabel Inside UINavigationBar");
			return titleLabel?.Text;
		}

		static public Task AssertTabItemTextDoesNotContainColor(
			this UITabBar navigationView,
			string tabText,
			Color expectedColor,
			IMauiContext mauiContext) => AssertTabItemTextColor(navigationView, tabText, expectedColor, false, mauiContext);

		static public Task AssertTabItemTextContainsColor(
			this UITabBar navigationView,
			string tabText,
			Color expectedColor,
			IMauiContext mauiContext) => AssertTabItemTextColor(navigationView, tabText, expectedColor, true, mauiContext);

		static async Task AssertTabItemTextColor(
			this UITabBar navigationView,
			string tabText,
			Color expectedColor,
			bool hasColor,
			IMauiContext mauiContext)
		{
			var tabBarItemView = GetTabItemView(navigationView, tabText).FindDescendantView<UILabel>();
			if (tabBarItemView is null)
				throw new Exception($"Unable to locate Tab Item Icon Container: {tabText}");

			if (hasColor)
			{
				await tabBarItemView.AssertContainsColor(expectedColor, mauiContext, 0.1);
			}
			else
			{
				await tabBarItemView.AssertDoesNotContainColor(expectedColor, mauiContext);
			}
		}

		static async Task AssertTabItemIconColor(
			this UITabBar navigationView, string tabText, Color expectedColor, bool hasColor,
			IMauiContext mauiContext)
		{
			var tabBarItemView = GetTabItemView(navigationView, tabText).FindDescendantView<UIImageView>();
			if (tabBarItemView is null)
				throw new Exception($"Unable to locate Tab Item Icon Container: {tabText}");

			if (hasColor)
			{
				await tabBarItemView.AssertContainsColor(expectedColor, mauiContext);
			}
			else
			{
				await tabBarItemView.AssertDoesNotContainColor(expectedColor, mauiContext);
			}
		}

		static public Task AssertTabItemIconDoesNotContainColor(
			this UITabBar navigationView,
			string tabText,
			Color expectedColor,
			IMauiContext mauiContext) => AssertTabItemIconColor(navigationView, tabText, expectedColor, false, mauiContext);

		static public Task AssertTabItemIconContainsColor(
			this UITabBar navigationView,
			string tabText,
			Color expectedColor,
			IMauiContext mauiContext) => AssertTabItemIconColor(navigationView, tabText, expectedColor, true, mauiContext);

		static UIView GetTabItemView(this UITabBar tabBar, string tabText)
		{
			var tabBarItem = tabBar.Items?.Single(t => string.Equals(t.Title, tabText, StringComparison.OrdinalIgnoreCase));

			if (tabBarItem is null)
				throw new Exception($"Unable to find tab bar item: {tabText}");

			var tabBarItemView = tabBarItem.ValueForKey(new Foundation.NSString("view")) as UIView;

			if (tabBarItemView is null)
				throw new Exception($"Unable to find tab bar item: {tabText}");

			return tabBarItemView;
		}
	}
}