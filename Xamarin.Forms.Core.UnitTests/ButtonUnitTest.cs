using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ButtonUnitTest
		: CommandSourceTests<Button>
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		[Test]
		public void MeasureInvalidatedOnTextChange()
		{
			var button = new Button();

			bool fired = false;
			button.MeasureInvalidated += (sender, args) => fired = true;

			button.Text = "foo";
			Assert.True(fired);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
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

		[Test]
		[TestCase(true)]
		[TestCase(false)]
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

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void TestReleasedEvent(bool isEnabled)
		{
			var view = new Button()
			{
				IsEnabled = isEnabled,
			};

			bool released = false;
			view.Released += (sender, e) => released = true;

			((IButtonController)view).SendReleased();

			Assert.True(released == isEnabled ? true : false);
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


		[Test]
		public void TestBindingContextPropagation()
		{
			var context = new object();
			var button = new Button();
			button.BindingContext = context;
			var source = new FileImageSource();
			button.ImageSource = source;
			Assert.AreSame(context, source.BindingContext);

			button = new Button();
			source = new FileImageSource();
			button.ImageSource = source;
			button.BindingContext = context;
			Assert.AreSame(context, source.BindingContext);
		}

		[Test]
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

		[Test]
		public void AssignToFontStructUpdatesFontFamily(
			[Values(NamedSize.Default, NamedSize.Large, NamedSize.Medium, NamedSize.Small, NamedSize.Micro)] NamedSize size,
			[Values(FontAttributes.None, FontAttributes.Bold, FontAttributes.Italic, FontAttributes.Bold | FontAttributes.Italic)] FontAttributes attributes)
		{
			var button = new Button();
			double startSize = button.FontSize;
			var startAttributes = button.FontAttributes;

			bool firedSizeChanged = false;
			bool firedAttributesChanged = false;
			button.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == Label.FontSizeProperty.PropertyName)
					firedSizeChanged = true;
				if (args.PropertyName == Label.FontAttributesProperty.PropertyName)
					firedAttributesChanged = true;
			};

			button.Font = Font.OfSize("Testing123", size).WithAttributes(attributes);

			Assert.AreEqual(Device.GetNamedSize(size, typeof(Label), true), button.FontSize);
			Assert.AreEqual(attributes, button.FontAttributes);
			Assert.AreEqual(startSize != button.FontSize, firedSizeChanged);
			Assert.AreEqual(startAttributes != button.FontAttributes, firedAttributesChanged);
		}

		[Test]
		public void AssignToFontFamilyUpdatesFont()
		{
			var button = new Button();

			button.FontFamily = "CrazyFont";
			Assert.AreEqual(button.Font, Font.OfSize("CrazyFont", button.FontSize));
		}

		[Test]
		public void AssignToFontSizeUpdatesFont()
		{
			var button = new Button();

			button.FontSize = 1000;
			Assert.AreEqual(button.Font, Font.SystemFontOfSize(1000));
		}

		[Test]
		public void AssignToFontAttributesUpdatesFont()
		{
			var button = new Button();

			button.FontAttributes = FontAttributes.Italic | FontAttributes.Bold;
			Assert.AreEqual(button.Font, Font.SystemFontOfSize(button.FontSize, FontAttributes.Bold | FontAttributes.Italic));
		}

		[Test]
		public void CommandCanExecuteUpdatesEnabled()
		{
			var button = new Button();

			bool result = false;

			var bindingContext = new
			{
				Command = new Command(() => { }, () => result)
			};

			button.SetBinding(Button.CommandProperty, "Command");
			button.BindingContext = bindingContext;

			Assert.False(button.IsEnabled);

			result = true;

			bindingContext.Command.ChangeCanExecute();

			Assert.True(button.IsEnabled);
		}

		[Test]
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

		[Test]
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

		public void ButtonBorderRadiusForwardsToButtonCornerRadius()
		{
			var button = new Button();
			button.BorderRadius = 10;

			Assert.AreEqual(10, button.CornerRadius);
		}

		[Test]
		public void ButtonCornerRadiusForwardsToButtonBorderRadius()
		{
			var button = new Button();
			button.CornerRadius = 10;

			Assert.AreEqual(10, button.BorderRadius);
		}

		[Test]
		public void ButtonCornerRadiusClearValueForwardsToButtonBorderRadius()
		{
			var button = new Button();

			button.CornerRadius = 10;

			button.ClearValue(Button.CornerRadiusProperty);

			Assert.AreEqual((int)Button.BorderRadiusProperty.DefaultValue, button.BorderRadius);
		}

		[Test]
		public void ButtonBorderRadiusClearValueForwardsToButtonCornerRadius()
		{
			var button = new Button();

			button.BorderRadius = 10;

			button.ClearValue(Button.BorderRadiusProperty);

			Assert.AreEqual((int)Button.CornerRadiusProperty.DefaultValue, button.CornerRadius);
		}

		[Test]
		public void ButtonCornerRadiusSetToFive()
		{
			var button = new Button();

			button.CornerRadius = 25;
			Assert.AreEqual(25, button.CornerRadius);

			button.CornerRadius = 5;
			Assert.AreEqual(5, button.CornerRadius);
		}

		[Test]
		public void ButtonBorderRadiusSetMinusOne()
		{
			var button = new Button();

			button.BorderRadius = 25;
			Assert.AreEqual(25, button.BorderRadius);

			button.BorderRadius = -1;
			Assert.AreEqual(-1, button.BorderRadius);
		}

		private void AssertButtonContentLayoutsEqual(Button.ButtonContentLayout layout1, object layout2)
		{
			var bcl = (Button.ButtonContentLayout)layout2;

			Assert.AreEqual(layout1.Position, bcl.Position);
			Assert.AreEqual(layout1.Spacing, bcl.Spacing);
		}
	}
}