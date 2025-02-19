using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class PlatformBindingGalleryPage : ContentPage
	{
		public new StackLayout Layout { get; set; }
		public bool PlatformControlsAdded { get; set; }

		NestedPlatformViewModel ViewModel { get; set; }

		public const string ReadyForPlatformBindingsMessage = "ReadyForPlatformBindings";

		protected override void OnAppearing()
		{
			base.OnAppearing();
			MessagingCenter.Send(this, ReadyForPlatformBindingsMessage);
		}

		public PlatformBindingGalleryPage()
		{

			var vm = new NestedPlatformViewModel();
			vm.FormsLabel = "Forms Label Binding";
			vm.PlatformLabel = "Native Label Binding";
			vm.PlatformLabelColor = Colors.Red;
			vm.Age = 45;

			Layout = new StackLayout { Padding = 20, VerticalOptions = LayoutOptions.FillAndExpand };

			var buttonNav = new Button { Text = "New Page" };
			buttonNav.Clicked += (object sender, EventArgs e) =>
			{
				App.Current.MainPage = new ContentPage { Content = new Label { Text = "New page" } };
			};

			var button = new Button { Text = "Change BindingContext " };
			button.Clicked += (object sender, EventArgs e) =>
			{
				vm = new NestedPlatformViewModel();
				vm.FormsLabel = "Forms Label Binding Changed";
				vm.PlatformLabel = "Native Label Binding Changed";
				vm.PlatformLabelColor = Colors.Pink;
				vm.Age = 10;

				BindingContext = ViewModel = vm;
				;
			};

			var boxView = new BoxView { HeightRequest = 50 };
			boxView.SetBinding(BoxView.BackgroundColorProperty, "PlatformLabelColor");

			var label = new Label();
			label.SetBinding(Label.TextProperty, "FormsLabel");
			var labelAge = new Label();
			labelAge.SetBinding(Label.TextProperty, nameof(vm.Age));

			Layout.Children.Add(buttonNav);
			Layout.Children.Add(label);
			Layout.Children.Add(boxView);
			Layout.Children.Add(button);
			Layout.Children.Add(labelAge);

			BindingContext = ViewModel = vm;
			;

			Content = new ScrollView { Content = Layout };
		}
	}


	[Preserve(AllMembers = true)]
	public class NestedPlatformViewModel : ViewModelBase
	{
		string _formsLabel;
		public string FormsLabel
		{
			get { return _formsLabel; }
			set { if (_formsLabel == value) return; _formsLabel = value; OnPropertyChanged(); }
		}

		string _platformLabel;
		public string PlatformLabel
		{
			get { return _platformLabel; }
			set { if (_platformLabel == value) return; _platformLabel = value; OnPropertyChanged(); }
		}

		Color _platformLabelColor;
		public Color PlatformLabelColor
		{
			get { return _platformLabelColor; }
			set { if (_platformLabelColor == value) return; _platformLabelColor = value; OnPropertyChanged(); }
		}

		int _age;
		public int Age
		{
			get { return _age; }
			set { if (_age == value) return; _age = value; OnPropertyChanged(); }
		}

		bool _selected;
		public bool Selected
		{
			get { return _selected; }
			set { if (_selected == value) return; _selected = value; OnPropertyChanged(); }
		}
	}

}
