using System;
using System.Collections;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Tests
{
	[TestFixture]
	public class ObjectDisposedExceptionTests : CrossPlatformTestFixture
	{
		static IEnumerable VisualElementTestCases
		{
			get
			{
				yield return CreateTestCase(new Func<VisualElement>(() => new BoxView()), nameof(BoxView));
				yield return CreateTestCase(new Func<VisualElement>(() => new Button()), nameof(Button));
				yield return CreateTestCase(new Func<VisualElement>(() => new CheckBox()), nameof(CheckBox));
				yield return CreateTestCase(new Func<VisualElement>(() => new DatePicker()), nameof(DatePicker));
				yield return CreateTestCase(new Func<VisualElement>(() => new Editor()), nameof(Editor));
				yield return CreateTestCase(new Func<VisualElement>(() => new Entry()), nameof(Entry));
				yield return CreateTestCase(new Func<VisualElement>(() => new Frame()), nameof(Frame));
				yield return CreateTestCase(new Func<VisualElement>(() => new Image()), nameof(Image));
				yield return CreateTestCase(new Func<VisualElement>(() => new ImageButton()), nameof(ImageButton));
				yield return CreateTestCase(new Func<VisualElement>(() => new Label()), nameof(Label));
				yield return CreateTestCase(new Func<VisualElement>(() => new Picker()), nameof(Picker));
				yield return CreateTestCase(new Func<VisualElement>(() => new ProgressBar()), nameof(ProgressBar));
				yield return CreateTestCase(new Func<VisualElement>(() => new SearchBar()), nameof(SearchBar));
				yield return CreateTestCase(new Func<VisualElement>(() => new Slider()), nameof(Slider));
				yield return CreateTestCase(new Func<VisualElement>(() => new Stepper()), nameof(Stepper));
				yield return CreateTestCase(new Func<VisualElement>(() => new Switch()), nameof(Switch));
				yield return CreateTestCase(new Func<VisualElement>(() => new TimePicker()), nameof(TimePicker));
			}
		}

		static TestCaseData CreateTestCase(Func<VisualElement> createVisualElement, string category)
		{
			return new TestCaseData(createVisualElement).SetCategory(category).SetName($"GitHub9431_{category}");
		}

		[Test, TestCaseSource(nameof(VisualElementTestCases))]
		[Description("[Bug] ObjectDisposedException (BoxView inside CollectionView)")]
		public void GitHub9431(Func<VisualElement> createVisualElement)
		{
			var color1 = Colors.Linen;
			var color2 = Colors.HotPink;
			var model = new _9431Model() { BGColor = color1 };

			for (int m = 0; m < 3; m++)
			{
				var visualElement = createVisualElement();
				visualElement.SetBinding(VisualElement.BackgroundColorProperty, new Binding("BGColor"));
				visualElement.BindingContext = model;
				TestingPlatform.CreateRenderer(visualElement);

				if (m == 1)
				{
					GC.Collect();
				}

				model.BGColor = model.BGColor == color1 ? color2 : color1;
			}
		}
	}
}
