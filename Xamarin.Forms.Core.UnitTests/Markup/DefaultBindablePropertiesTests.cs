using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace Xamarin.Forms.Markup.UnitTests
{
	using XamarinFormsMarkupUnitTestsDefaultBindablePropertiesViews;

	[TestFixture(true)]
	[TestFixture(false)]
	public class DefaultBindablePropertiesTests : MarkupBaseTestFixture
	{
		public DefaultBindablePropertiesTests(bool withExperimentalFlag) : base(withExperimentalFlag) { }

		[Test]
		public void AllBindableElementsInCoreHaveDefaultBindablePropertyOrAreExcluded()
		{
			const string na = "not applicable", tbd = "to be determined";
			var excludedTypeReasons = new Dictionary<Type, string>
			{ // Key: type, Value: reason why it does not have a default bindable property
				{ typeof(Application), na },
				{ typeof(AdaptiveTrigger), na },
				{ typeof(BaseMenuItem), na },
				{ typeof(BaseShellItem), na },
				{ typeof(Behavior), na },
				{ typeof(BindableObject), na },
				{ typeof(CarouselView), na },
				{ typeof(Cell), na },
				{ typeof(ColumnDefinition), na },
				{ typeof(CompareStateTrigger), na },
				{ typeof(DataTrigger), na },
				{ typeof(DeviceStateTrigger), na },
				{ typeof(Element), na },
				{ typeof(EventTrigger), na },
				{ typeof(FontImageSource), na },
				{ typeof(FormattedString), na },
				{ typeof(GestureElement), na },
				{ typeof(GestureRecognizer), na },
				{ typeof(GridItemsLayout), na },
				{ typeof(GroupableItemsView), na },
				{ typeof(ImageSource), na },
				{ typeof(InputView), na },
				{ typeof(ItemsLayout), na },
				{ typeof(LinearItemsLayout), na },
				{ typeof(MediaSource), na },
				{ typeof(Menu), na },
				{ typeof(MultiTrigger), na },
				{ typeof(NavigableElement), na },
				{ typeof(OpenGLView), na },
				{ typeof(OrientationStateTrigger), na },
				{ typeof(PanGestureRecognizer), na },
				{ typeof(PinchGestureRecognizer), na },
				{ typeof(RowDefinition), na },
				{ typeof(SelectableItemsView), na },
				{ typeof(StateTrigger), na },
				{ typeof(StateTriggerBase), na },
				{ typeof(StructuredItemsView), na },
				{ typeof(SwipeItems), na },
				{ typeof(TableRoot), na },
				{ typeof(TableSection), na },
				{ typeof(TableView), na },
				{ typeof(Trigger), na },
				{ typeof(TriggerBase), na },
				{ typeof(View), na },
				{ typeof(ViewCell), na },
				{ typeof(VisualElement), na },
				{ typeof(WebViewSource), na },

				{ typeof(AppLinkEntry), tbd },
				{ typeof(FlyoutItem), tbd },
				{ typeof(Shell), tbd },
				{ typeof(ShellContent), tbd },
				{ typeof(ShellGroupItem), tbd },
				{ typeof(ShellItem), tbd },
				{ typeof(ShellSection), tbd },
				{ typeof(Tab), tbd },
				{ typeof(TabBar), tbd }
			};

			var failMessage = new StringBuilder();
			var bindableObjectTypes = typeof(BindableObject).Assembly.GetExportedTypes()
				.Where(t => typeof(BindableObject).IsAssignableFrom(t) && !typeof(Layout).IsAssignableFrom(t) && !t.ContainsGenericParameters);
			// The logical default property for a Layout is for its child view(s), which is not a bindable property. 
			// So we exclude Layouts from this test. Note that it is still perfectly OK to define a default 
			// bindable property for a Layout where that makes sense.
			// We also do not support specifying default properties for unconstructed generic types.

			foreach (var type in bindableObjectTypes)
			{
				if (excludedTypeReasons.TryGetValue(type, out string exclusionReason))
				{
					Console.WriteLine($"Info: no default BindableProperty defined for BindableObject type {type.FullName} because {exclusionReason}");
					continue;
				}

				if (DefaultBindableProperties.GetFor(type) == null)
				{
					failMessage.AppendLine(type.FullName);
					var propertyNames = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
						.Where(f => f.FieldType == typeof(BindableProperty)).Select(f => f.DeclaringType.Name + "." + f.Name).ToList();
					if (propertyNames.Count > 0)
					{
						failMessage.AppendLine("\tCandidate properties:");
						foreach (var propertyName in propertyNames)
							failMessage.Append("\t").AppendLine(propertyName);
					}
				}
			}

			if (failMessage.Length > 0)
				Assert.Fail(
					$"Missing default BindableProperty / exclusion for BindableObject types:\n{failMessage}\n" +
					$"Either register these types in {typeof(DefaultBindableProperties).FullName} or exclude them in this test"
				);
		}

		[Test]
		public void GetDefaultBindablePropertyForBuiltInType()
			=> Assert.That(DefaultBindableProperties.GetFor(new Label()), Is.Not.Null);

		[Test]
		public void GetDefaultBindablePropertyForDerivedType()
			=> Assert.That(DefaultBindableProperties.GetFor(new DerivedFromBoxView()), Is.Not.Null);

		[Test]
		public void GetDefaultBindablePropertyForUnsupportedType()
			=> Assert.Throws<ArgumentException>(
				() => DefaultBindableProperties.GetFor(new CustomView()),
				"No default bindable property is registered for BindableObject type XamarinFormsMarkupUnitTestsDefaultBindablePropertiesViews.CustomView" +
				"\r\nEither specify a property when calling Bind() or register a default bindable property for this BindableObject type");

		[Test]
		public void RegisterDefaultBindableProperty()
		{
			var v = new CustomViewWithText();
			Assert.Throws<ArgumentException>(() => DefaultBindableProperties.GetFor(v));

			AssertExperimental(() => DefaultBindableProperties.Register(CustomViewWithText.TextProperty));

			if (withExperimentalFlag)
				Assert.That(DefaultBindableProperties.GetFor(v), Is.EqualTo(CustomViewWithText.TextProperty));
		}

		[Test]
		public void GetDefaultBindableCommandPropertiesForBuiltInType()
			=> Assert.That(DefaultBindableProperties.GetForCommand(new Button()), Is.Not.Null);

		[Test]
		public void GetDefaultBindableCommandPropertiesForDerivedType()
			=> Assert.That(DefaultBindableProperties.GetFor(new DerivedFromButton()), Is.Not.Null);

		[Test]
		public void GetDefaultBindableCommandPropertiesForUnsupportedType()
			=> Assert.Throws<ArgumentException>(
				() => DefaultBindableProperties.GetFor(new CustomView()),
				"No command + command parameter properties are registered for BindableObject type XamarinFormsMarkupUnitTestsDefaultBindablePropertiesViews.CustomView" +
				"\r\nRegister command + command parameter properties for this BindableObject type");

		[Test]
		public void RegisterDefaultBindableCommandProperties()
		{
			var v = new CustomViewWithCommand();
			Assert.Throws<ArgumentException>(() => DefaultBindableProperties.GetForCommand(v));

			AssertExperimental(() => DefaultBindableProperties.RegisterForCommand((CustomViewWithCommand.CommandProperty, CustomViewWithCommand.CommandParameterProperty)));

			if (withExperimentalFlag)
				Assert.That(DefaultBindableProperties.GetForCommand(v), Is.EqualTo((CustomViewWithCommand.CommandProperty, CustomViewWithCommand.CommandParameterProperty)));
		}

		[TearDown]
		public override void TearDown()
		{
			if (DefaultBindableProperties.GetFor(typeof(CustomViewWithText)) != null)
				DefaultBindableProperties.Unregister(CustomViewWithText.TextProperty);

			if (DefaultBindableProperties.GetForCommand(typeof(CustomViewWithCommand)) != (null, null))
				DefaultBindableProperties.UnregisterForCommand(CustomViewWithCommand.CommandProperty);
			base.TearDown();
		}
	}
}

namespace XamarinFormsMarkupUnitTestsDefaultBindablePropertiesViews
{
	using System.Windows.Input;
	using Xamarin.Forms;

	internal class DerivedFromBoxView : BoxView { }
	internal class DerivedFromButton : Button { }

	internal class CustomView : View { }

	internal class CustomViewWithText : View
	{
		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(CustomViewWithText), default(string));

		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}
	}

	internal class CustomViewWithCommand : View
	{
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(CustomViewWithCommand), default(ICommand));
		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(CustomViewWithCommand), default(object));

		public ICommand Command
		{
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		public object CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}
	}
}