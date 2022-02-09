using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.UI.Xaml.Controls;
using NUnit.Framework;
using WBorder = Microsoft.UI.Xaml.Controls.Border;

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

		protected Control GetPlatformControl(VisualElement element)
		{
			return GetRenderer(element).GetPlatformElement() as Control;
		}

		protected Panel GetPanel(VisualElement element)
		{
			return GetRenderer(element).ContainerElement as Panel;
		}

		protected WBorder GetBorder(VisualElement element)
		{
			var renderer = GetRenderer(element);
			var nativeElement = renderer.GetPlatformElement();
			return nativeElement as WBorder;
		}

		protected TextBlock GetPlatformControl(Label label)
		{
			return GetRenderer(label).GetPlatformElement() as TextBlock;
		}

		protected async Task<TProperty> GetControlProperty<TProperty>(Label label, Func<TextBlock, TProperty> getProperty)
		{
			return await Device.InvokeOnMainThreadAsync(() =>
			{
				var textBlock = GetPlatformControl(label);
				return getProperty(textBlock);
			});
		}

		protected FormsButton GetPlatformControl(Button button)
		{
			return GetRenderer(button).GetPlatformElement() as FormsButton;
		}

		protected FormsTextBox GetPlatformControl(Entry entry)
		{
			return GetRenderer(entry).GetPlatformElement() as FormsTextBox;
		}

		protected FormsTextBox GetPlatformControl(Editor editor)
		{
			return GetRenderer(editor).GetPlatformElement() as FormsTextBox;
		}

		protected async Task<TProperty> GetRendererProperty<TProperty>(View view,
			Func<IVisualElementRenderer, TProperty> getProperty)
		{
			return await Device.InvokeOnMainThreadAsync(() =>
			{
				var renderer = GetRenderer(view);
				return getProperty(renderer);
			});
		}
	}
}
