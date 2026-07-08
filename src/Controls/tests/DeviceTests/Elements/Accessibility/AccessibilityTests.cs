#if !WINDOWS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Accessibility)]
	public partial class AccessibilityTests : ControlsHandlerTestBase
	{
		[Theory("IsInAccessibleTree initializes correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task IsInAccessibleTree(bool result)
		{
			var button = new Button();
			AutomationProperties.SetIsInAccessibleTree(button, result);
			var important = await GetValueAsync<bool, ButtonHandler>(button, handler => button.IsAccessibilityElement());
			Assert.Equal(result, important);
		}

#if !WINDOWS
		[Theory("ExcludedWithChildren initializes correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ExcludedWithChildren(bool result)
		{
			var button = new Button();
			AutomationProperties.SetExcludedWithChildren(button, result);
			var excluded = await GetValueAsync<bool, ButtonHandler>(button, handler => button.IsExcludedWithChildren());
			Assert.Equal(result, excluded);
		}
#endif

		[Category(TestCategory.Accessibility)]
		[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
		public class InNewWindowCollection : ControlsHandlerTestBase
		{
			[Fact(
#if MACCATALYST
			Skip = "Fails on Mac Catalyst, fixme"
#endif
			)]
			public async Task ValidateIsImportantForAccessibility()
			{
				EnsureHandlerCreated(builder =>
				{
					builder.ConfigureMauiHandlers(handlers =>
					{
						handlers.AddMauiControlsHandlers();
						handlers.AddHandler(typeof(Window), typeof(WindowHandlerStub));
					});
				});

				// Ideally this would use a theory and class data for each control type.
				//
				// To accurately test this we need to attach the elements to the UI Tree.
				// Currently tests that attach to the UI Tree can't run in parallel so it slows everyone down.
				// So, I opted to just attach a layout and then iterate over the controls.
				// I left the test case class in case it makes sense to switch in the future. 

				var layout = new VerticalStackLayout();
				var contentPage = new ContentPage()
				{
					Content = new ScrollView()
					{
						Content = layout
					}
				};

				var expectedResults = new Dictionary<IView, bool>();

				foreach (var control in new AllControlsTestCase())
				{
					var view = (IView)Activator.CreateInstance((Type)control[0]);
					if (view is View ve)
					{
						ve.WidthRequest = 50;
						ve.HeightRequest = 50;

						MockAccessibilityExpectations(ve);
					}

					if (view is IPropertyMapperView pmv)
					{
						var viewMapper = pmv.GetPropertyMapperOverrides();
					}

					layout.Add(view);
					expectedResults.Add(view, (bool)control[1]);
				}

				expectedResults.Add(contentPage, false);

				var stringBuilder = new StringBuilder();

				await CreateHandlerAndAddToWindow<IWindowHandler>(contentPage, async (handler) =>
				{
					// make sure everything has finished laying out
					await Task.Delay(100);
					foreach (var expected in expectedResults)
					{
						var actual = expected.Key.IsAccessibilityElement();
						if (expected.Value != actual)
						{
							stringBuilder.AppendLine($"{expected.Key} Expected: {expected.Value} Actual: {actual}");
						}
					}
				});

				Assert.True(stringBuilder.Length == 0, stringBuilder.ToString());
			}
		}

		class LabelWithText : Label
		{
			// on iOS UILabel will only be an accessible element if it has text
			public LabelWithText()
			{
				Text = "testing";
			}
		}

		class AllControlsTestCase : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
#if IOS
				yield return new object[] { typeof(ActivityIndicator), false };
#else
				yield return new object[] { typeof(ActivityIndicator), true };
#endif
				yield return new object[] { typeof(Border), false };
				yield return new object[] { typeof(BoxView), false };
				yield return new object[] { typeof(Button), true };
#if IOS
				yield return new object[] { typeof(CarouselView), false };
#else
				yield return new object[] { typeof(CarouselView), true };
#endif
				yield return new object[] { typeof(CheckBox), true };
#if IOS
				yield return new object[] { typeof(CollectionView), false };
#else
				yield return new object[] { typeof(CollectionView), true };
#endif
				yield return new object[] { typeof(ContentView), false };
				yield return new object[] { typeof(DatePicker), true };
				yield return new object[] { typeof(Editor), true };
				yield return new object[] { typeof(Ellipse), false };
				yield return new object[] { typeof(Entry), true };
				yield return new object[] { typeof(GraphicsView), false };
				yield return new object[] { typeof(Grid), false };
				yield return new object[] { typeof(HorizontalStackLayout), false };
				yield return new object[] { typeof(Image), false };
				yield return new object[] { typeof(ImageButton), true };
#if IOS
				yield return new object[] { typeof(IndicatorView), false };
#else
				// https://github.com/dotnet/maui/issues/10404
				yield return new object[] { typeof(IndicatorView), false };
#endif
#if IOS
				yield return new object[] { typeof(Label), false };
#else
				yield return new object[] { typeof(Label), true };
#endif
				yield return new object[] { typeof(LabelWithText), true };
				yield return new object[] { typeof(Line), false };
				yield return new object[] { typeof(Path), false };
				yield return new object[] { typeof(Picker), true };
				yield return new object[] { typeof(Polygon), false };
				yield return new object[] { typeof(Polyline), false };
				yield return new object[] { typeof(ProgressBar), true };
				yield return new object[] { typeof(RadioButton), true };
				yield return new object[] { typeof(Rectangle), false };
				yield return new object[] { typeof(RefreshView), false };
				yield return new object[] { typeof(RoundRectangle), false };
#if IOS
				yield return new object[] { typeof(ScrollView), false };
#else
				yield return new object[] { typeof(ScrollView), true };
#endif
				yield return new object[] { typeof(SearchBar), true };
				yield return new object[] { typeof(Slider), true };
#if IOS
				yield return new object[] { typeof(Stepper), false };
#else
				yield return new object[] { typeof(Stepper), true };
#endif
				yield return new object[] { typeof(SwipeView), false };
				yield return new object[] { typeof(Switch), true };
				yield return new object[] { typeof(TimePicker), true };
				yield return new object[] { typeof(VerticalStackLayout), false };
#if IOS
				yield return new object[] { typeof(WebView), false };
#else
				yield return new object[] { typeof(WebView), true };
#endif
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
#endif
