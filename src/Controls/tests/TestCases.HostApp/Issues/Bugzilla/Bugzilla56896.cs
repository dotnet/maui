using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 56896, "ListViews for lists with many elements regressed in performance on iOS", PlatformAffected.iOS)]
	public class Bugzilla56896 : TestContentPage
	{
		const string Instructions = "The number in blue is the number of constructor calls. The number in purple is the initial load time in milliseconds.";
		const string InstructionsId = "InstructionsId";
		const string ConstructorCountId = "constructorCount";
		const string TimeId = "time";


		class MyViewModel : INotifyPropertyChanged
		{
			int _constructorCallCount;

			public int ConstructorCallCount
			{
				get { return _constructorCallCount; }
				set
				{
					_constructorCallCount = value;
					OnPropertyChanged();
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;

			protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}


		class Fizz : ViewCell
		{
			readonly MyViewModel _vm;

			Label myLabel;
			public Fizz(MyViewModel vm)
			{
				_vm = vm;

				vm.ConstructorCallCount++;

				Height = 30;

				myLabel = new Label { Text = "fizz" };
				View = myLabel;
			}
			~Fizz()
			{
				_vm.ConstructorCallCount--;
			}
		}


		class Buzz : ViewCell
		{
			readonly MyViewModel _vm;

			Label myLabel;
			public Buzz(MyViewModel vm)
			{
				_vm = vm;

				vm.ConstructorCallCount++;

				Height = 50;

				myLabel = new Label { Text = "buzz" };
				View = myLabel;
			}
			~Buzz()
			{
				_vm.ConstructorCallCount--;
			}
		}


		class Fizzbuzz : ViewCell
		{
			readonly MyViewModel _vm;

			Label myLabel;
			public Fizzbuzz(MyViewModel vm)
			{
				_vm = vm;

				vm.ConstructorCallCount++;

				Height = 150;

				myLabel = new Label { Text = "fizzbuzz" };
				View = myLabel;
			}
			~Fizzbuzz()
			{
				_vm.ConstructorCallCount--;
			}
		}


		class Number : ViewCell
		{
			readonly MyViewModel _vm;

			Label myLabel;
			public Number(MyViewModel vm)
			{
				_vm = vm;

				vm.ConstructorCallCount++;

				Height = 44;

				myLabel = new Label();
				myLabel.SetBinding(Label.TextProperty, ".");
				View = myLabel;
			}
			~Number()
			{
				_vm.ConstructorCallCount--;
			}
		}

		class MyDataTemplateSelector : DataTemplateSelector
		{
			DataTemplate _fizzbuzz;
			DataTemplate _fizz;
			DataTemplate _buzz;
			DataTemplate _number;

			public MyDataTemplateSelector(MyViewModel vm)
			{
				_fizzbuzz = new DataTemplate(() => new Fizzbuzz(vm));
				_fizz = new DataTemplate(() => new Fizz(vm));
				_buzz = new DataTemplate(() => new Buzz(vm));
				_number = new DataTemplate(() => new Number(vm));
			}

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
			{
				int number = (int)item;

				if (number % 15 == 0)
					return _fizzbuzz;
				else if (number % 5 == 0)
					return _buzz;
				else if (number % 3 == 0)
					return _fizz;
				else
					return _number;
			}
		}


		Label _timeLabel = new Label { TextColor = Colors.Purple, AutomationId = TimeId };
		Stopwatch _timer = new Stopwatch();
		ListView _listView;
		protected override void Init()
		{
			_timer.Start();
			var vm = new MyViewModel();

			BindingContext = vm;

			var label = new Label { TextColor = Colors.Blue, AutomationId = ConstructorCountId };
			label.SetBinding(Label.TextProperty, nameof(vm.ConstructorCallCount));

			_listView = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				HasUnevenRows = true,
				// Set the RowHeight to enable optimal performance and minimal constructor calls.
				// It will still use the specified Cell heights on final measure.
				// Note, however, that doing this negates the fix for Bugzilla 43313, so if user expects
				// to add items to the bottom of this list and scroll smoothly, user should omit the RowHeight
				// and rely solely on the Cell heights. This will cause each row to be constructed at least once,
				// but it will allow the ListView to estimate the height properly for smooth scrolling.
				// Also note that performance will degrade if the first cell does not have a specified height or
				// if most of the cells do not have a specified height. It is recommended to specify a height on all
				// or none of the cells when possible.
				RowHeight = 50,
				ItemsSource = Enumerable.Range(1, 5001),
				ItemTemplate = new MyDataTemplateSelector(vm)
			};
			Content = new StackLayout { Children = { new Label { Text = Instructions, AutomationId = InstructionsId }, label, _timeLabel, _listView } };
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			_timer.Stop();
			_timeLabel.Text = _timer.ElapsedMilliseconds.ToString();
			_timer.Reset();
		}
	}
}