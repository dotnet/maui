using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.Forms
{
	public static class ViewExtensions
	{
		public static void CancelAnimations(this VisualElement view)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));
			view.AbortAnimation(nameof(LayoutTo));
			view.AbortAnimation(nameof(TranslateTo));
			view.AbortAnimation(nameof(RotateTo));
			view.AbortAnimation(nameof(RotateYTo));
			view.AbortAnimation(nameof(RotateXTo));
			view.AbortAnimation(nameof(ScaleTo));
			view.AbortAnimation(nameof(ScaleXTo));
			view.AbortAnimation(nameof(ScaleYTo));
			view.AbortAnimation(nameof(FadeTo));
		}

		static Task<bool> AnimateTo(this VisualElement view, double start, double end, string name,
			Action<VisualElement, double> updateAction, uint length = 250, Easing easing = null)
		{
			if (easing == null)
				easing = Easing.Linear;

			var tcs = new TaskCompletionSource<bool>();

			var weakView = new WeakReference<VisualElement>(view);

			void UpdateProperty(double f)
			{
				if (weakView.TryGetTarget(out VisualElement v))
				{
					updateAction(v, f);
				}
			}

			new Animation(UpdateProperty, start, end, easing).Commit(view, name, 16, length, finished: (f, a) => tcs.SetResult(a));

			return tcs.Task;
		}

		public static Task<bool> FadeTo(this VisualElement view, double opacity, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			return AnimateTo(view, view.Opacity, opacity, nameof(FadeTo), (v, value) => v.Opacity = value, length, easing);
		}

		public static Task<bool> LayoutTo(this VisualElement view, Rectangle bounds, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			Rectangle start = view.Bounds;
			Func<double, Rectangle> computeBounds = progress =>
			{
				double x = start.X + (bounds.X - start.X) * progress;
				double y = start.Y + (bounds.Y - start.Y) * progress;
				double w = start.Width + (bounds.Width - start.Width) * progress;
				double h = start.Height + (bounds.Height - start.Height) * progress;

				return new Rectangle(x, y, w, h);
			};

			return AnimateTo(view, 0, 1, nameof(LayoutTo), (v, value) => v.Layout(computeBounds(value)), length, easing);
		}

		public static Task<bool> RelRotateTo(this VisualElement view, double drotation, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			return view.RotateTo(view.Rotation + drotation, length, easing);
		}

		public static Task<bool> RelScaleTo(this VisualElement view, double dscale, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			return view.ScaleTo(view.Scale + dscale, length, easing);
		}

		public static Task<bool> RotateTo(this VisualElement view, double rotation, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			return AnimateTo(view, view.Rotation, rotation, nameof(RotateTo), (v, value) => v.Rotation = value, length, easing);
		}

		public static Task<bool> RotateXTo(this VisualElement view, double rotation, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			return AnimateTo(view, view.RotationX, rotation, nameof(RotateXTo), (v, value) => v.RotationX = value, length, easing);
		}

		public static Task<bool> RotateYTo(this VisualElement view, double rotation, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			return AnimateTo(view, view.RotationY, rotation, nameof(RotateYTo), (v, value) => v.RotationY = value, length, easing);
		}

		public static Task<bool> ScaleTo(this VisualElement view, double scale, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			return AnimateTo(view, view.Scale, scale, nameof(ScaleTo), (v, value) => v.Scale = value, length, easing);
		}

		public static Task<bool> ScaleXTo(this VisualElement view, double scale, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			return AnimateTo(view, view.ScaleX, scale, nameof(ScaleXTo), (v, value) => v.ScaleX = value, length, easing);
		}

		public static Task<bool> ScaleYTo(this VisualElement view, double scale, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			return AnimateTo(view, view.ScaleY, scale, nameof(ScaleYTo), (v, value) => v.ScaleY = value, length, easing);
		}

		public static Task<bool> TranslateTo(this VisualElement view, double x, double y, uint length = 250, Easing easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));
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
			new Animation { { 0, 1, new Animation(translateX, view.TranslationX, x, easing: easing) }, { 0, 1, new Animation(translateY, view.TranslationY, y, easing: easing) } }.Commit(view, nameof(TranslateTo), 16, length, null,
				(f, a) => tcs.SetResult(a));

			return tcs.Task;
		}

		internal static T FindParentOfType<T>(this Element element)
		{
			var navPage = element
				.GetParentsPath()
				.OfType<T>()
				.FirstOrDefault();

			return navPage;
		}

		internal static IEnumerable<Element> GetParentsPath(this Element self)
		{
			Element current = self;

			while (!Application.IsApplicationOrNull(current.RealParent))
			{
				current = current.RealParent;
				yield return current;
			}
		}

		internal static string GetStringValue(this Element element)
		{
			string text = null;
			if (element is Label label)
				text = label.Text;
			else if (element is Entry entry)
				text = entry.Text;
			else if (element is Editor editor)
				text = editor.Text;
			else if (element is TimePicker tp)
				text = tp.Time.ToString();
			else if (element is DatePicker dp)
				text = dp.Date.ToString();
			else if (element is CheckBox cb)
				text = cb.IsChecked.ToString();
			else if (element is Switch sw)
				text = sw.IsToggled.ToString();
			else if (element is RadioButton rb)
				text = rb.IsChecked.ToString();

			return text;
		}

		internal static bool TrySetValue(this Element element, string text)
		{
			if (element is Label label)
			{
				label.Text = text;
				return true;
			}
			else if (element is Entry entry)
			{
				entry.Text = text;
				return true;
			}
			else if (element is Editor editor)
			{
				editor.Text = text;
				return true;
			}
			else if (element is CheckBox cb && bool.TryParse(text, out bool result))
			{
				cb.IsChecked = result;
				return true;
			}
			else if (element is Switch sw && bool.TryParse(text, out bool swResult))
			{
				sw.IsToggled = swResult;
				return true;
			}
			else if (element is RadioButton rb && bool.TryParse(text, out bool rbResult))
			{
				rb.IsChecked = rbResult;
				return true;
			}
			else if (element is TimePicker tp && TimeSpan.TryParse(text, out TimeSpan tpResult))
			{
				tp.Time = tpResult;
				return true;
			}
			else if (element is DatePicker dp && DateTime.TryParse(text, out DateTime dpResult))
			{
				dp.Date = dpResult;
				return true;
			}

			return false;
		}
	}
}