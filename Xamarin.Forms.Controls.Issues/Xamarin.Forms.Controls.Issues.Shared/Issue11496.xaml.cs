using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.SwipeView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11496, "[Bug] Issue with SwipeView not working since Xamarin.Forms update v4.7.0.1080 and above on Android",
		PlatformAffected.Android)]
	public partial class Issue11496 : TestContentPage
	{
		public Issue11496()
		{
#if APP
			Title = "Issue 11496";
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
	}

	[Preserve(AllMembers = true)]
	public partial class Issue11496ItemControl : ContentView
	{
		readonly Label _label;

		public Issue11496ItemControl()
		{
			var layout = new Grid();

			_label = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			layout.Children.Add(_label);

			Content = layout;

			var tapCommand = new TapGestureRecognizer
			{
				Command = Command,
				CommandParameter = CommandParameter
			};

			layout.GestureRecognizers.Add(tapCommand);

			tapCommand.Tapped += (sender, args) =>
			{
				if (TestItem != null)
					Application.Current.MainPage.DisplayAlert("Issue11496", $"Tapped {((Issue11496Item)TestItem).Name}", "Ok");
			};
		}

		public static readonly BindableProperty TestItemProperty =
			BindableProperty.Create(nameof(TestItem), typeof(object), typeof(Issue11496ItemControl));

		public static readonly BindableProperty CommandProperty =
			BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(Issue11496ItemControl));

		public static readonly BindableProperty CommandParameterProperty =
			BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(Issue11496ItemControl));

		public object TestItem
		{
			get => GetValue(TestItemProperty);
			set => SetValue(TestItemProperty, value);
		}

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

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			if (TestItem != null)
				_label.Text = ((Issue11496Item)TestItem).Name;

		}
	}

	[Preserve(AllMembers = true)]
	public class Issue11496Item
	{
		public string Name { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue11496ViewModel : BindableObject
	{
		List<Issue11496Item> _items;
		ICommand _command;
		object _commandParam;

		public List<Issue11496Item> Items
		{
			get => _items;
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		public ICommand Command
		{
			get => _command;
			set
			{
				_command = value;
				OnPropertyChanged();
			}
		}

		public object CommandParameter
		{
			get => _commandParam;
			set
			{
				_commandParam = value;
				OnPropertyChanged();
			}
		}

		public Issue11496ViewModel()
		{
			Items = new List<Issue11496Item>()
			{
				new Issue11496Item() { Name = "Test One" },
				new Issue11496Item() { Name = "Test Two" },
				new Issue11496Item() { Name = "Test Three" },
				new Issue11496Item() { Name = "Test Four" },
				new Issue11496Item() { Name = "Test Five" },
				new Issue11496Item() { Name = "Test Six" },
				new Issue11496Item() { Name = "Test Seven" },
				new Issue11496Item() { Name = "Test Eight" },
			};
		}
	}
}