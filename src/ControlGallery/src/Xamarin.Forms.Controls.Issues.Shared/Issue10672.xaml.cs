using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls
{
#if UITEST
	[Category(UITestCategories.CarouselView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10672, "[Bug] Setting CarouselView.IsScrollAnimated To False Throws Exception On UWP When Resizing The Window", PlatformAffected.UWP)]
	public partial class Issue10672 : TestContentPage
	{
		public Issue10672()
		{
#if APP
			InitializeComponent();
#endif
		}

		public ObservableCollection<CarouselItemViewModel> Items { get; } = new ObservableCollection<CarouselItemViewModel>();

		protected override void Init()
		{
			for (int i = 0; i < 5; i++)
				Items.Add(new CarouselItemViewModel());

			BindingContext = this;
		}
	}

	[Preserve(AllMembers = true)]
	public class CarouselItemViewModel : BindableObject
	{
		static string RandomImage(int w = 1000, int h = 1000) => $"https://picsum.photos/{w}/{h}?{Guid.NewGuid()}";

		public ObservableCollection<string> Images { get; } = new ObservableCollection<string>();

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

		public CarouselItemViewModel()
		{
			Images.Add(RandomImage());
			Images.Add(RandomImage());
			Images.Add(RandomImage());
		}

		public ICommand Next => new Command(() =>
		{
			Position = (Position + 1) % Images.Count;
		});

		public ICommand Prev => new Command(() =>
		{
			var count = Images.Count;
			Position = (count + Position - 1) % count;
		});
	}
}