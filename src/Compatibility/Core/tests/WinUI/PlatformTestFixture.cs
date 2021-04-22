using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UAP.UnitTests
{
	public class PlatformTestFixture
	{
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

		protected static TestCaseData CreateTestCase(VisualElement element)
		{
			// We set the element type as a category on the test so that if you 
			// filter by category, say, "Button", you'll get any Button test 
			// generated from here. 

			return new TestCaseData(element).SetCategory(element.GetType().Name);
		}

		protected IVisualElementRenderer GetRenderer(VisualElement element)
		{
			return element.GetOrCreateRenderer();
		}

		protected Control GetNativeControl(VisualElement element)
		{
			return GetRenderer(element).GetNativeElement() as Control;
		}

		protected Panel GetPanel(VisualElement element) 
		{
			return GetRenderer(element).ContainerElement as Panel;
		}

		protected Border GetBorder(VisualElement element) 
		{
			var renderer = GetRenderer(element);
			var nativeElement = renderer.GetNativeElement();
			return nativeElement as Border;
		}
		
		protected TextBlock GetNativeControl(Label label)
		{
			return GetRenderer(label).GetNativeElement() as TextBlock;
		}

		protected async Task<TProperty> GetControlProperty<TProperty>(Label label, Func<TextBlock, TProperty> getProperty)
		{
			return await Device.InvokeOnMainThreadAsync(() => {
				var textBlock = GetNativeControl(label);
				return getProperty(textBlock);
			});
		}

		protected FormsButton GetNativeControl(Button button)
		{
			return GetRenderer(button).GetNativeElement() as FormsButton;
		}

		protected FormsTextBox GetNativeControl(Entry entry)
		{
			return GetRenderer(entry).GetNativeElement() as FormsTextBox;
		}

		protected FormsTextBox GetNativeControl(Editor editor)
		{
			return GetRenderer(editor).GetNativeElement() as FormsTextBox;
		}

		protected async Task<TProperty> GetRendererProperty<TProperty>(View view,
			Func<IVisualElementRenderer, TProperty> getProperty)
		{
			return await Device.InvokeOnMainThreadAsync(() => {
				var renderer = GetRenderer(view);
				return getProperty(renderer);
			});
		}
	}
}
