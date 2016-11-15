using Xamarin.Forms.CustomAttributes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 34720, "Incorrect iOS button IsEnabled when scrolling ListView with command binding ")]
	public class Bugzilla34720 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init ()
		{
			Title = "Test Command Binding";
			_list = new ListView () { 
				ClassId = "SampleList",
				// Note: Turning on and off row height does not effect the issue, 
				// but with row heights on there is a visual glitch with the recyclyed row spacing
				//RowHeight = SampleViewCell.RowHeight,
				HasUnevenRows = true,
				ItemTemplate = new DataTemplate (typeof(SampleViewCell)),
				BackgroundColor = Color.FromHex ("E0E0E0"),
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
			};
			_list.SetBinding (ListView.ItemsSourceProperty, "Items");
			_list.SetBinding (ListView.RefreshCommandProperty, "RefreshCommand");
			_list.SetBinding (ListView.IsRefreshingProperty, "IsRefreshing");

			var listViewModel = new TestListViewModel ();
			listViewModel.AddTestData ();
			BindingContext = listViewModel;


			_list.ItemTapped += (sender, e) =>
			{
				DisplayAlert("hello", "You tapped " + e.Item.ToString(), "OK", "Cancel");
			};

			var btnDisable = new Button () {
				Text = "Disable ListView",
			};

			btnDisable.Clicked += (object sender, EventArgs e) => {
				if (_list.IsEnabled == true){
					_list.IsEnabled = false;
					btnDisable.Text = "Enable ListView";
				}
				else {
					_list.IsEnabled = true;
					btnDisable.Text = "Disable ListView";
				}
			};

			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = { btnDisable, _list }
			};
		}

		ListView _list;

		[Preserve (AllMembers = true)]
		public class SampleViewCell : ViewCell
		{
			public static int RowHeight = 120;
			static int s_idSeed;
			int id;

			public SampleViewCell ()
			{
				id = s_idSeed++;
				var grid = new Grid {
					ClassId = "SampleCard",
					Padding = new Thickness (7, 10),
					RowSpacing = 0,
					ColumnSpacing = 0,
					RowDefinitions = {
						new RowDefinition{ Height = new GridLength (80, GridUnitType.Absolute) },	
						new RowDefinition{ Height = new GridLength (40, GridUnitType.Absolute) },	
					},

				};

				var head = new SampleHeaderView ();
				grid.Children.Add (head);

				var foot = new SampleListActionView ();
				Grid.SetRow (foot, 1);
				grid.Children.Add (foot);

				View = grid;
			}

			#region Testing Code

			// Note this block can be removed it is just used to observing the ViewCell creation.
			int _counter;


			protected override void OnAppearing ()
			{

				base.OnAppearing ();
				_counter++;
				System.Diagnostics.Debug.WriteLine ("OnAppearing {0}, {1}", id, _counter);
			}

			protected override void OnDisappearing ()
			{
				base.OnDisappearing ();
				_counter--;
				System.Diagnostics.Debug.WriteLine ("OnDisappearing {0}, {1}", id, _counter);
			}

			#endregion

			public class SampleHeaderView : ContentView
			{

				public SampleHeaderView ()
				{
					//+-----------+----------------+
					//|       1   |                |
					//+-----------+----------------+
					//|       2   |                |
					//+-----------+----------------+


					var grid = new Grid {
						Padding = new Thickness (5, 5, 5, 1),
						RowSpacing = 1,
						ColumnSpacing = 1,
						BackgroundColor = Color.FromHex ("FAFAFA"),
						RowDefinitions = {
							new RowDefinition{ Height = new GridLength (1.25, GridUnitType.Star) },	
							new RowDefinition{ Height = new GridLength (0.8, GridUnitType.Star) },	
						},
						ColumnDefinitions = {
							new ColumnDefinition{ Width = new GridLength (1, GridUnitType.Star) },
							new ColumnDefinition{ Width = new GridLength (1, GridUnitType.Star) },
						} 
					};
					//1 number
					var materialNumber = new Label () {
						VerticalOptions = LayoutOptions.StartAndExpand,
						HorizontalOptions = LayoutOptions.StartAndExpand
					};
					Grid.SetColumnSpan (materialNumber, 2);
					materialNumber.SetBinding (Label.TextProperty, "Number");
					grid.Children.Add (materialNumber);

					//2 Description
					var materialDescription = new Label () {
						VerticalOptions = LayoutOptions.StartAndExpand,
						HorizontalOptions = LayoutOptions.StartAndExpand
					};
					Grid.SetColumnSpan (materialDescription, 2);
					Grid.SetRow (materialDescription, 1);
					materialDescription.SetBinding (Label.TextProperty, "Description");
					//grid.Children.Add (materialDescription);

					//3 Approve Label
					var canApprove = new Label () {
						VerticalOptions = LayoutOptions.StartAndExpand,
						HorizontalOptions = LayoutOptions.StartAndExpand
					};
					Grid.SetColumn (canApprove, 1);
					Grid.SetRow (canApprove, 1);
					canApprove.SetBinding (Label.TextProperty, new Binding ("CanApprove", stringFormat: "Can Approve: {0}"));
					grid.Children.Add (canApprove);

					//3 Approve Label
					var canDeny = new Label () {
						VerticalOptions = LayoutOptions.StartAndExpand,
						HorizontalOptions = LayoutOptions.StartAndExpand
					};
					Grid.SetColumn (canDeny, 0);
					Grid.SetRow (canDeny, 1);
					canDeny.SetBinding (Label.TextProperty, new Binding ("CanDeny", stringFormat: "Can Deny: {0}"));
					grid.Children.Add (canDeny);

					Content = grid;

				}
			}

			public class SampleListActionView : ContentView
			{
				public SampleListActionView ()
				{
					var overallGrid = new Grid {
						BackgroundColor = Color.FromHex ("FAFAFA"),
						HorizontalOptions = LayoutOptions.FillAndExpand,
						VerticalOptions = LayoutOptions.CenterAndExpand,
						ColumnDefinitions = {
							new ColumnDefinition{ Width = new GridLength (1, GridUnitType.Star) },
							new ColumnDefinition{ Width = new GridLength (1, GridUnitType.Star) },
						}
					};

					var grid = new Grid {
						VerticalOptions = LayoutOptions.FillAndExpand,
						HorizontalOptions = LayoutOptions.FillAndExpand,
						ColumnDefinitions = {
							new ColumnDefinition{ Width = new GridLength (1, GridUnitType.Star) },
							new ColumnDefinition{ Width = new GridLength (1, GridUnitType.Star) },
						}
					};
					// 1 Deny
					var denyBtn = new Button {
						ClassId = "btnReject",
						Text = "DENY",
						HorizontalOptions = LayoutOptions.FillAndExpand,
						VerticalOptions = LayoutOptions.FillAndExpand
					};

					denyBtn.SetBinding(Button.CommandProperty, "DenyCommand");

					grid.Children.Add (denyBtn);

					// 2 Approve
					var approveBtn = new Button {
						ClassId = "btnApprove",
						Text = "Approve",
						HorizontalOptions = LayoutOptions.FillAndExpand,
						VerticalOptions = LayoutOptions.FillAndExpand,

					};
					Grid.SetColumn (approveBtn, 1);
					approveBtn.SetBinding (Button.CommandProperty, "ApproveCommand");
					grid.Children.Add (approveBtn);


					Grid.SetColumn (grid, 1);
					overallGrid.Children.Add (grid);
					Content = overallGrid;
				}
			}
		}

		[Preserve (AllMembers = true)]
		public class TestListViewModel : INotifyPropertyChanged
		{
			Collection<TestViewModel> _items = new ObservableCollection<TestViewModel> ();

			public void AddTestData ()
			{
				for (int i = 0; i < 20; i++) {
					_items.Add (new TestViewModel () {
						Description = string.Format ("Sample Description {0}", i),
						Number = (i + 1).ToString (),
						CanApprove = i % 2 == 0,
						CanDeny = i % 2 == 0
					});
				}
				RaisePropertyChanged ("Items");			
			}

			public Collection<TestViewModel> Items { get { return _items; } set { _items = value; } }

			public Command RefreshCommand {
				get {
					return new Command (OnRefresh);
				}
			}

			public bool IsRefreshing { get; set; }

			async void OnRefresh ()
			{
				IsRefreshing = true;
				RaisePropertyChanged ("IsRefreshing");	
				_items.Clear ();
				await Task.Delay (1000);
				AddTestData ();
				IsRefreshing = false;
				RaisePropertyChanged ("IsRefreshing");
			}

			#region INotifyPropertyChanged implementation

			public event PropertyChangedEventHandler PropertyChanged;

			#endregion

			protected virtual void RaisePropertyChanged (string propertyName)
			{
				PropertyChangedEventHandler propertyChanged = PropertyChanged;
				if (propertyChanged != null) {
					propertyChanged (this, new PropertyChangedEventArgs (propertyName));
				}
			}
		}

		[Preserve (AllMembers = true)]
		public class TestViewModel
		{

			public string Number {	get; set; }

			public string Description {	get; set; }

			bool _canApprove;

			public bool CanApprove {
				get{ return _canApprove; }
				set {
					_canApprove = value;
					ApproveCommand.ChangeCanExecute ();
				}
			}

			bool _canDeny;

			public bool CanDeny {
				get { return _canDeny; }
				set {
					_canDeny = value;
					DenyCommand.ChangeCanExecute ();
				}
			}

			public Command ApproveCommand {
				get {
					return new Command (OnApprove, () => CanApprove);
				}
			}

			public Command DenyCommand {
				get {
					return new Command (OnDeny, () => CanDeny);
				}
			}

			async void OnApprove ()
			{
				await Application.Current.MainPage.DisplayAlert ("Approve", string.Format ("Can Approve {0} {1}", Number, CanApprove), "Ok");
			}

			async void OnDeny ()
			{
				await Application.Current.MainPage.DisplayAlert ("Deny", string.Format ("Can Deny {0} {1}", Number, CanDeny), "Ok");
			}
		}
	}
}
