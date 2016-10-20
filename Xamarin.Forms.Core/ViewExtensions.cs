using System;
using System.Threading.Tasks;

namespace Xamarin.Forms
{
	public static class ViewExtensions
	{
		public static void CancelAnimations(VisualElement view)
		{
			if (view == null)
				throw new ArgumentNullException("view");
			view.AbortAnimation("LayoutTo");
			view.AbortAnimation("TranslateTo");
			view.AbortAnimation("RotateTo");
			view.AbortAnimation("RotateYTo");
			view.AbortAnimation("RotateXTo");
			view.AbortAnimation("ScaleTo");
			view.AbortAnimation("FadeTo");
			view.AbortAnimation("SizeTo");
		}

		public static Task<bool> FadeTo(this VisualElement view, double opacity, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException("view");
			if (easing == null)
				easing = Easing.Linear;

			var tcs = new TaskCompletionSource<bool>();
			var weakView = new WeakReference<VisualElement>(view);
			Action<double> fade = f =>
			{
				VisualElement v;
				if (weakView.TryGetTarget(out v))
					v.Opacity = f;
			};

			new Animation(fade, view.Opacity, opacity, easing).Commit(view, "FadeTo", 16, length, finished: (f, a) => tcs.SetResult(a));

			return tcs.Task;
		}

		public static Task<bool> LayoutTo(this VisualElement view, Rectangle bounds, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException("view");
			if (easing == null)
				easing = Easing.Linear;

			var tcs = new TaskCompletionSource<bool>();
			Rectangle start = view.Bounds;
			Func<double, Rectangle> computeBounds = progress =>
			{
				double x = start.X + (bounds.X - start.X) * progress;
				double y = start.Y + (bounds.Y - start.Y) * progress;
				double w = start.Width + (bounds.Width - start.Width) * progress;
				double h = start.Height + (bounds.Height - start.Height) * progress;

				return new Rectangle(x, y, w, h);
			};
			var weakView = new WeakReference<VisualElement>(view);
			Action<double> layout = f =>
			{
				VisualElement v;
				if (weakView.TryGetTarget(out v))
					v.Layout(computeBounds(f));
			};
			new Animation(layout, 0, 1, easing).Commit(view, "LayoutTo", 16, length, finished: (f, a) => tcs.SetResult(a));

			return tcs.Task;
		}

		public static Task<bool> RelRotateTo(this VisualElement view, double drotation, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException("view");
			return view.RotateTo(view.Rotation + drotation, length, easing);
		}

		public static Task<bool> RelScaleTo(this VisualElement view, double dscale, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException("view");
			return view.ScaleTo(view.Scale + dscale, length, easing);
		}

		public static Task<bool> RotateTo(this VisualElement view, double rotation, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException("view");
			if (easing == null)
				easing = Easing.Linear;

			var tcs = new TaskCompletionSource<bool>();
			var weakView = new WeakReference<VisualElement>(view);
			Action<double> rotate = f =>
			{
				VisualElement v;
				if (weakView.TryGetTarget(out v))
					v.Rotation = f;
			};

			new Animation(rotate, view.Rotation, rotation, easing).Commit(view, "RotateTo", 16, length, finished: (f, a) => tcs.SetResult(a));

			return tcs.Task;
		}

		public static Task<bool> RotateXTo(this VisualElement view, double rotation, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException("view");
			if (easing == null)
				easing = Easing.Linear;

			var tcs = new TaskCompletionSource<bool>();
			var weakView = new WeakReference<VisualElement>(view);
			Action<double> rotatex = f =>
			{
				VisualElement v;
				if (weakView.TryGetTarget(out v))
					v.RotationX = f;
			};

			new Animation(rotatex, view.RotationX, rotation, easing).Commit(view, "RotateXTo", 16, length, finished: (f, a) => tcs.SetResult(a));

			return tcs.Task;
		}

		public static Task<bool> RotateYTo(this VisualElement view, double rotation, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException("view");
			if (easing == null)
				easing = Easing.Linear;

			var tcs = new TaskCompletionSource<bool>();
			var weakView = new WeakReference<VisualElement>(view);
			Action<double> rotatey = f =>
			{
				VisualElement v;
				if (weakView.TryGetTarget(out v))
					v.RotationY = f;
			};

			new Animation(rotatey, view.RotationY, rotation, easing).Commit(view, "RotateYTo", 16, length, finished: (f, a) => tcs.SetResult(a));

			return tcs.Task;
		}

		public static Task<bool> ScaleTo(this VisualElement view, double scale, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException("view");
			if (easing == null)
				easing = Easing.Linear;

			var tcs = new TaskCompletionSource<bool>();
			var weakView = new WeakReference<VisualElement>(view);
			Action<double> _scale = f =>
			{
				VisualElement v;
				if (weakView.TryGetTarget(out v))
					v.Scale = f;
			};

			new Animation(_scale, view.Scale, scale, easing).Commit(view, "ScaleTo", 16, length, finished: (f, a) => tcs.SetResult(a));

			return tcs.Task;
		}

		public static Task<bool> TranslateTo(this VisualElement view, double x, double y, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException("view");
			easing = easing ?? Easing.Linear;

			var tcs = new TaskCompletionSource<bool>();
			var weakView = new WeakReference<VisualElement>(view);
			Action<double> translateX = f =>
			{
				VisualElement v;
				if (weakView.TryGetTarget(out v))
					v.TranslationX = f;
			};
			Action<double> translateY = f =>
			{
				VisualElement v;
				if (weakView.TryGetTarget(out v))
					v.TranslationY = f;
			};
			new Animation { { 0, 1, new Animation(translateX, view.TranslationX, x, easing: easing) }, { 0, 1, new Animation(translateY, view.TranslationY, y, easing:easing) } }.Commit(view, "TranslateTo", 16, length, null,
				(f, a) => tcs.SetResult(a));

			return tcs.Task;
		}
	}
}