using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;
using Xunit.Sdk;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.DeviceTests
{
	public static partial class AssertionExtensions
	{
		public static async Task SendValueToKeyboard(this AView view, char value, int timeout = 1000)
		{
			await view.ShowKeyboardForView(timeout);

			// I tried various permutations of KeyEventActions to set the keyboard in upper case
			// But I wasn't successful
			if (Enum.TryParse($"{value}".ToUpper(), out Keycode result))
			{
				view.OnCreateInputConnection(new EditorInfo())?
					.SendKeyEvent(new KeyEvent(10, 10, KeyEventActions.Down, result, 0));

				view.OnCreateInputConnection(new EditorInfo())?
					.SendKeyEvent(new KeyEvent(10, 10, KeyEventActions.Up, result, 0));
			}
		}

		public static async Task SendKeyboardReturnType(this AView view, ReturnType returnType, int timeout = 1000)
		{
			await view.ShowKeyboardForView(timeout);

			view
				.OnCreateInputConnection(new EditorInfo())?
				.PerformEditorAction(returnType.ToPlatform());

			// Let the action propagate
			await Task.Delay(10);
		}

		public static async Task WaitForFocused(this AView view, int timeout = 1000, string message = "")
		{
			try
			{
				if (view is SearchView searchView)
				{
					var queryEditor = searchView.GetFirstChildOfType<EditText>();

					if (queryEditor is null)
						throw new Exception("Unable to locate EditText on SearchView");

					view = queryEditor;
				}

				if (!view.IsFocused)
				{
					TaskCompletionSource focusSource = new TaskCompletionSource();
					view.FocusChange += OnFocused;

					try
					{
						await focusSource.Task.WaitAsync(TimeSpan.FromMilliseconds(timeout));
					}
					catch
					{
						view.FocusChange -= OnFocused;

						if (!view.IsFocused)
							throw;
					}

					// Even though the event fires focus hasn't fully been achieved
					await Task.Yield();

					void OnFocused(object? sender, AView.FocusChangeEventArgs e)
					{
						if (!e.HasFocus)
							return;

						view.FocusChange -= OnFocused;
						focusSource.SetResult();
					}
				}
			}
			catch (Exception ex)
			{
				if (!string.IsNullOrEmpty(message))
					throw new Exception(message, ex);
				else
					throw;
			}
		}

		public static async Task WaitForUnFocused(this AView view, int timeout = 1000)
		{
			if (view.IsFocused)
			{
				TaskCompletionSource focusSource = new TaskCompletionSource();
				view.FocusChange += OnUnFocused;

				try
				{
					await focusSource.Task.WaitAsync(TimeSpan.FromMilliseconds(timeout));
				}
				catch
				{
					view.FocusChange -= OnUnFocused;

					if (view.IsFocused)
						throw;
				}

				await focusSource.Task.WaitAsync(TimeSpan.FromMilliseconds(timeout));

				// Even though the event fires unfocus hasn't fully been achieved
				await Task.Delay(10);

				void OnUnFocused(object? sender, AView.FocusChangeEventArgs e)
				{
					if (e.HasFocus)
						return;

					view.FocusChange -= OnUnFocused;
					focusSource.SetResult();
				}
			}
		}

		public static Task FocusView(this AView view, int timeout = 1000)
		{
			if (!view.IsFocused)
			{
				view.Focus(new FocusRequest(view.IsFocused));
				return view.WaitForFocused(timeout);
			}

			return Task.CompletedTask;
		}

		public static async Task ShowKeyboardForView(this AView view, int timeout = 1000, string message = "")
		{
			if (KeyboardManager.IsSoftKeyboardVisible(view))
				return;

			try
			{
				await view.FocusView(timeout);
				if (!KeyboardManager.ShowKeyboard(view))
					throw new Exception("ShowKeyboard operation failed");

				await view.WaitForKeyboardToShow(timeout);
			}
			catch (Exception ex)
			{
				if (!string.IsNullOrEmpty(message))
					throw new Exception(message, ex);
				else
					throw;
			}
		}

		public static async Task HideKeyboardForView(this AView view, int timeout = 1000, string? message = null)
		{
			if (!KeyboardManager.IsSoftKeyboardVisible(view))
				return;

			try
			{
				if (!KeyboardManager.HideKeyboard(view))
					throw new Exception("HideKeyboard operation failed");

				await view.WaitForKeyboardToHide(timeout);
			}
			catch (Exception ex)
			{
				if (!string.IsNullOrEmpty(message))
					throw new Exception(message, ex);
				else
					throw;
			}
		}

		public static async Task WaitForKeyboardToShow(this AView view, int timeout = 1000, string message = "")
		{
			try
			{
				var result = await Wait(() => KeyboardManager.IsSoftKeyboardVisible(view), timeout);
				Assert.True(result);

				// Even if the OS is reporting that the keyboard has opened it seems like the animation hasn't quite finished
				// If you try to call hide too quickly after showing, sometimes it will just show and then pop back down.
				await Task.Delay(100);
			}
			catch (Exception ex)
			{
				if (!string.IsNullOrEmpty(message))
					throw new Exception(message, ex);
				else
					throw;
			}
		}

		public static async Task WaitForKeyboardToHide(this AView view, int timeout = 1000)
		{
			var result = await Wait(() => !KeyboardManager.IsSoftKeyboardVisible(view), timeout);
			Assert.True(result);

			// Even if the OS is reporting that the keyboard has closed it seems like the animation hasn't quite finished
			// If you try to call hide too quickly after showing, sometimes it will just hide and then pop back up.
			await Task.Delay(100);
		}

		public static Task<bool> WaitForLayout(AView view, int timeout = 1000)
		{
			var tcs = new TaskCompletionSource<bool>();

			view.LayoutChange += OnLayout;
			var cts = new CancellationTokenSource();
			cts.Token.Register(() => OnLayout(view), true);
			cts.CancelAfter(timeout);

			return tcs.Task;

			void OnLayout(object? sender = null, AView.LayoutChangeEventArgs? e = null)
			{
				var view = (AView)sender!;

				if (view.Handle != IntPtr.Zero)
					view.LayoutChange -= OnLayout;

				// let the layout resolve after changing
				tcs.TrySetResult(e != null);
			}
		}

		public static string ToBase64String(this Bitmap bitmap)
		{
			using var ms = new MemoryStream();
			bitmap.Compress(Bitmap.CompressFormat.Png, 0, ms);
			return Convert.ToBase64String(ms.ToArray());
		}

		public static string CreateColorAtPointError(this Bitmap bitmap, AColor expectedColor, int x, int y) =>
			CreateColorError(bitmap, $"Expected {expectedColor} at point {x},{y} in renderered view.");

		public static string CreateColorError(this Bitmap bitmap, string message) =>
			$"{message} This is what it looked like:<img>{bitmap.ToBase64String()}</img>";

		public static string CreateEqualError(this Bitmap bitmap, Bitmap other, string message) =>
			$"{message} This is what it looked like: <img>{bitmap.ToBase64String()}</img> and <img>{other.ToBase64String()}</img>";

		public static AColor ColorAtPoint(this Bitmap bitmap, int x, int y, bool includeAlpha = false)
		{
			int pixel = bitmap.GetPixel(x, y);

			int red = AColor.GetRedComponent(pixel);
			int blue = AColor.GetBlueComponent(pixel);
			int green = AColor.GetGreenComponent(pixel);

			if (includeAlpha)
			{
				int alpha = AColor.GetAlphaComponent(pixel);
				return AColor.Argb(alpha, red, green, blue);
			}
			else
			{
				return AColor.Rgb(red, green, blue);
			}
		}

		public static Task AttachAndRun(this AView view, Action action) =>
			view.AttachAndRun(() =>
			{
				action();
				return Task.FromResult(true);
			});

		public static Task<T> AttachAndRun<T>(this AView view, Func<T> action) =>
			view.AttachAndRun(() =>
			{
				var result = action();
				return Task.FromResult(result);
			});

		public static Task AttachAndRun(this AView view, Func<Task> action) =>
			view.AttachAndRun(async () =>
			{
				await action();
				return true;
			});

		// Android doesn't handle adding and removing views in parallel very well
		// If a view is removed while a different test triggers a layout then you hit
		// a NRE exception
		static SemaphoreSlim _attachAndRunSemaphore = new SemaphoreSlim(1);
		public static async Task<T> AttachAndRun<T>(this AView view, Func<Task<T>> action)
		{
			if (view.Parent is WrapperView wrapper)
				view = wrapper;

			if (view.Parent == null)
			{
				var context = view.Context!;
				var layout = new FrameLayout(context)
				{
					LayoutParameters = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
				};
				view.LayoutParameters = new FrameLayout.LayoutParams(view.Width, view.Height)
				{
					Gravity = GravityFlags.Center
				};

				var act = context.GetActivity()!;
				var rootView = act.FindViewById<FrameLayout>(Android.Resource.Id.Content)!;

				view.Id = AView.GenerateViewId();
				layout.Id = AView.GenerateViewId();

				try
				{
					await _attachAndRunSemaphore.WaitAsync();
					layout.AddView(view);
					rootView.AddView(layout);
					return await Run(view, action);
				}
				finally
				{
					rootView.RemoveView(layout);
					layout.RemoveView(view);
					_attachAndRunSemaphore.Release();
				}
			}
			else
			{
				return await Run(view, action);
			}

			static async Task<T> Run(AView view, Func<Task<T>> action)
			{
				await Task.WhenAll(
					WaitForLayout(view),
					Wait(() => view.Width > 0 && view.Height > 0));

				return await action();
			}
		}

		public static Task<Bitmap> ToBitmap(this AView view) =>
			view.AttachAndRun(() =>
			{
				if (view.Parent is WrapperView wrapper)
					view = wrapper;

				var bitmap = Bitmap.CreateBitmap(view.Width, view.Height, Bitmap.Config.Argb8888!)!;
				using (var canvas = new Canvas(bitmap))
				{
					view.Draw(canvas);
				}
				return bitmap;
			});

		public static Bitmap AssertColorAtPoint(this Bitmap bitmap, AColor expectedColor, int x, int y)
		{
			var actualColor = bitmap.ColorAtPoint(x, y);

			if (!actualColor.IsEquivalent(expectedColor))
				Assert.Equal(expectedColor, actualColor);

			return bitmap;
		}

		public static Bitmap AssertColorAtCenter(this Drawable drawable, AColor expectedColor)
		{
			var bitmapDrawable = Assert.IsType<BitmapDrawable>(drawable);
			var bitmap = bitmapDrawable.Bitmap;
			Assert.NotNull(bitmap);

			return bitmap!.AssertColorAtCenter(expectedColor);
		}

		public static Bitmap AssertColorAtCenter(this Bitmap bitmap, AColor expectedColor)
		{
			return bitmap.AssertColorAtPoint(expectedColor, bitmap.Width / 2, bitmap.Height / 2);
		}

		public static Bitmap AssertColorAtBottomLeft(this Bitmap bitmap, AColor expectedColor)
		{
			return bitmap.AssertColorAtPoint(expectedColor, 0, 0);
		}

		public static Bitmap AssertColorAtBottomRight(this Bitmap bitmap, AColor expectedColor)
		{
			return bitmap.AssertColorAtPoint(expectedColor, bitmap.Width - 1, 0);
		}

		public static Bitmap AssertColorAtTopLeft(this Bitmap bitmap, AColor expectedColor)
		{
			return bitmap.AssertColorAtPoint(expectedColor, 0, bitmap.Height - 1);
		}

		public static Bitmap AssertColorAtTopRight(this Bitmap bitmap, AColor expectedColor)
		{
			return bitmap.AssertColorAtPoint(expectedColor, bitmap.Width - 1, bitmap.Height - 1);
		}

		public static Task<Bitmap> AssertContainsColor(this Bitmap bitmap, Graphics.Color expectedColor, Func<Maui.Graphics.RectF, Maui.Graphics.RectF>? withinRectModifier = null)
			=> Task.FromResult(bitmap.AssertContainsColor(expectedColor.ToPlatform()));

		public static Task<Bitmap> AssertDoesNotContainColor(this Bitmap bitmap, Graphics.Color unexpectedColor, Func<Maui.Graphics.RectF, Maui.Graphics.RectF>? withinRectModifier = null)
			=> Task.FromResult(bitmap.AssertDoesNotContainColor(unexpectedColor.ToPlatform()));

		public static Bitmap AssertContainsColor(this Bitmap bitmap, AColor expectedColor, Func<Maui.Graphics.RectF, Maui.Graphics.RectF>? withinRectModifier = null)
		{
			var imageRect = new Graphics.RectF(0, 0, bitmap.Width, bitmap.Height);

			if (withinRectModifier is not null)
				imageRect = withinRectModifier.Invoke(imageRect);

			for (int x = (int)imageRect.X; x < (int)imageRect.Width; x++)
			{
				for (int y = (int)imageRect.Y; y < (int)imageRect.Height; y++)
				{
					if (bitmap.ColorAtPoint(x, y, true).IsEquivalent(expectedColor))
					{
						return bitmap;
					}
				}
			}

			throw new XunitException($"Color {expectedColor} not found.");
		}

		public static Bitmap AssertDoesNotContainColor(this Bitmap bitmap, AColor unexpectedColor, Func<Maui.Graphics.RectF, Maui.Graphics.RectF>? withinRectModifier = null)
		{
			var imageRect = new Graphics.RectF(0, 0, bitmap.Width, bitmap.Height);

			if (withinRectModifier is not null)
				imageRect = withinRectModifier.Invoke(imageRect);

			for (int x = (int)imageRect.X; x < (int)imageRect.Width; x++)
			{
				for (int y = (int)imageRect.Y; y < (int)imageRect.Height; y++)
				{
					if (bitmap.ColorAtPoint(x, y, true).IsEquivalent(unexpectedColor))
					{
						throw new XunitException($"Color {unexpectedColor} was found at point {x}, {y}.");
					}
				}
			}

			return bitmap;
		}

		public static Task<Bitmap> AssertContainsColor(this AView view, Graphics.Color expectedColor) =>
			AssertContainsColor(view, expectedColor.ToPlatform());

		public static Task<Bitmap> AssertDoesNotContainColor(this AView view, Graphics.Color unexpectedColor) =>
			AssertDoesNotContainColor(view, unexpectedColor.ToPlatform());

		public static async Task<Bitmap> AssertContainsColor(this AView view, AColor expectedColor)
		{
			var bitmap = await view.ToBitmap();
			return AssertContainsColor(bitmap, expectedColor);
		}

		public static async Task<Bitmap> AssertDoesNotContainColor(this AView view, AColor unexpectedColor)
		{
			var bitmap = await view.ToBitmap();
			return AssertDoesNotContainColor(bitmap, unexpectedColor);
		}

		public static async Task<Bitmap> AssertColorAtPointAsync(this AView view, AColor expectedColor, int x, int y)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtPoint(expectedColor, x, y);
		}

		public static async Task<Bitmap> AssertColorAtCenterAsync(this AView view, AColor expectedColor)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtCenter(expectedColor);
		}

		public static async Task<Bitmap> AssertColorAtBottomLeft(this AView view, AColor expectedColor)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtBottomLeft(expectedColor);
		}

		public static async Task<Bitmap> AssertColorAtBottomRight(this AView view, AColor expectedColor)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtBottomRight(expectedColor);
		}

		public static async Task<Bitmap> AssertColorAtTopLeft(this AView view, AColor expectedColor)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtTopLeft(expectedColor);
		}

		public static async Task<Bitmap> AssertColorAtTopRight(this AView view, AColor expectedColor)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtTopRight(expectedColor);
		}

		public static Task AssertEqualAsync(this Bitmap bitmap, Bitmap other)
		{
			Assert.NotNull(bitmap);
			Assert.NotNull(other);

			Assert.Equal(new Size(bitmap.Width, bitmap.Height), new Size(other.Width, other.Height));

			Assert.True(IsMatching(bitmap, other), CreateEqualError(bitmap, other, $"Images did not match."));

			return Task.CompletedTask;
		}

		static bool IsMatching(Bitmap bitmap1, Bitmap bitmap2)
		{
			for (int x = 0; x < bitmap1.Width; x++)
			{
				for (int y = 0; y < bitmap1.Height; y++)
				{
					var first = bitmap1.ColorAtPoint(x, y, true);
					var second = bitmap2.ColorAtPoint(x, y, true);

					if (!first.IsEquivalent(second))
						return false;
				}
			}

			return true;
		}

		public static Task AssertNotEqualAsync(this Bitmap bitmap, Bitmap other)
		{
			Assert.NotNull(bitmap);
			Assert.NotNull(other);

			Assert.NotEqual(new Size(bitmap.Width, bitmap.Height), new Size(other.Width, other.Height));

			if (IsMatching(bitmap, other))
			{
				throw new XunitException(CreateEqualError(bitmap, other, $"Images did not match."));
			}

			return Task.CompletedTask;
		}

		public static TextUtils.TruncateAt? ToPlatform(this LineBreakMode mode) =>
			mode switch
			{
				LineBreakMode.NoWrap => null,
				LineBreakMode.WordWrap => null,
				LineBreakMode.CharacterWrap => null,
				LineBreakMode.HeadTruncation => TextUtils.TruncateAt.Start,
				LineBreakMode.TailTruncation => TextUtils.TruncateAt.End,
				LineBreakMode.MiddleTruncation => TextUtils.TruncateAt.Middle,
				_ => throw new ArgumentOutOfRangeException(nameof(mode))
			};

		public static FontWeight GetFontWeight(this Typeface typeface) =>
			OperatingSystem.IsAndroidVersionAtLeast(28)
				? (FontWeight)typeface.Weight
				: typeface.IsBold ? FontWeight.Bold : FontWeight.Regular;

		public static bool IsAccessibilityElement(this AView view) =>
			view.GetSemanticPlatformElement().IsImportantForAccessibility;


		public static bool IsExcludedWithChildren(this AView view) =>
			view.GetSemanticPlatformElement().ImportantForAccessibility == ImportantForAccessibility.NoHideDescendants;
	}
}
