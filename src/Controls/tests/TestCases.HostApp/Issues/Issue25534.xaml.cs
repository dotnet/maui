#nullable enable
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25534, "Updating IconImageSource in ToolbarItem multiple times causes exception and crash after navigating back and forth between Shell pages",
		PlatformAffected.iOS)]
	public partial class Issue25534 : Shell
	{

		public Issue25534()
		{
			InitializeComponent();
		}
	}

	public class Issue25534HomePage : ContentPage
	{
		public Issue25534HomePage()
		{
			Title = "HomePage";
			var viewModel = new Issue25534PageModel();
			this.BindingContext = viewModel;

			var button = new Button
			{
				Text = "Go To Second Page",
				AutomationId = "GoToSecondPage",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			button.Clicked += async (sender, e) =>
			{
				var secondaryPage = new SecondaryPage(viewModel);
				await Navigation.PushAsync(secondaryPage);
			};
			var stack = new StackLayout();
			stack.Children.Add(button);

			this.Content = stack;
		}
	}

	public class SecondaryPage : ContentPage
	{
		private readonly Issue25534PageModel ViewModel;

		internal SecondaryPage(Issue25534PageModel vm)
		{
			BindingContext = ViewModel = vm;
			Title = "Secondary Page";
			var noteToolbarItem = new ToolbarItem
			{
				Text = "Notes",
			};
			noteToolbarItem.SetBinding(ToolbarItem.IconImageSourceProperty, nameof(ViewModel.NoteIcon));
			var testToolbarItem = new ToolbarItem
			{
				Text = "Test"
			};
			ToolbarItems.Add(noteToolbarItem);
			ToolbarItems.Add(testToolbarItem);
			var headerLabel = new Label
			{
				Text = "Secondary Page",
				AutomationId = "SecondPageLabel",
				HorizontalOptions = LayoutOptions.Center
			};
			var stackLayout = new VerticalStackLayout
			{
				Padding = new Thickness(30, 0),
				Spacing = 25,
				Children =
				{
					headerLabel
				}
			};
			Content = stackLayout;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			ViewModel.SetIcon();
		}
	}
	internal partial class Issue25534PageModel : INotifyPropertyChanged
	{
		//As per MS doc this should be a valid image path with file extension and the SVG file in .NET MAUI app project should be referenced with a .png extension. Refer https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/image?view=net-maui-8.0#load-a-font-icon
		protected const string ButtonEmpty = "dotnet_bot_resized2.png";
		protected const string ButtonNote = "dotnet_bot_resized3.png";

		private string? noteIcon = ButtonEmpty;
		public string? NoteIcon
		{
			get => noteIcon;
			set => SetProperty(ref noteIcon, value);
		}

		public void SetIcon()
		{
			NoteIcon = ButtonEmpty; //Setting toolbaritem multiple times to replicate the issue.
			NoteIcon = ButtonNote;
			NoteIcon = ButtonEmpty;
			NoteIcon = ButtonNote;
		}

		protected bool SetProperty<T>(ref T backingStore, T value,
		[CallerMemberName] string propertyName = "",
		Action? onChanged = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
			{
				return false;
			}

			backingStore = value;
			onChanged?.Invoke();
			OnPropertyChanged(propertyName);
			return true;
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{

			PropertyChangedEventHandler? changed = PropertyChanged;
			if (changed == null)
			{
				return;
			}

			changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

}
