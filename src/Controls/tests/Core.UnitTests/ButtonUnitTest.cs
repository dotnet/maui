using System;
using Xunit;
using static Microsoft.Maui.Controls.Core.UnitTests.VisualStateTestHelpers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ButtonUnitTest : VisualElementCommandSourceTests<Button>
	{
		[Fact]
		public void MeasureInvalidatedOnTextChange()
		{
			var button = new Button();

			bool fired = false;
			button.MeasureInvalidated += (sender, args) => fired = true;

			button.Text = "foo";
			Assert.True(fired);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void TestClickedvent(bool isEnabled)
		{
			var view = new Button()
			{
				IsEnabled = isEnabled,
			};

			bool activated = false;
			view.Clicked += (sender, e) => activated = true;

			((IButtonController)view).SendClicked();

			Assert.True(activated == isEnabled ? true : false);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void TestPressedEvent(bool isEnabled)
		{
			var view = new Button()
			{
				IsEnabled = isEnabled,
			};

			bool pressed = false;
			view.Pressed += (sender, e) => pressed = true;

			((IButtonController)view).SendPressed();

			Assert.True(pressed == isEnabled ? true : false);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void TestReleasedEvent(bool isEnabled)
		{
			var view = new Button()
			{
				IsEnabled = isEnabled,
			};

			bool released = false;
			view.Released += (sender, e) => released = true;

			((IButtonController)view).SendReleased();

			// Released should always fire, even if the button is disabled
			// Otherwise, a press which disables a button will leave it in the
			// Pressed state forever
			Assert.True(released);
		}

		protected override Button CreateSource()
		{
			return new Button();
		}

		protected override void Activate(Button source)
		{
			((IButtonController)source).SendClicked();
		}

		protected override BindableProperty IsEnabledProperty
		{
			get { return Button.IsEnabledProperty; }
		}

		protected override BindableProperty CommandProperty
		{
			get { return Button.CommandProperty; }
		}

		protected override BindableProperty CommandParameterProperty
		{
			get { return Button.CommandParameterProperty; }
		}


		[Fact]
		public void TestBindingContextPropagation()
		{
			var context = new object();
			var button = new Button();
			button.BindingContext = context;
			var source = new FileImageSource();
			button.ImageSource = source;
			Assert.Same(context, source.BindingContext);

			button = new Button();
			source = new FileImageSource();
			button.ImageSource = source;
			button.BindingContext = context;
			Assert.Same(context, source.BindingContext);
		}

		[Fact]
		public void TestImageSourcePropertiesChangedTriggerResize()
		{
			var source = new FileImageSource();
			var button = new Button { ImageSource = source };
			bool fired = false;
			button.MeasureInvalidated += (sender, e) => fired = true;
			Assert.Null(source.File);
			source.File = "foo.png";
			Assert.NotNull(source.File);
			Assert.True(fired);
		}

		[Fact]
		public void AssignToFontFamilyUpdatesFont()
		{
			var button = new Button();

			button.FontFamily = "CrazyFont";
			Assert.Equal((button as ITextStyle).Font, Font.OfSize("CrazyFont", button.FontSize));
		}

		[Fact]
		public void AssignToFontSizeUpdatesFont()
		{
			var button = new Button();

			button.FontSize = 1000;
			Assert.Equal((button as ITextStyle).Font, Font.SystemFontOfSize(1000));
		}

		[Fact]
		public void AssignToFontAttributesUpdatesFont()
		{
			var button = new Button();

			button.FontAttributes = FontAttributes.Italic | FontAttributes.Bold;
			Assert.Equal((button as ITextStyle).Font, Font.SystemFontOfSize(button.FontSize, FontWeight.Bold, FontSlant.Italic));
		}

		[Fact]
		public void ButtonContentLayoutTypeConverterTest()
		{
			var converter = new Button.ButtonContentTypeConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));

			AssertButtonContentLayoutsEqual(new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 10), converter.ConvertFromInvariantString("left,10"));
			AssertButtonContentLayoutsEqual(new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Right, 10), converter.ConvertFromInvariantString("right"));
			AssertButtonContentLayoutsEqual(new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top, 20), converter.ConvertFromInvariantString("top,20"));
			AssertButtonContentLayoutsEqual(new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 15), converter.ConvertFromInvariantString("15"));
			AssertButtonContentLayoutsEqual(new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Bottom, 0), converter.ConvertFromInvariantString("Bottom, 0"));

			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString(""));
		}

		[Fact]
		public void ButtonClickWhenCommandCanExecuteFalse()
		{
			bool invoked = false;
			var button = new Button()
			{
				Command = new Command(() => invoked = true
				, () => false),
			};

			(button as IButtonController)
				?.SendClicked();

			Assert.False(invoked);
		}

		[Fact]
		public void ButtonCornerRadiusSetToFive()
		{
			var button = new Button();

			button.CornerRadius = 25;
			Assert.Equal(25, button.CornerRadius);

			button.CornerRadius = 5;
			Assert.Equal(5, button.CornerRadius);
		}

		private void AssertButtonContentLayoutsEqual(Button.ButtonContentLayout layout1, object layout2)
		{
			var bcl = (Button.ButtonContentLayout)layout2;

			Assert.Equal(layout1.Position, bcl.Position);
			Assert.Equal(layout1.Spacing, bcl.Spacing);
		}

		[Fact]
		public void PressedVisualState()
		{
			var vsgList = CreateTestStateGroups();
			var stateGroup = vsgList[0];
			var element = new Button();
			VisualStateManager.SetVisualStateGroups(element, vsgList);

			element.SendPressed();
			Assert.Equal(PressedStateName, stateGroup.CurrentState.Name);

			element.SendReleased();
			Assert.NotEqual(PressedStateName, stateGroup.CurrentState.Name);
		}
	}
}
