using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 40704, "Strange duplication of listview headers when collapsing/expanding sections")]
public class Bugzilla40704 : TestContentPage
{
	ListView listview;
	int count = 2;

	const string Collapse = "btnCollapse";
	const string List = "lstMain";

	protected override void Init()
	{
		listview = new ListView(ListViewCachingStrategy.RecycleElement)
		{
			AutomationId = List,
			IsGroupingEnabled = true,
			HasUnevenRows = true,
			GroupHeaderTemplate = new DataTemplate(typeof(GroupHeaderViewCell)),
			ItemTemplate = new DataTemplate(typeof(ItemTestViewCell))
		};

		FillPatientsList();

		var button = new Button()
		{
			Text = "Collapse",
			AutomationId = Collapse
		};

		listview.Footer = button;
		button.Clicked += Button_Clicked;
		Content = listview;
	}

	void Button_Clicked(object sender, EventArgs e)
	{
		var source = listview.ItemsSource as List<PatientsGroupViewModel>;
		source[count].Toggle();
		count--;
		if (count < 0)
			count = 2;
	}

	private void FillPatientsList()
	{
		const int groupsNumber = 3;
		const int patientsNumber = 10;

		var patientGroups = new List<PatientsGroupViewModel>();
		var random = new Random();

		for (var i = 0; i < groupsNumber; i++)
		{
			var patients = new List<PatientViewModel>();
			for (var j = 0; j < patientsNumber; j++)
			{
				var code = string.Format("{0}-{1}", i, j);
				var length = random.Next(5, 100);
				var strBuilder = new StringBuilder();
				for (int z = 0; z < length; z++)
				{
					strBuilder.Append(code);
					if (z % 7 == 0)
					{
						strBuilder.Append(' ');
					}
				}

				patients.Add(new PatientViewModel(code) { Description = strBuilder.ToString() });
			}

			patientGroups.Add(new PatientsGroupViewModel(patients)
			{
				Title = "Menu - " + i.ToString(),
			});

		}

		listview.ItemsSource = patientGroups;
	}


	public class GroupHeaderViewCell : ViewCell
	{
		TapGestureRecognizer tapGesture;

		public GroupHeaderViewCell()
		{
			Height = 40;
			var grd = new Grid { BackgroundColor = Colors.Aqua, Padding = new Thickness(5, 10) };
			tapGesture = new TapGestureRecognizer();
			tapGesture.Tapped += HeaderCell_OnTapped;
			grd.GestureRecognizers.Add(tapGesture);
#pragma warning disable CS0618 // Type or member is obsolete
			var lbl = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.FillAndExpand, TextColor = Colors.Black, FontSize = 16 };
#pragma warning restore CS0618 // Type or member is obsolete
			lbl.SetBinding(Label.TextProperty, new Binding("Title"));
			lbl.SetBinding(Label.AutomationIdProperty, new Binding("Title"));

			grd.Children.Add(lbl);
			View = grd;
		}

		void HeaderCell_OnTapped(object sender, EventArgs e)
		{
			var cell = (Layout)sender;
			var vm = cell.BindingContext as PatientsGroupViewModel;
			vm?.Toggle();
		}
	}


	public class ItemTestViewCell : ViewCell
	{
		public ItemTestViewCell()
		{

			var grd = new Grid { BackgroundColor = Colors.Yellow };
#pragma warning disable CS0618 // Type or member is obsolete
			var lbl = new Label { HorizontalOptions = LayoutOptions.FillAndExpand, TextColor = Colors.Black, FontSize = 16, LineBreakMode = LineBreakMode.WordWrap };
#pragma warning restore CS0618 // Type or member is obsolete
			lbl.SetBinding(Label.TextProperty, new Binding("Description"));
			grd.Children.Add(lbl);
			View = grd;
		}
	}


	public class RangeObservableCollection<T> : ObservableCollection<T>
	{
		private bool _suppressNotification = false;

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (!_suppressNotification)
				base.OnCollectionChanged(e);
		}

		public void AddRange(IEnumerable<T> list)
		{
			if (list == null)
				throw new ArgumentNullException("list");

			_suppressNotification = true;

			foreach (var item in list)
			{
				Add(item);
			}
			_suppressNotification = false;
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
	}


	public class PatientsGroupViewModel : RangeObservableCollection<PatientViewModel>
	{
		public bool IsCollapsed { get; private set; }

		public string Title { get; set; }

		private readonly List<PatientViewModel> _patients;

		public PatientsGroupViewModel(List<PatientViewModel> patients)
		{
			_patients = patients;

			UpdateCollection();
		}

		public void Toggle()
		{
			IsCollapsed = !IsCollapsed;

			UpdateCollection();
		}

		private void UpdateCollection()
		{
			if (!IsCollapsed)
			{
				AddRange(_patients);
			}
			else
			{
				Clear();
			}
		}
	}


	public class PatientViewModel
	{
		public PatientViewModel(string code)
		{
			Code = code;
		}

		public string Code { get; set; }

		public string Description { get; set; }
	}
}
