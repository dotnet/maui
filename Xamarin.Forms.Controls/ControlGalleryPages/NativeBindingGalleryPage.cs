using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	public class NativeBindingGalleryPage : ContentPage
	{
		public StackLayout Layout { get; set; }
		public bool NativeControlsAdded { get; set; }

		NestedNativeViewModel ViewModel { get; set; }

		public NativeBindingGalleryPage()
		{

			var vm = new NestedNativeViewModel();
			vm.FormsLabel = "Forms Label Binding";
			vm.NativeLabel = "Native Label Binding";
			vm.NativeLabelColor = Color.Red;
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
				vm = new NestedNativeViewModel();
				vm.FormsLabel = "Forms Label Binding Changed";
				vm.NativeLabel = "Native Label Binding Changed";
				vm.NativeLabelColor = Color.Pink;
				vm.Age = 10;

				BindingContext = ViewModel = vm; ;
			};

			var boxView = new BoxView { HeightRequest = 50 };
			boxView.SetBinding(BoxView.BackgroundColorProperty, "NativeLabelColor");

			var label = new Label();
			label.SetBinding(Label.TextProperty, "FormsLabel");

			Layout.Children.Add(buttonNav);

			Layout.Children.Add(label);

			Layout.Children.Add(boxView);
			Layout.Children.Add(button);

			BindingContext = ViewModel = vm; ;

			Content = new ScrollView { Content = Layout };
		}
	}


	[Preserve(AllMembers = true)]
	public class NestedNativeViewModel : ViewModelBase
	{
		string _formsLabel;
		public string FormsLabel
		{
			get { return _formsLabel; }
			set { _formsLabel = value; OnPropertyChanged(); }
		}

		string _nativeLabel;
		public string NativeLabel
		{
			get { return _nativeLabel; }
			set { _nativeLabel = value; OnPropertyChanged(); }
		}

		Color _nativeLabelColor;
		public Color NativeLabelColor
		{
			get { return _nativeLabelColor; }
			set { _nativeLabelColor = value; OnPropertyChanged(); }
		}

		int _age;
		public int Age
		{
			get { return _age; }
			set { _age = value; OnPropertyChanged(); }
		}

		bool _selected;
		public bool Selected
		{
			get { return _selected; }
			set { _selected = value; OnPropertyChanged(); }
		}
	}

}
