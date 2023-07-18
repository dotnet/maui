using System;
using System.Linq;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	public static partial class AssertionExtensions
	{
		public static async Task WaitForKeyboardToShow(this UIView view, int timeout = 1000)
		{
			var result = await Wait(() => KeyboardAutoManagerScroll.IsKeyboardShowing, timeout);
			Assert.True(result);
		}

		public static async Task WaitForKeyboardToHide(this UIView view, int timeout = 1000)
		{
			var result = await Wait(() => !KeyboardAutoManagerScroll.IsKeyboardShowing, timeout);
			Assert.True(result);
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
			if (!view.IsFocused())
			{
				await Wait(() => view.IsFocused(), timeout);
			}

			Assert.True(view.IsFocused());
		}

		public static async Task WaitForUnFocused(this UIView view, int timeout = 1000)
		{
			if (view.IsFocused())
			{
				await Wait(() => view.IsFocused(), timeout);
			}

			Assert.False(view.IsFocused());
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
			currentView.AddSubview(view);

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
				Assert.Equal(expectedColor, cap, new ColorComparison());

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

			var value = text.GetAttribute(UIStringAttributeKey.KerningAdjustment, 0, out var range);
			if (value == null)
				return 0;

			Assert.Equal(0, range.Location);
			Assert.Equal(text.Length, range.Length);

			var kerning = Assert.IsType<NSNumber>(value);

			return kerning.DoubleValue;
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
			var item = uINavigationBar.FindDescendantView<UIView>(result =>
			{
				return result.Class.Name?.Contains("UIButtonBarButton", StringComparison.OrdinalIgnoreCase) == true;
			});

			return item is not null;
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
	}
}