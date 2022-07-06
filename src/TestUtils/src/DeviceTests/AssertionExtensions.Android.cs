using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

namespace Microsoft.Maui.DeviceTests
{
	public static partial class AssertionExtensions
	{
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

		public static Bitmap AssertContainsColor(this Bitmap bitmap, AColor expectedColor)
		{
			for (int x = 0; x < bitmap.Width; x++)
			{
				for (int y = 0; y < bitmap.Height; y++)
				{
					if (bitmap.ColorAtPoint(x, y, true).IsEquivalent(expectedColor))
					{
						return bitmap;
					}
				}
			}

			Assert.True(false, CreateColorError(bitmap, $"Color {expectedColor} not found."));
			return bitmap;
		}

		public static Task<Bitmap> AssertContainsColor(this AView view, Graphics.Color expectedColor) =>
			AssertContainsColor(view, expectedColor.ToPlatform());

		public static async Task<Bitmap> AssertContainsColor(this AView view, AColor expectedColor)
		{
			var bitmap = await view.ToBitmap();
			return AssertContainsColor(bitmap, expectedColor);
		}

		public static async Task<Bitmap> AssertColorAtPoint(this AView view, AColor expectedColor, int x, int y)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtPoint(expectedColor, x, y);
		}

		public static async Task<Bitmap> AssertColorAtCenter(this AView view, AColor expectedColor)
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

		public static Task AssertEqual(this Bitmap bitmap, Bitmap other)
		{
			Assert.NotNull(bitmap);
			Assert.NotNull(other);

			Assert.Equal(new Size(bitmap.Width, bitmap.Height), new Size(other.Width, other.Height));

			Assert.True(IsMatching(), CreateEqualError(bitmap, other, $"Images did not match."));

			return Task.CompletedTask;

			bool IsMatching()
			{
				for (int x = 0; x < bitmap.Width; x++)
				{
					for (int y = 0; y < bitmap.Height; y++)
					{
						var first = bitmap.ColorAtPoint(x, y, true);
						var second = other.ColorAtPoint(x, y, true);

						if (!first.IsEquivalent(second))
							return false;
					}
				}
				return true;
			}
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
	}
}