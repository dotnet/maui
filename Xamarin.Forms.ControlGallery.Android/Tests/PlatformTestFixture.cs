using Android.Content;
using Android.Content.PM;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using AView = Android.Views.View;
using AColor = Android.Graphics.Color;
using Android.Views;
using System;
using AProgressBar = Android.Widget.ProgressBar;
using ASearchView = Android.Widget.SearchView;
using System.Collections.Generic;
using NUnit.Framework;

#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
using AndroidX.CardView.Widget;
#else
using Android.Support.V7.Widget;
#endif

namespace Xamarin.Forms.ControlGallery.Android.Tests
{
	public class PlatformTestFixture
	{
		Context _context;

		protected static AColor EmptyBackground = new AColor(0, 0, 0, 255);

		// Sequence for generating test cases
		protected static IEnumerable<VisualElement> BasicElements
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

		protected static TestCaseData CreateTestCase(VisualElement element) 
		{
			// We set the element type as a category on the test so that if you 
			// filter by category, say, "Button", you'll get any Button test 
			// generated from here. 

			return new TestCaseData(element).SetCategory(element.GetType().Name);
		}

		protected Context Context
		{
			get
			{
				if (_context == null)
				{
					_context = DependencyService.Resolve<Context>();
				}

				return _context;
			}
		}

		protected static void ToggleRTLSupport(Context context, bool enabled)
		{
			context.ApplicationInfo.Flags = enabled
				? context.ApplicationInfo.Flags | ApplicationInfoFlags.SupportsRtl
				: context.ApplicationInfo.Flags & ~ApplicationInfoFlags.SupportsRtl;
		}

		protected IVisualElementRenderer GetRenderer(VisualElement element) 
		{
			var renderer = element.GetRenderer();
			if (renderer == null) 
			{
				renderer = Platform.Android.Platform.CreateRendererWithContext(element, Context);
				Platform.Android.Platform.SetRenderer(element, renderer);
			}

			return renderer;
		}

		protected AView GetNativeControl(VisualElement element)
		{
			switch (element)
			{
				case BoxView boxView:
					return GetNativeControl(boxView);
				case Button button:
					return GetNativeControl(button);
				case CheckBox checkBox:
					return GetNativeControl(checkBox);
				case DatePicker datePicker:
					return GetNativeControl(datePicker);
				case Editor editor:
					return GetNativeControl(editor);
				case Entry entry:
					return GetNativeControl(entry);
				case Image image:
					return GetNativeControl(image);
				case ImageButton imageButton:
					return GetNativeControl(imageButton);
				case Frame frame:
					return GetNativeControl(frame);
				case Label label:
					return GetNativeControl(label);
				case Picker picker:
					return GetNativeControl(picker);
				case ProgressBar progressBar:
					return GetNativeControl(progressBar);
				case SearchBar searchBar:
					return GetNativeControl(searchBar);
				case Slider slider:
					return GetNativeControl(slider);
				case Stepper stepper:
					return GetNativeControl(stepper);
				case Switch @switch:
					return GetNativeControl(@switch);
				case TimePicker timePicker:
					return GetNativeControl(timePicker);
			}

			throw new NotImplementedException($"Don't know how to get the native control for {element}");
		}

		protected BoxRenderer GetNativeControl(BoxView boxView)
		{
			var renderer = GetRenderer(boxView);
			return renderer as BoxRenderer;
		}

		protected AppCompatButton GetNativeControl(Button button)
		{
			var renderer = GetRenderer(button);

			if (renderer is AppCompatButton fastButton)
			{
				return fastButton;
			}

			var viewRenderer = renderer.View as Platform.Android.AppCompat.ButtonRenderer;
			return viewRenderer.Control;
		}

		protected AppCompatCheckBox GetNativeControl(CheckBox checkbox)
		{
			var renderer = GetRenderer(checkbox);
			return renderer as AppCompatCheckBox;
		}

		protected EditText GetNativeControl(DatePicker datePicker)
		{
			var renderer = GetRenderer(datePicker);
			var viewRenderer = renderer.View as DatePickerRenderer;
			return viewRenderer.Control;
		}

		protected FormsEditText GetNativeControl(Editor editor)
		{
			var renderer = GetRenderer(editor);
			var viewRenderer = renderer.View as EditorRenderer;
			return viewRenderer.Control;
		}

		protected FormsEditText GetNativeControl(Entry entry)
		{
			var renderer = GetRenderer(entry);
			var viewRenderer = renderer.View as EntryRenderer;
			return viewRenderer.Control;
		}

		protected ImageView GetNativeControl(Image image)
		{
			var renderer = GetRenderer(image);

			if (renderer is ImageView fastImage)
			{
				return fastImage;
			}

			var viewRenderer = renderer.View as ImageRenderer;
			return viewRenderer.Control;
		}

		protected AppCompatImageButton GetNativeControl(ImageButton imageButton)
		{
			return GetRenderer(imageButton) as AppCompatImageButton;
		}

		protected CardView GetNativeControl(Frame frame)
		{
			var renderer = GetRenderer(frame);
			return renderer as CardView;
		}

		protected TextView GetNativeControl(Label label) 
		{
			var renderer = GetRenderer(label);
			var viewRenderer = renderer.View as LabelRenderer;

			if (viewRenderer != null)
			{
				return viewRenderer.Control;
			}

			var fastRenderer = renderer.View as Platform.Android.FastRenderers.LabelRenderer;

			return fastRenderer;
		}

		protected EditText GetNativeControl(Picker picker)
		{
			var renderer = GetRenderer(picker);
			var viewRenderer = renderer.View as Platform.Android.AppCompat.PickerRenderer;
			return viewRenderer.Control;
		}

		protected AProgressBar GetNativeControl(ProgressBar progressBar)
		{
			var renderer = GetRenderer(progressBar);
			var viewRenderer = renderer.View as ProgressBarRenderer;
			return viewRenderer.Control;
		}

		protected ASearchView GetNativeControl(SearchBar searchBar)
		{
			var renderer = GetRenderer(searchBar);
			var viewRenderer = renderer.View as SearchBarRenderer;
			return viewRenderer.Control;
		}

		protected SeekBar GetNativeControl(Slider slider)
		{
			var renderer = GetRenderer(slider);
			var viewRenderer = renderer.View as SliderRenderer;
			return viewRenderer.Control;
		}

		protected LinearLayout GetNativeControl(Stepper stepper)
		{
			var renderer = GetRenderer(stepper);
			var viewRenderer = renderer.View as StepperRenderer;
			return viewRenderer.Control;
		}

		protected SwitchCompat GetNativeControl(Switch @switch)
		{
			var renderer = GetRenderer(@switch);
			var viewRenderer = renderer.View as Platform.Android.AppCompat.SwitchRenderer;
			return viewRenderer.Control;
		}

		protected EditText GetNativeControl(TimePicker timePicker)
		{
			var renderer = GetRenderer(timePicker);
			var viewRenderer = renderer.View as TimePickerRenderer;
			return viewRenderer.Control;
		}

		protected void Layout(VisualElement element, AView nativeView) 
		{
			var size = element.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);
			var width = size.Request.Width;
			var height = size.Request.Height;
			element.Layout(new Rectangle(0, 0, width, height));

			int widthSpec = AView.MeasureSpec.MakeMeasureSpec((int)width, MeasureSpecMode.Exactly);
			int heightSpec = AView.MeasureSpec.MakeMeasureSpec((int)height, MeasureSpecMode.Exactly);
			nativeView.Measure(widthSpec, heightSpec);
			nativeView.Layout(0, 0, (int)width, (int)height);
		}

		// Some of the renderer properties aren't set until the renderer is actually 
		// attached to the view hierarchy; to test them, we need to add them, run the test,
		// then remove them
		protected void ParentView(AView view) 
		{
			((ViewGroup)Application.Current.MainPage.GetRenderer().View).AddView(view);
		}

		protected void UnparentView(AView view)
		{
			((ViewGroup)Application.Current.MainPage.GetRenderer().View).RemoveView(view);
		}
	}
}