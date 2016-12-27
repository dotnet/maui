using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

// Apply the default category of "Issues" to all of the tests in this assembly
// We use this as a catch-all for tests which haven't been individually categorized
#if UITEST
[assembly: NUnit.Framework.Category("Issues")]
#endif

namespace Xamarin.Forms.Controls.Issues
{
	public class Bugzilla41054NumericValidationBehavior : Behavior<Entry>
	{
		public static readonly BindableProperty AttachBehaviorProperty =
			BindableProperty.CreateAttached("AttachBehavior", typeof(bool), typeof(Bugzilla41054NumericValidationBehavior), false, propertyChanged: OnAttachBehaviorChanged);

		public static bool GetAttachBehavior(BindableObject view)
		{
			return (bool)view.GetValue(AttachBehaviorProperty);
		}

		public static void SetAttachBehavior(BindableObject view, bool value)
		{
			view.SetValue(AttachBehaviorProperty, value);
		}

		static void OnAttachBehaviorChanged(BindableObject view, object oldValue, object newValue)
		{
			var entry = view as Entry;
			if (entry == null)
			{
				return;
			}

			bool attachBehavior = (bool)newValue;
			if (attachBehavior)
			{
				entry.Behaviors.Add(new Bugzilla41054NumericValidationBehavior());
			}
			else
			{
				var toRemove = entry.Behaviors.FirstOrDefault(b => b is Bugzilla41054NumericValidationBehavior);
				if (toRemove != null)
				{
					entry.Behaviors.Remove(toRemove);
				}
			}
		}

		protected override void OnAttachedTo(Entry entry)
		{
			entry.TextChanged += OnEntryTextChanged;
			base.OnAttachedTo(entry);
		}

		protected override void OnDetachingFrom(Entry entry)
		{
			entry.TextChanged -= OnEntryTextChanged;
			base.OnDetachingFrom(entry);
		}

		void OnEntryTextChanged(object sender, TextChangedEventArgs args)
		{
			double result;
			bool isValid = double.TryParse(args.NewTextValue, out result);
			((Entry)sender).TextColor = isValid ? Color.Default : Color.Red;
		}
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41054, "Cannot update Entry.Text from attached Behavior on UWP", PlatformAffected.Default)]
	public class Bugzilla41054 : TestContentPage
	{
		protected override void Init()
		{
			var entry = new Entry
			{
				Placeholder = "Enter a System.Double; text will be red when invalid",
				PlaceholderColor = Color.Green,
			};
			var entry2 = new Entry
			{
				Placeholder = "This entry starts with blue text when typing",
				TextColor = Color.Blue
			};

			Bugzilla41054NumericValidationBehavior.SetAttachBehavior(entry, true);

			Content = new StackLayout
			{
				Children = {
					entry,
					entry2,
					new Entry
					{
						Text = "This is an entry with some default colored text"
					},
					new Button
					{
						Text = "Change first entry placeholder color to purple",
						Command = new Command(() => entry.PlaceholderColor = Color.Purple)
					},
					new Button
					{
						Text = "Change second entry text color to orange",
						Command = new Command(() => entry2.TextColor = Color.Orange)
					}
				}
			};
		}
	}
}
