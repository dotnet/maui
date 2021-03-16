using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Activity Indicator Does Not Show when set to default color")]
	public class ModalActivityIndicatorTest : TestContentPage
	{
		protected override void Init()
		{
			var vm = new ModalActivityIndicatorModel() { IsBusy = false, BusyText = "Not busy" };

			var button = new Button() { Text = "Make Busy" };
			var colorToggle = new Button() { Text = "Toggle Activity Indicator Color" };

			button.Clicked += async (sender, args) =>
			{
				vm.IsBusy = true;
				vm.BusyText = "Busy";
				await Task.Delay(1500);
				vm.IsBusy = false;
				vm.BusyText = "Not Busy";
			};

			var activityIndicator = new ModalActivityIndicator();
			activityIndicator.BindingContext = vm;

			colorToggle.Clicked += (sender, args) =>
			{
				vm.Color = vm.Color.IsDefault ? Color.Green : Color.Default;
			};

			Content = new StackLayout()
			{
				Children = { button, colorToggle, activityIndicator }
			};
		}

		[Preserve(AllMembers = true)]
		public class ModalActivityIndicatorModel : INotifyPropertyChanged
		{
			bool _isBusy;
			string _busyText;
			Color _color;

			public ModalActivityIndicatorModel()
			{
				_color = Color.Default;
			}

			public bool IsBusy
			{
				get { return _isBusy; }
				set
				{
					_isBusy = value;
					OnPropertyChanged();
				}
			}

			public string BusyText
			{
				get { return _busyText; }
				set
				{
					_busyText = value;
					OnPropertyChanged();
				}
			}

			public Color Color
			{
				get { return _color; }
				set
				{
					_color = value;
					OnPropertyChanged();
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;

			protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		[Preserve(AllMembers = true)]
		public class ModalActivityIndicator : RelativeLayout
		{
			public ModalActivityIndicator()
			{
				this.SetBinding(IsVisibleProperty, "IsBusy");
				this.SetBinding(IsEnabledProperty, "IsBusy");

				Children.Add(
					view: new BoxView
					{
						Opacity = .4,
						BackgroundColor = Color.FromHex("#ccc")
					},
					widthConstraint: Microsoft.Maui.Controls.Constraint.RelativeToParent((parent) =>
					{
						return parent.Width;
					}),
					heightConstraint: Microsoft.Maui.Controls.Constraint.RelativeToParent((parent) =>
					{
						return parent.Height;
					})
					);

				var content = new StackLayout
				{
					BackgroundColor = Color.White,
					Spacing = 10,
					Padding = new Thickness(
						horizontalSize: 10,
						verticalSize: 20
						)
				};

				var activityIndicator = new ActivityIndicator { IsRunning = true };
				activityIndicator.SetBinding(ActivityIndicator.ColorProperty, "Color");

				content.Children.Add(activityIndicator);
				var label = new Label { HorizontalOptions = LayoutOptions.CenterAndExpand };

				label.SetBinding(Label.TextProperty, "BusyText");
				label.SetBinding(Label.TextColorProperty, "Color");

				content.Children.Add(label);

				Children.Add(
					view: content,
					widthConstraint: Microsoft.Maui.Controls.Constraint.RelativeToParent((parent) =>
					{
						return parent.Width / 2;
					}),
					heightConstraint: Microsoft.Maui.Controls.Constraint.RelativeToParent((parent) =>
					{
						return parent.Width / 3;
					}),
					xConstraint: Microsoft.Maui.Controls.Constraint.RelativeToParent((parent) =>
					{
						return parent.Width / 4;
					}),
					yConstraint: Microsoft.Maui.Controls.Constraint.RelativeToParent((parent) =>
					{
						return (parent.Height / 2) - (parent.Width / 6);
					})
					);
			}
		}
	}
}
