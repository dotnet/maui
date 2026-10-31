using System;
using System.IO;
using System.Threading;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.ImageSources;
using Xunit;
using GtkPicture = global::Gtk.Picture;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>Animated images: GIF frames advance on the widget's frame clock while <c>IsAnimationPlaying</c>.</summary>
	public class AnimatedImageTests
	{
		[Fact]
		public void HeaderSniffRecognisesAnimatableFormatsOnly()
		{
			Assert.True(GtkImageAnimation.MayBeAnimated(TestImages.CreateAnimatedGif()));
			Assert.False(GtkImageAnimation.MayBeAnimated(TestImages.CreatePng(4, 4, 0, 0, 0)));
			Assert.False(GtkImageAnimation.MayBeAnimated(new byte[4]));
		}

		[GtkFact]
		public void AnimationDecodesAndAdvancesFrames() => GtkTestHost.Run(() =>
		{
			using var animation = GtkImageAnimation.TryCreate(TestImages.CreateAnimatedGif());
			Assert.NotNull(animation);
			var first = animation!.CurrentTexture;
			Assert.Equal(1, first.Width);

			Thread.Sleep(150);
			Assert.True(animation.Advance());
			Assert.NotSame(first, animation.CurrentTexture);
		});

		[GtkFact]
		public void StillImagesAreNotAnimations() => GtkTestHost.Run(() =>
			Assert.Null(GtkImageAnimation.TryCreate(TestImages.CreatePng(4, 4, 0, 0, 0))));

		[GtkFact]
		public void ImagePlaysAnimatedSourcesOnlyWhileIsAnimationPlaying() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();
			var data = TestImages.CreateAnimatedGif();
			var image = new Image { Source = ImageSource.FromStream(() => new MemoryStream(data)) };
			using var host = new WindowHost(new Grid { Children = { image } }, 60, 60);
			var widget = (GtkPicture)host.Widget(image);
			var animator = ((GtkImageHandler)image.Handler!).Animator!;

			Assert.True(GtkTestHost.PumpUntil(() => animator.Animation is not null && widget.Paintable is not null, host.PlatformWindow));
			var first = widget.Paintable;
			Assert.False(animator.IsTicking);
			Thread.Sleep(150);
			GtkTestHost.Pump(host.PlatformWindow, 4);
			Assert.Same(first, widget.Paintable);

			image.IsAnimationPlaying = true;
			Assert.True(animator.IsTicking);
			Assert.True(GtkTestHost.PumpUntil(() => !ReferenceEquals(first, widget.Paintable), host.PlatformWindow), "no frame was shown");

			image.IsAnimationPlaying = false;
			Assert.False(animator.IsTicking);
			GtkTestHost.AssertNoComplaints();
		});

		[GtkFact]
		public void ReplacingTheSourceWithAStillImageEndsTheAnimation() => GtkTestHost.Run(() =>
		{
			var gif = TestImages.CreateAnimatedGif();
			var png = TestImages.CreatePng(4, 4, 0, 255, 0);
			var image = new Image { IsAnimationPlaying = true, Source = ImageSource.FromStream(() => new MemoryStream(gif)) };
			using var host = new WindowHost(new Grid { Children = { image } }, 60, 60);
			var widget = (GtkPicture)host.Widget(image);
			var animator = ((GtkImageHandler)image.Handler!).Animator!;
			Assert.True(GtkTestHost.PumpUntil(() => animator.IsTicking, host.PlatformWindow));

			image.Source = ImageSource.FromStream(() => new MemoryStream(png));
			Assert.True(GtkTestHost.PumpUntil(() => animator.Animation is null && widget.Paintable is Gdk.Texture { Width: 4 }, host.PlatformWindow));
			Assert.False(animator.IsTicking);
		});

		[GtkFact]
		public void AnimatedFileSourcesPlayToo() => GtkTestHost.Run(() =>
		{
			var path = Path.Combine(Path.GetTempPath(), $"maui-gtk-{Guid.NewGuid():N}.gif");
			File.WriteAllBytes(path, TestImages.CreateAnimatedGif());
			try
			{
				var image = new Image { IsAnimationPlaying = true, Source = ImageSource.FromFile(path) };
				using var host = new WindowHost(new Grid { Children = { image } }, 60, 60);
				var animator = ((GtkImageHandler)image.Handler!).Animator!;
				Assert.True(GtkTestHost.PumpUntil(() => animator.Animation is not null && animator.IsTicking, host.PlatformWindow));
			}
			finally
			{
				File.Delete(path);
			}
		});
	}
}
