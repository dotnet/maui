using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using NUnit.Framework;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS.UnitTests
{
	[Preserve(AllMembers = true)]
	public class PlatformTestFixture
	{
		protected static UIColor EmptyBackground = new UIColor(0f, 0f, 0f, 0f);

		// Sequence for generating test cases
		protected static IEnumerable<View> BasicViews
		{
			get
			{
				yield return new BoxView { };
				yield return new Button { };
				yield return new CheckBox { };
				yield return new DatePicker { };
				yield return new Editor { };
				yield return new Entry { };
				yield return new Frame { };
				yield return new Image { };
				yield return new ImageButton { };
				yield return new Label { };
				yield return new Picker { };
				yield return new ProgressBar { };
				yield return new SearchBar { };
				yield return new Slider { };
				yield return new Stepper { };
				yield return new Switch { };
				yield return new TimePicker { };
			}
		}

		[SetUp]
		public virtual void Setup()
		{

		}

		[TearDown]
		public virtual void TearDown()
		{

		}

		protected static TestCaseData CreateTestCase(VisualElement element)
		{
			// We set the element type as a category on the test so that if you 
			// filter by category, say, "Button", you'll get any Button test 
			// generated from here. 

			return new TestCaseData(element).SetCategory(element.GetType().Name);
		}

		protected IVisualElementRenderer GetRenderer(VisualElement element)
		{
			return Platform.CreateRenderer(element);
		}

		protected async Task<IVisualElementRenderer> GetRendererAsync(VisualElement element)
		{
			return await Device.InvokeOnMainThreadAsync<IVisualElementRenderer>(() => Platform.CreateRenderer(element));
		}

		protected UIView GetPlatformControl(VisualElement visualElement)
		{
			var renderer = GetRenderer(visualElement);
			var viewRenderer = renderer as IVisualPlatformElementRenderer;
			return viewRenderer?.Control;
		}

		protected UILabel GetPlatformControl(Label label)
		{
			var renderer = GetRenderer(label);
			var viewRenderer = renderer.PlatformView as LabelRenderer;
			return viewRenderer.Control;
		}

		protected async Task<TProperty> GetControlProperty<TProperty>(Label label, Func<UILabel, TProperty> getProperty)
		{
			return await Device.InvokeOnMainThreadAsync(() =>
			{
				using (var uiLabel = GetPlatformControl(label))
				{
					return getProperty(uiLabel);
				}
			});
		}

		protected async Task<TProperty> GetControlProperty<TProperty>(VisualElement view, Func<UIView, TProperty> getProperty)
		{
			return await Device.InvokeOnMainThreadAsync(() =>
			{
				using (var renderer = GetPlatformControl(view))
				{
					return getProperty(renderer);
				}
			});
		}

		protected UITextField GetPlatformControl(Entry entry)
		{
			var renderer = GetRenderer(entry);
			var viewRenderer = renderer.PlatformView as EntryRenderer;
			return viewRenderer.Control;
		}

		protected async Task<TProperty> GetControlProperty<TProperty>(Entry entry, Func<UITextField, TProperty> getProperty)
		{
			return await Device.InvokeOnMainThreadAsync(() =>
			{
				using (var uiTextField = GetPlatformControl(entry))
				{
					return getProperty(uiTextField);
				}
			});
		}

		protected UITextView GetPlatformControl(Editor editor)
		{
			var renderer = GetRenderer(editor);
			var viewRenderer = renderer.PlatformView as EditorRenderer;
			return viewRenderer.Control;
		}

		protected async Task<TProperty> GetControlProperty<TProperty>(Editor editor, Func<UITextView, TProperty> getProperty)
		{
			return await Device.InvokeOnMainThreadAsync(() =>
			{
				using (var uiTextView = GetPlatformControl(editor))
				{
					return getProperty(uiTextView);
				}
			});
		}

		protected UIButton GetPlatformControl(Button button)
		{
			var renderer = GetRenderer(button);
			var viewRenderer = renderer.PlatformView as ButtonRenderer;
			return viewRenderer.Control;
		}

		protected async Task<TProperty> GetControlProperty<TProperty>(Button button, Func<UIButton, TProperty> getProperty)
		{
			return await Device.InvokeOnMainThreadAsync(() =>
			{
				using (var uiButton = GetPlatformControl(button))
				{
					return getProperty(uiButton);
				}
			});
		}

		protected UIButton GetPlatformControl(ImageButton button)
		{
			var renderer = GetRenderer(button);
			var viewRenderer = renderer.PlatformView as ImageButtonRenderer;
			return viewRenderer.Control;
		}

		protected async Task<TProperty> GetControlProperty<TProperty>(ImageButton button, Func<UIButton, TProperty> getProperty)
		{
			return await Device.InvokeOnMainThreadAsync(() =>
			{
				using (var uiButton = GetPlatformControl(button))
				{
					return getProperty(uiButton);
				}
			});
		}

		protected UITextField GetPlatformControl(DatePicker datePicker)
		{
			var renderer = GetRenderer(datePicker);
			var viewRenderer = renderer.PlatformView as DatePickerRenderer;
			return viewRenderer.Control;
		}

		protected UIDatePicker GetPickerControl(DatePicker datePicker)
		{
			var renderer = GetRenderer(datePicker);
			var viewRenderer = renderer.PlatformView as DatePickerRenderer;
			return viewRenderer.Picker;
		}

		protected async Task<TProperty> GetControlProperty<TProperty>(DatePicker datePicker, Func<UITextField, TProperty> getProperty)
		{
			return await Device.InvokeOnMainThreadAsync(() =>
			{
				using (var uiTextField = GetPlatformControl(datePicker))
				{
					return getProperty(uiTextField);
				}
			});
		}

		protected async Task<TProperty> GetControlProperty<TProperty>(DatePicker datePicker, Func<UIDatePicker, TProperty> getProperty)
		{
			return await Device.InvokeOnMainThreadAsync(() =>
			{
				using (var uiDatePicker = GetPickerControl(datePicker))
				{
					return getProperty(uiDatePicker);
				}
			});
		}

		protected UITextField GetPlatformControl(TimePicker timePicker)
		{
			var renderer = GetRenderer(timePicker);
			var viewRenderer = renderer.PlatformView as TimePickerRenderer;
			return viewRenderer.Control;
		}

		protected UIDatePicker GetPickerControl(TimePicker timePicker)
		{
			var renderer = GetRenderer(timePicker);
			var viewRenderer = renderer.PlatformView as TimePickerRenderer;
			return viewRenderer.Picker;
		}

		protected async Task<TProperty> GetControlProperty<TProperty>(TimePicker timePicker, Func<UITextField, TProperty> getProperty)
		{
			return await Device.InvokeOnMainThreadAsync(() =>
			{
				using (var uiTextField = GetPlatformControl(timePicker))
				{
					return getProperty(uiTextField);
				}
			});
		}

		protected async Task<TProperty> GetControlProperty<TProperty>(TimePicker timePicker, Func<UIDatePicker, TProperty> getProperty)
		{
			return await Device.InvokeOnMainThreadAsync(() =>
			{
				using (var uiDatePicker = GetPickerControl(timePicker))
				{
					return getProperty(uiDatePicker);
				}
			});
		}


		protected async Task<TProperty> GetRendererProperty<TProperty>(View view,
			Func<IVisualElementRenderer, TProperty> getProperty, bool requiresLayout = false)
		{
			if (requiresLayout)
			{
				return await GetRendererPropertyWithLayout(view, getProperty);
			}
			else
			{
				return await GetRendererProperty(view, getProperty);
			}
		}

		async Task<TProperty> GetRendererProperty<TProperty>(View view,
			Func<IVisualElementRenderer, TProperty> getProperty)
		{
			return await Device.InvokeOnMainThreadAsync(() =>
			{
				using (var renderer = GetRenderer(view))
				{
					return getProperty(renderer);
				}
			});
		}

		async Task<TProperty> GetRendererPropertyWithLayout<TProperty>(View view,
			Func<IVisualElementRenderer, TProperty> getRendererProperty)
		{
			return await Device.InvokeOnMainThreadAsync(() =>
			{

				var page = new ContentPage() { Content = view };
				using (var pageRenderer = GetRenderer(page))
				{
					using (var renderer = GetRenderer(view))
					{
						page.Layout(new Rectangle(0, 0, 200, 200));
						return getRendererProperty(renderer);
					}
				}
			});
		}

		protected bool AreColorsSimilar(UIColor c1, UIColor c2, int tolerance)
		{
			c1.GetRGBA(out nfloat c1R, out nfloat c1G, out nfloat c1B, out nfloat c1A);

			c1R *= 255;
			c1G *= 255;
			c1B *= 255;

			c2.GetRGBA(out nfloat c2R, out nfloat c2G, out nfloat c2B, out nfloat c2A);

			c2R *= 255;
			c2G *= 255;
			c2B *= 255;

			var t =
				Math.Abs(c1R - c2R) < tolerance &&
				Math.Abs(c1G - c2G) < tolerance &&
				Math.Abs(c1B - c2B) < tolerance;
			System.Diagnostics.Debug.WriteLine($"TEST {t}");
			return
				Math.Abs(c1R - c2R) < tolerance &&
				Math.Abs(c1G - c2G) < tolerance &&
				Math.Abs(c1B - c2B) < tolerance;
		}


	}
}