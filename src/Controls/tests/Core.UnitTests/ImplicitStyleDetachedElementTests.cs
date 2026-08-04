using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	// https://github.com/dotnet/maui/issues/36822
	// Implicit styles from Application.Current.Resources must not be applied while a
	// detached element is still inside its constructor chain (the removed "XF previewer"
	// fallback in ResourcesExtensions.TryGetResource did exactly that, running user
	// propertyChanged callbacks against partially-constructed controls).
	public class ImplicitStyleDetachedElementTests : BaseTestFixture
	{
		class CustomControl : ContentView
		{
			public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
				nameof(TextColor), typeof(Color), typeof(CustomControl), Colors.Black,
				propertyChanged: (bindable, oldValue, newValue) => ((CustomControl)bindable)._label.TextColor = (Color)newValue);

			readonly Label _label;

			public Color TextColor
			{
				get => (Color)GetValue(TextColorProperty);
				set => SetValue(TextColorProperty, value);
			}

			public CustomControl()
			{
				_label = new Label { TextColor = TextColor };
				Content = _label;
			}

			public Label Label => _label;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Application.ClearCurrent();
			}
			base.Dispose(disposing);
		}

		static Application SetUpAppWithImplicitStyle()
		{
			var app = new MockApplication
			{
				Resources = new ResourceDictionary
				{
					new Style(typeof(CustomControl))
					{
						Setters =
						{
							new Setter { Property = CustomControl.TextColorProperty, Value = Colors.Red }
						}
					}
				}
			};
			Application.Current = app;
			return app;
		}

		[Fact]
		public void ImplicitAppStyleDoesNotApplyDuringConstructionOfDetachedElement()
		{
			SetUpAppWithImplicitStyle();

			// Must not throw: user propertyChanged callbacks cannot be invoked while the
			// instance is still inside its constructor chain
			var control = new CustomControl();

			// A detached element must not resolve implicit styles from the static
			// Application.Current.Resources; they apply through resource inheritance on attach
			Assert.Equal(Colors.Black, control.TextColor);
			Assert.Equal(Colors.Black, control.Label.TextColor);
		}

		[Fact]
		public void ImplicitAppStyleAppliesWhenElementIsAttached()
		{
			var app = SetUpAppWithImplicitStyle();

			var control = new CustomControl();
			app.LoadPage(new ContentPage { Content = control });

			Assert.Equal(Colors.Red, control.TextColor);
			Assert.Equal(Colors.Red, control.Label.TextColor);
		}
	}
}
