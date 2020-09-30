using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Xaml;


#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 9771, "Changing CarouselView Position does not change view ", PlatformAffected.UWP)]
	public partial class Issue9771 : TestContentPage
	{
		public Issue9771()
		{
#if APP
			InitializeComponent();
			carousel.Scrolled += (s, e) =>
			{
				System.Diagnostics.Debug.WriteLine($" Center Item:{e.CenterItemIndex} Scroll:{e.HorizontalOffset} Delta:{e.HorizontalDelta}");
			};
#endif
		}

		protected override void Init()
		{
			BindingContext = new MainViewModel();
		}
	}


	public class MainViewModel : ViewModelBase
	{
		bool _isLoading;
		public bool IsLoading
		{
			get { return _isLoading; }
			set
			{
				_isLoading = value;
				OnPropertyChanged();
			}
		}


		public MainViewModel()
		{
			Task.Run(async () =>
			{
				IsLoading = true;

				await Task.Delay(1000);

				Items = new List<string>();
				for (int i = 0; i < 100; i++)
				{
					Items.Add(i.ToString());
				}

				Position = Items.Count / 2;

				IsLoading = false;
			});


		}

		List<string> _items;
		public List<string> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		int _position;
		public int Position
		{
			get { return _position; }
			set
			{
				_position = value;
				OnPropertyChanged();
			}
		}

		ICommand _LeftButtonCommand;
		public ICommand LeftButtonCommand
		{
			get
			{
				return _LeftButtonCommand
					?? (_LeftButtonCommand = new Command(
					() =>
					{
						Position--;
					}));
			}
		}

		private ICommand _rightButtonCommand;
		public ICommand RightButtonCommand
		{
			get
			{
				return _rightButtonCommand
					?? (_rightButtonCommand = new Command(
					() =>
					{
						Position++;
					}));
			}
		}
	}

	public class ExtendedCarousel : CarouselView, INotifyPropertyChanged, IDisposable
	{
		public ExtendedCarousel()
		{
			HorizontalScrollBarVisibility = ScrollBarVisibility.Never;
			VerticalScrollBarVisibility = ScrollBarVisibility.Never;

			IsScrollAnimated = true;

			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				SnapPointsAlignment = SnapPointsAlignment.Center,
				SnapPointsType = SnapPointsType.MandatorySingle
			};
		}

		public override bool AnimatePositionChanges => false;

		public void Dispose()
		{
		}

		protected override void OnPositionChanged(PositionChangedEventArgs args)
		{
			var d = this.CurrentItem;

			IsLoading = false;
			base.OnPositionChanged(args);
		}

		protected override void OnPropertyChanging([CallerMemberName] string propertyName = null)
		{
			if (propertyName == nameof(Position))
			{
				IsLoading = true;
			}
			base.OnPropertyChanging(propertyName);
		}

		public static readonly BindableProperty IsLoadingProperty = BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(ExtendedCarousel), false, defaultBindingMode: BindingMode.TwoWay);
		public bool IsLoading
		{
			get { return (bool)this.GetValue(IsLoadingProperty); }
			set
			{
				this.SetValue(IsLoadingProperty, value);
				RaisePropertyChanged();
			}
		}

		public new event PropertyChangedEventHandler PropertyChanged;

		void RaisePropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

}