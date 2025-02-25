using System.Collections.ObjectModel;
using System.Globalization;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26328, "SwipeView causes Java.Lang.IllegalArgumentException: Cannot add a null child view to a ViewGroup", PlatformAffected.Android)]
	public partial class Issue26328 : TestContentPage
	{
		public Issue26328()
		{
			InitializeComponent();
		}

		protected override void Init()
		{
			BindingContext = new Issue26328ViewModel();
		}
	}

	public class Issue26328ItemModel
	{
		public int Id { get; set; }
		public string Title { get; set; }

		public Issue26328ItemModel(int i)
		{
			Id = i;
			Title = "Hello";
		}

		public Command SwipeCommand =>
			new Command(DeleteMessage);

		void DeleteMessage()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Send<Issue26328ItemModel, Issue26328RemoveMessage>(this, "Swipe", new Issue26328RemoveMessage(this));
#pragma warning disable CS0618 // Type or member is obsolete
		}
	}

	public class Issue26328RemoveMessage
	{
		public Issue26328ItemModel Item { get; }

		public Issue26328RemoveMessage(Issue26328ItemModel item)
		{
			Item = item;
		}
	}

	public class Issue26328ViewModel
	{
		public ObservableCollection<Issue26328ItemModel> ItemList { get; set; }

		public Issue26328ViewModel()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Subscribe<Issue26328ItemModel, Issue26328RemoveMessage>(this, "Swipe", (sender, arg) => RemoveItem(arg.Item));
#pragma warning disable CS0618 // Type or member is obsolete
			ItemList = [];
			for (var i = 0; i < 200; ++i)
			{
				ItemList.Add(new Issue26328ItemModel(i));
			}
		}

		void RemoveItem(Issue26328ItemModel item)
		{
			ItemList.Remove(item);
			GC.Collect();
		}
	}

	[AcceptEmptyServiceProvider]
	public class Issue26328TestConverter : IMultiValueConverter, IMarkupExtension
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values[0] == null || values[1] == null)
			{
				return null;
			}
			var id = (int)values[0];
			var title = (string)values[1];
			return $"{title} - {id}";
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
	}
}