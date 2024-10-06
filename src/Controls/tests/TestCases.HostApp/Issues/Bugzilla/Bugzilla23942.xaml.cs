using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 23942, "Cannot bind properties in BindableObjects added to static resources in XAML", PlatformAffected.All)]
	public partial class Bugzilla23942 : TestContentPage
	{
		[Preserve(AllMembers = true)]
		public class TestViewModel : ViewModelBase
		{
			string _doesItWork;
			public string DoesItWork
			{
				get
				{
					return _doesItWork;
				}
				set
				{
					_doesItWork = value;
					OnPropertyChanged();
				}
			}
		}

		public Bugzilla23942()
		{
			InitializeComponent();
		}

		private void InitializeView()
		{
			TestViewModel vm = new TestViewModel() { DoesItWork = "initial binding works" };
			BindingContext = vm;
			vm.DoesItWork = "success";
		}

		protected override void Init()
		{
			InitializeView();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			var lbl = this.FindByName<Bugzilla23942Label>("label");
			lbl.Text = lbl.Options.Text;
		}
	}

	[Preserve(AllMembers = true)]
	public class Bugzilla23942Options : BindableObject
	{
		public static readonly BindableProperty TextProperty =
			BindableProperty.Create(propertyName: nameof(Text),
									returnType: typeof(string),
									declaringType: typeof(Bugzilla23942Options),
									defaultValue: default(string));

		public string Text
		{
			get
			{
				return (string)GetValue(TextProperty);
			}
			set
			{
				SetValue(TextProperty, value);
			}
		}
	}

	[Preserve(AllMembers = true)]
	public class Bugzilla23942Label : Label
	{
		public static readonly BindableProperty OptionsProperty =
			BindableProperty.Create(propertyName: nameof(Options),
									returnType: typeof(Bugzilla23942Options),
									declaringType: typeof(Bugzilla23942Label),
									defaultValue: default(Bugzilla23942Options));

		public Bugzilla23942Options Options
		{
			get
			{
				return (Bugzilla23942Options)GetValue(OptionsProperty);
			}
			set
			{
				SetValue(OptionsProperty, value);
			}
		}
	}
}
