using System.ComponentModel;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using CategoryAttribute = NUnit.Framework.CategoryAttribute;
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{

	public static class ContentDescriptionEffectProperties
	{
		public static readonly BindableProperty ContentDescriptionProperty = BindableProperty.CreateAttached(
			"ContentDescription",
			typeof(string),
			typeof(ContentDescriptionEffectProperties),
			null);

		public static readonly BindableProperty NameAndHelpTextProperty = BindableProperty.CreateAttached(
			"NameAndHelpText",
			typeof(string),
			typeof(ContentDescriptionEffectProperties),
			null);

		public static string GetContentDescription(BindableObject view)
		{
			return (string)view.GetValue(ContentDescriptionProperty);
		}

		public static string GetNameAndHelpText(BindableObject view)
		{
			return (string)view.GetValue(NameAndHelpTextProperty);
		}
	}

	public class ContentDescriptionEffect : RoutingEffect
	{
		public const string EffectName = "ContentDescriptionEffect";

		public ContentDescriptionEffect() : base($"{Effects.ResolutionGroupName}.{EffectName}")
		{
		}
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5150, "AutomationProperties.Name, AutomationProperties.HelpText on Button not read by Android TalkBack", PlatformAffected.Android)]
	public class Issue5150 : TestContentPage
	{
		void AddView(StackLayout layout, View view, string labelPrefix, string automationId, string buttonName = null, string buttonHelp = null)
		{
			var automationIdLabel = new Label();
			automationIdLabel.Text = $"AutomationId = {automationId}";
			automationIdLabel.AutomationId = $"{labelPrefix}-automationIdLabel";

			var contentDescriptionLabel = new Label();
			contentDescriptionLabel.AutomationId = $"{labelPrefix}-contentDescriptionLabel";

			var nameAndHelpTextLabel = new Label();
			nameAndHelpTextLabel.AutomationId = $"{labelPrefix}-nameAndHelpTextLabel";

			view.AutomationId = automationId;
			view.Effects.Add(new ContentDescriptionEffect());
			view.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
			{
				if (e.PropertyName == ContentDescriptionEffectProperties.ContentDescriptionProperty.PropertyName)
				{
					contentDescriptionLabel.Text = $"ContentDescription = {ContentDescriptionEffectProperties.GetContentDescription(view)}";
				}

				if (e.PropertyName == ContentDescriptionEffectProperties.NameAndHelpTextProperty.PropertyName)
				{
					nameAndHelpTextLabel.Text = $"Name + HelpText = {ContentDescriptionEffectProperties.GetNameAndHelpText(view)}";
				}
			};
			layout.Children.Add(view);
			layout.Children.Add(automationIdLabel);
			layout.Children.Add(contentDescriptionLabel);
			layout.Children.Add(nameAndHelpTextLabel);

			AutomationProperties.SetIsInAccessibleTree(view, true);
			AutomationProperties.SetName(view, buttonName);
			AutomationProperties.SetHelpText(view, buttonHelp);
		}

		void AddButton(StackLayout layout, string labelPrefix, string automationId, string buttonText, string buttonName = null, string buttonHelp = null)
		{
			var button = new Button();
			button.Text = buttonText;
			AddView(layout, button, labelPrefix, automationId, buttonName, buttonHelp);
		}

		protected override void Init()
		{
			var scrollView = new ScrollView();
			var layout = new StackLayout();
			scrollView.Content = layout;

			layout.Children.Add(new Label
			{
				Text = "On the Android platform, the 'Name + Help Text' " +
					"labels below each button should match the text read by " +
					"TalkBack."
			});

			AddButton(layout, "button1prop", "button1", "Button 1", "Name 1");
			AddButton(layout, "button2prop", "button2", "Button 2", buttonHelp: "Help 2.");
			AddButton(layout, "button3prop", "button3", "Button 3", "Name 3", "Help 3.");
			AddButton(layout, "button4prop", "button4", null, buttonHelp: "Help 4.");

			AddView(layout, new Switch(), "switch1prop", "switch1", "Switch 1 Name", "Switch Help 1.");

			var image = new Image();
			image.Source = ImageSource.FromFile("coffee.png");
			AddView(layout, image, "image1prop", "image1", "Coffee", "Get some coffee!");

			Content = scrollView;
		}

#if UITEST && __ANDROID__
		void Verify(string labelPrefix, string automationId, string expectedNameAndHelpText)
		{
			RunningApp.ScrollTo(automationId);
			RunningApp.WaitForElement(q => q.Marked(automationId));
			RunningApp.ScrollTo($"{labelPrefix}-nameAndHelpTextLabel");
			RunningApp.WaitForElement(q => q.Text($"Name + HelpText = {expectedNameAndHelpText}"));
		}

		[Test]
		[Category(UITestCategories.Button)]
		[Category(UITestCategories.Accessibility)]
		[Ignore("Automated Test not relevant until https://github.com/xamarin/Xamarin.Forms/issues/1529 is fixed")]
		public void Issue5150Test() 
		{
			Verify("button1prop", "button1", "Name 1");
			Verify("button2prop", "button2", "Button 2. Help 2.");
			Verify("button3prop", "button3", "Name 3. Help 3.");
			Verify("button4prop", "button4", "Help 4.");
			Verify("switch1prop", "switch1", "Switch 1 Name. Switch Help 1.");
			Verify("image1prop", "image1", "Coffee. Get some coffee!");
		}
#endif
	}
}