using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Extension methods for <see cref="VisualElement" />s, providing animatable scaling, rotation, and layout functions.
	/// </summary>
	public static class ViewExtensions
	{
		/// <summary>
		/// Aborts all animations (e.g. <c>LayoutTo</c>, <c>TranslateTo</c>, <c>ScaleTo</c>, etc.) on the <paramref name= "view" /> element.
		/// </summary>
		///	<param name="view">The view on which this method operates.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="view"/> is <see langword="null"/>.</exception>
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
			Action<VisualElement, double> updateAction, uint length = 250, Easing? easing = null)
		{
			if (easing == null)
				easing = Easing.Linear;

			var tcs = new TaskCompletionSource<bool>();

			var weakView = new WeakReference<VisualElement>(view);

			void UpdateProperty(double f)
			{
				if (weakView.TryGetTarget(out VisualElement? v))
				{
					updateAction(v, f);
				}
			}

			new Animation(UpdateProperty, start, end, easing).Commit(view, name, 16, length, finished: (f, a) => tcs.SetResult(a));

			return tcs.Task;
		}


		/// <summary>
		/// Returns a task that performs the fade that is described by the <paramref name="opacity" />, <paramref name = "length" />, and <paramref name="easing" /> parameters.
		/// </summary>
		/// <param name="view">The view on which this method operates.</param>
		/// <param name="opacity">The opacity to fade to.</param>
		/// <param name="length">The time, in milliseconds, over which to animate the transition. The default is 250.</param>
		/// <param name="easing">The easing function to use for the animation.</param>
		/// <returns>A <see cref="Task"/> containing a <see cref="bool"/> value which indicates whether the animation was canceled. <see langword="true"/> indicates that the animation was canceled. <see langword="false"/> indicates that the animation ran to completion.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> is <see langword="null"/>.</exception>
		public static Task<bool> FadeTo(this VisualElement view, double opacity, uint length = 250, Easing? easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			return AnimateTo(view, view.Opacity, opacity, nameof(FadeTo), (v, value) => v.Opacity = value, length, easing);
		}

		/// <summary>
		/// <summary>Returns a task that eases the bounds of the <see cref="VisualElement" /> that is specified by the <paramref name="view" />
		/// to the rectangle that is specified by the <paramref name="bounds" /> parameter.</summary>
		/// </summary>
		/// <param name="view">The view on which this method operates.</param>
		/// <param name="bounds">The layout bounds.</param>
		/// <param name="length">The time, in milliseconds, over which to animate the transition. The default is 250.</param>
		/// <param name="easing">The easing function to use for the animation.</param>
		/// <returns>A <see cref="Task"/> containing a <see cref="bool"/> value which indicates whether the animation was canceled. <see langword="true"/> indicates that the animation was canceled. <see langword="false"/> indicates that the animation ran to completion.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> is <see langword="null"/>.</exception>
		public static Task<bool> LayoutTo(this VisualElement view, Rect bounds, uint length = 250, Easing? easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			Rect start = view.Bounds;
			Func<double, Rect> computeBounds = progress =>
			{
				double x = start.X + (bounds.X - start.X) * progress;
				double y = start.Y + (bounds.Y - start.Y) * progress;
				double w = start.Width + (bounds.Width - start.Width) * progress;
				double h = start.Height + (bounds.Height - start.Height) * progress;

				return new Rect(x, y, w, h);
			};

			return AnimateTo(view, 0, 1, nameof(LayoutTo), (v, value) => v.Layout(computeBounds(value)), length, easing);
		}

		/// <summary>
		/// Rotates the <see cref="VisualElement" /> that is specified by <paramref name="view" /> from its current rotation by <paramref name="drotation" />.
		/// </summary>
		/// <param name="view">The view on which this method operates.</param>
		/// <param name="drotation">The relative rotation.</param>
		/// <param name="length">The time, in milliseconds, over which to animate the transition. The default is 250.</param>
		/// <param name="easing">The easing function to use for the animation.</param>
		/// <returns>A <see cref="Task"/> containing a <see cref="bool"/> value which indicates whether the animation was canceled. <see langword="true"/> indicates that the animation was canceled. <see langword="false"/> indicates that the animation ran to completion.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> is <see langword="null"/>.</exception>
		public static Task<bool> RelRotateTo(this VisualElement view, double drotation, uint length = 250, Easing? easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			return view.RotateTo(view.Rotation + drotation, length, easing);
		}

		/// <summary>
		/// Returns a task that scales the <see cref="VisualElement" /> that is specified by <paramref name="view" />
		/// from its current scale to <paramref name="dscale" />.
		/// </summary>
		/// <param name="view">The view on which this method operates.</param>
		/// <param name="dscale">The relative scale.</param>
		/// <param name="length">The time, in milliseconds, over which to animate the transition. The default is 250.</param>
		/// <param name="easing">The easing function to use for the animation.</param>
		/// <returns>A <see cref="Task"/> containing a <see cref="bool"/> value which indicates whether the animation was canceled. <see langword="true"/> indicates that the animation was canceled. <see langword="false"/> indicates that the animation ran to completion.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> is <see langword="null"/>.</exception>
		public static Task<bool> RelScaleTo(this VisualElement view, double dscale, uint length = 250, Easing? easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			return view.ScaleTo(view.Scale + dscale, length, easing);
		}

		/// <summary>
		/// Returns a task that rotates the <see cref="VisualElement" /> that is specified by <paramref name="view" />
		/// that is described by the <paramref name="rotation" />, <paramref name="length" />, and <paramref name="easing" /> parameters.
		/// </summary>
		/// <param name="view">The view on which this method operates.</param>
		/// <param name="rotation">The final rotation value.</param>
		/// <param name="length">The time, in milliseconds, over which to animate the transition. The default is 250.</param>
		/// <param name="easing">The easing function to use for the animation.</param>
		/// <returns>A <see cref="Task"/> containing a <see cref="bool"/> value which indicates whether the animation was canceled. <see langword="true"/> indicates that the animation was canceled. <see langword="false"/> indicates that the animation ran to completion.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> is <see langword="null"/>.</exception>
		public static Task<bool> RotateTo(this VisualElement view, double rotation, uint length = 250, Easing? easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			return AnimateTo(view, view.Rotation, rotation, nameof(RotateTo), (v, value) => v.Rotation = value, length, easing);
		}

		/// <summary>
		/// Returns a task that skews the X axis of the the <see cref="VisualElement" /> that is specified by <paramref name="view" />
		/// by <paramref name="rotation" />, taking time <paramref name="length" /> and using <paramref name="easing" />.
		/// </summary>
		/// <param name="view">The view on which this method operates.</param>
		/// <param name="rotation">The final rotation value.</param>
		/// <param name="length">The time, in milliseconds, over which to animate the transition. The default is 250.</param>
		/// <param name="easing">The easing function to use for the animation.</param>
		/// <returns>A <see cref="Task"/> containing a <see cref="bool"/> value which indicates whether the animation was canceled. <see langword="true"/> indicates that the animation was canceled. <see langword="false"/> indicates that the animation ran to completion.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> is <see langword="null"/>.</exception>
		public static Task<bool> RotateXTo(this VisualElement view, double rotation, uint length = 250, Easing? easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			return AnimateTo(view, view.RotationX, rotation, nameof(RotateXTo), (v, value) => v.RotationX = value, length, easing);
		}

		/// <summary>
		/// Returns a task that skews the Y axis of the the <see cref="VisualElement" /> that is specified by <paramref name="view" />
		/// by <paramref name="rotation" />, taking time <paramref name="length" /> and using <paramref name="easing" />.
		/// </summary>
		/// <param name="view">The view on which this method operates.</param>
		/// <param name="rotation">The final rotation value.</param>
		/// <param name="length">The time, in milliseconds, over which to animate the transition. The default is 250.</param>
		/// <param name="easing">The easing function to use for the animation.</param>
		/// <returns>A <see cref="Task"/> containing a <see cref="bool"/> value which indicates whether the animation was canceled. <see langword="true"/> indicates that the animation was canceled. <see langword="false"/> indicates that the animation ran to completion.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> is <see langword="null"/>.</exception>
		public static Task<bool> RotateYTo(this VisualElement view, double rotation, uint length = 250, Easing? easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			return AnimateTo(view, view.RotationY, rotation, nameof(RotateYTo), (v, value) => v.RotationY = value, length, easing);
		}

		/// <summary>
		/// Returns a task that scales the <see cref="VisualElement" /> that is specified by <paramref name="view" /> to the absolute scale factor <paramref name="scale" />.
		/// </summary>
		/// <param name="view">The view on which this method operates.</param>
		/// <param name="scale">The final absolute scale.</param>
		/// <param name="length">The time, in milliseconds, over which to animate the transition. The default is 250.</param>
		/// <param name="easing">The easing function to use for the animation.</param>
		/// <returns>A <see cref="Task"/> containing a <see cref="bool"/> value which indicates whether the animation was canceled. <see langword="true"/> indicates that the animation was canceled. <see langword="false"/> indicates that the animation ran to completion.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> is <see langword="null"/>.</exception>
		public static Task<bool> ScaleTo(this VisualElement view, double scale, uint length = 250, Easing? easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			return AnimateTo(view, view.Scale, scale, nameof(ScaleTo), (v, value) => v.Scale = value, length, easing);
		}

		/// <summary>
		/// Returns a task that scales the X axis of the the <see cref="VisualElement" /> that is specified by <paramref name="view" />
		/// to the absolute scale factor <paramref name="scale" />.
		/// </summary>
		/// <param name="view">The view on which this method operates.</param>
		/// <param name="scale">The final absolute scale.</param>
		/// <param name="length">The time, in milliseconds, over which to animate the transition. The default is 250.</param>
		/// <param name="easing">The easing function to use for the animation.</param>
		/// <returns>A <see cref="Task"/> containing a <see cref="bool"/> value which indicates whether the animation was canceled. <see langword="true"/> indicates that the animation was canceled. <see langword="false"/> indicates that the animation ran to completion.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> is <see langword="null"/>.</exception>
		public static Task<bool> ScaleXTo(this VisualElement view, double scale, uint length = 250, Easing? easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			return AnimateTo(view, view.ScaleX, scale, nameof(ScaleXTo), (v, value) => v.ScaleX = value, length, easing);
		}

		/// <summary>
		/// Returns a task that scales the Y axis of the the <see cref="VisualElement" /> that is specified by <paramref name="view" />
		/// to the absolute scale factor <paramref name="scale" />.
		/// </summary>
		/// <param name="view">The view on which this method operates.</param>
		/// <param name="scale">The final absolute scale.</param>
		/// <param name="length">The time, in milliseconds, over which to animate the transition. The default is 250.</param>
		/// <param name="easing">The easing function to use for the animation.</param>
		/// <returns>A <see cref="Task"/> containing a <see cref="bool"/> value which indicates whether the animation was canceled. <see langword="true"/> indicates that the animation was canceled. <see langword="false"/> indicates that the animation ran to completion.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> is <see langword="null"/>.</exception>
		public static Task<bool> ScaleYTo(this VisualElement view, double scale, uint length = 250, Easing? easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			return AnimateTo(view, view.ScaleY, scale, nameof(ScaleYTo), (v, value) => v.ScaleY = value, length, easing);
		}

		/// <summary>
		/// Animates an elements <see cref="VisualElement.TranslationX"/> and <see cref="VisualElement.TranslationY"/> properties
		/// from their current values to the new values. This ensures that the input layout is in the same position as the visual layout.
		/// </summary>
		/// <param name="view">The view on which this method operates.</param>
		/// <param name="x">The x component of the final translation vector.</param>
		/// <param name="y">The y component of the final translation vector.</param>
		/// <param name="length">The time, in milliseconds, over which to animate the transition. The default is 250.</param>
		/// <param name="easing">The easing function to use for the animation.</param>
		/// <returns>A <see cref="Task"/> containing a <see cref="bool"/> value which indicates whether the animation was canceled. <see langword="true"/> indicates that the animation was canceled. <see langword="false"/> indicates that the animation ran to completion.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> is <see langword="null"/>.</exception>
		public static Task<bool> TranslateTo(this VisualElement view, double x, double y, uint length = 250, Easing? easing = null)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			easing ??= Easing.Linear;

			var tcs = new TaskCompletionSource<bool>();
			var weakView = new WeakReference<VisualElement>(view);
			Action<double> translateX = f =>
			{
				if (weakView.TryGetTarget(out VisualElement? v))
					v.TranslationX = f;
			};
			Action<double> translateY = f =>
			{
				if (weakView.TryGetTarget(out VisualElement? v))
					v.TranslationY = f;
			};

			new Animation
			{
				{ 0, 1, new Animation(translateX, view.TranslationX, x, easing: easing) },
				{ 0, 1, new Animation(translateY, view.TranslationY, y, easing: easing) }
			}.Commit(view, nameof(TranslateTo), 16, length, null, (f, a) => tcs.SetResult(a));

			return tcs.Task;
		}

		internal static IAnimationManager GetAnimationManager(this IAnimatable animatable)
		{
			if (animatable is Element element)
			{
				if (element.FindMauiContext() is IMauiContext viewMauiContext)
					return viewMauiContext.GetAnimationManager();

				if (Application.Current?.FindMauiContext() is IMauiContext applicationMauiContext)
					return applicationMauiContext.GetAnimationManager();
			}

			throw new ArgumentException($"Unable to find {nameof(IAnimationManager)} for '{animatable.GetType().FullName}'.", nameof(animatable));
		}

		internal static IMauiContext RequireMauiContext(this Element element, bool fallbackToAppMauiContext = false)
			=> element.FindMauiContext(fallbackToAppMauiContext) ?? throw new InvalidOperationException($"{nameof(IMauiContext)} not found.");

		internal static IMauiContext? FindMauiContext(this Element element, bool fallbackToAppMauiContext = false)
		{
			if (element is Maui.IElement fe && fe.Handler?.MauiContext != null)
				return fe.Handler.MauiContext;

			foreach (var parent in element.GetParentsPath())
			{
				if (parent is Maui.IElement parentView && parentView.Handler?.MauiContext != null)
					return parentView.Handler.MauiContext;
			}

			return fallbackToAppMauiContext ? Application.Current?.FindMauiContext() : default;
		}

		internal static ILogger<T>? CreateLogger<T>(this Element element, bool fallbackToAppMauiContext = true) =>
			element.FindMauiContext(fallbackToAppMauiContext)?.CreateLogger<T>();

		internal static IFontManager RequireFontManager(this Element element, bool fallbackToAppMauiContext = false)
			=> element.RequireMauiContext(fallbackToAppMauiContext).Services.GetRequiredService<IFontManager>();

		internal static double GetDefaultFontSize(this Element element)
			=> element.FindMauiContext()?.Services?.GetService<IFontManager>()?.DefaultFontSize ?? 0d;

		internal static Element? FindParentWith(this Element element, Func<Element, bool> withMatch, bool includeThis = false)
		{
			if (includeThis && withMatch(element))
				return element;

			foreach (var parent in element.GetParentsPath())
			{
				if (withMatch(parent))
					return parent;
			}

			return default;
		}

		internal static T? FindParentOfType<T>(this Element element, bool includeThis = false)
			where T : Maui.IElement
		{
			if (includeThis && element is T view)
				return view;

			foreach (var parent in element.GetParentsPath())
			{
				if (parent is T parentView)
					return parentView;
			}

			return default;
		}

		internal static IList<IGestureRecognizer>? GetCompositeGestureRecognizers(this Element element)
		{
			if (element is IGestureController gc)
				return gc.CompositeGestureRecognizers;

			return null;
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

		internal static List<Page> GetParentPages(this Page target)
		{
			var result = new List<Page>();

			var parent = target.RealParent as Page;
			while (!Application.IsApplicationOrWindowOrNull(parent))
			{
				result.Add(parent!);
				parent = parent!.RealParent as Page;
			}

			return result;
		}

		internal static string? GetStringValue(this IView element)
		{
			string? text = null;

			if (element is ILabel label)
				text = label.Text;
			else if (element is IEntry entry)
				text = entry.Text;
			else if (element is IEditor editor)
				text = editor.Text;
			else if (element is ITimePicker tp)
				text = tp.Time.ToString();
			else if (element is IDatePicker dp)
				text = dp.Date.ToString();
			else if (element is ICheckBox cb)
				text = cb.IsChecked.ToString();
			else if (element is ISwitch sw)
				text = sw.IsOn.ToString();
			else if (element is IRadioButton rb)
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

		static internal bool RequestFocus(this VisualElement view)
		{
			// if there is an attached handler, we use that and we will end up in the MapFocus method below
			if (view.Handler is IViewHandler handler)
				return handler.InvokeWithResult(nameof(IView.Focus), new FocusRequest());

			// if there is no handler, we need to still run some code
			var focusRequest = new FocusRequest();
			view.MapFocus(focusRequest);
			return focusRequest.Result;
		}

		static internal void MapFocus(this VisualElement view, FocusRequest focusRequest, Action? baseMethod = null)
		{
			// the virtual view is already focused
			if (view.IsFocused)
			{
				focusRequest.TrySetResult(true);
				return;
			}

			// if there are legacy events, then use that
			if (view.HasFocusChangeRequestedEvent)
			{
				var arg = new VisualElement.FocusRequestArgs { Focus = true };
				view.InvokeFocusChangeRequested(arg);
				focusRequest.TrySetResult(arg.Result);
				return;
			}

			// otherwise, fall back to "base"
			if (baseMethod is not null)
			{
				baseMethod.Invoke();
				return;
			}

			// if there was nothing that handles this, then nothing changed
			focusRequest.TrySetResult(false);
		}

		internal static IMauiContext? GetCurrentlyPresentedMauiContext(this Element element)
		{
			var window = (element as Window) ?? (element as IWindowController)?.Window;

			if (window is null)
				return null;

			var modalStack = window.Navigation.ModalStack;
			if (modalStack.Count > 0)
			{
				var currentPage = modalStack[modalStack.Count - 1];
				if (currentPage.Handler?.MauiContext is IMauiContext mauiContext)
				{
					return mauiContext;
				}
			}

			return window.Handler?.MauiContext;
		}

		/// <summary>
		/// Layout updates can be forced by app code rather than relying on the built-in layout system behavior. However, that is not generally recommended. 
		/// Calling InvalidateArrange, InvalidateMeasure or UpdateLayout is usually unnecessary and can cause poor performance if overused. 
		/// In many situations where app code might be changing layout properties, the layout system will probably already be processing updates asynchronously. 
		/// The layout system also has optimizations for dealing with cascades of layout changes through parent-child relationships, 
		/// and forcing layout with app code can work against such optimizations. Nevertheless, 
		/// it's possible that layout situations exist in more complicated scenarios where forcing layout is the best option for resolving a timing issue or other issue with layout. 
		/// Just use it deliberately and sparingly.
		/// </summary>
		/// <param name="view">The view on which this method operates.</param>
		public static void InvalidateMeasure(this VisualElement view)
		{
			(view as IView)?.InvalidateMeasure();
		}
	}
}