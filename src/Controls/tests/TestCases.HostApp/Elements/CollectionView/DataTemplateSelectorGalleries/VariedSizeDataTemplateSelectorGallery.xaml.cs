using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.CollectionViewGalleries.DataTemplateSelectorGalleries
{
	public partial class VariedSizeDataTemplateSelectorGallery : ContentPage
	{
		string _index = "1";
		string _itemsCount = "1";
		int _counter = 6;
		string _selectedTemplate = nameof(Latte);
		bool _shouldTriggerReset;

		public VariedSizeDataTemplateSelectorGallery()
		{
			InitializeComponent();
			BindingContext = this;

			foreach (var vehicle in CreateDefaultDrinks())
			{
				Items.Add(vehicle);
			}

			IEnumerable<DrinkBase> CreateDefaultDrinks()
			{
				yield return new Coffee("0");
				yield return new Milk("1");
				yield return new Coffee("2");
				yield return new Coffee("3");
				yield return new Milk("4");
				yield return new Coffee("5");
			}
		}

		public ObservableCollection<DrinkBase> Items { get; set; } =
			new ObservableCollection<DrinkBase>();

		public string Index
		{
			get => _index;
			set => SetValue(ref _index, value);
		}

		public string ItemsCount
		{
			get => _itemsCount;
			set => SetValue(ref _itemsCount, value);
		}

		public string SelectedTemplate
		{
			get => _selectedTemplate;
			set => SetValue(ref _selectedTemplate, value);
		}

		public bool ShouldTriggerReset
		{
			get => _shouldTriggerReset;
			set => SetValue(ref _shouldTriggerReset, value);
		}

		void Insert_OnClicked(object sender, EventArgs e)
		{
			if (!IsValid(out var index))
				return;

			Items.Insert(index, CreateDrink());
		}

		void Add_OnClicked(object sender, EventArgs e)
		{
			if (!IsValid(out var _))
				return;

			Items.Add(CreateDrink());
		}

		void SetValue<T>(ref T backingField, in T value, [CallerMemberName] string callerName = "")
		{
			if (Equals(backingField, value))
				return;
			OnPropertyChanging(callerName);
			backingField = value;
			OnPropertyChanged(callerName);
		}

		void Remove_OnClicked(object sender, EventArgs e)
		{
			if (!IsValid(out var index))
				return;

			Items.RemoveAt(index);
		}

		DrinkBase CreateDrink()
		{
			switch (SelectedTemplate)
			{
				case nameof(Milk):
					return new Milk(_counter++.ToString());
				case nameof(Coffee):
					return new Coffee(_counter++.ToString());
				case nameof(Latte):
					{
						var latte = new Latte(_counter++.ToString())
						{
							Ingredients = new ObservableCollection<DrinkBase>() { new Milk(_counter++.ToString()), new Coffee(_counter++.ToString()) }
						};
						return latte;
					}
				default:
					throw new ArgumentException();
			}
		}

		bool IsValid(out int index)
		{
			index = -1;
			if (string.IsNullOrWhiteSpace(Index))
				return false;
			if (!int.TryParse(Index, out index))
				return false;
			if (index > Items.Count || index < 0)
				return false;

			return true;
		}
	}

	class DrinkTemplateSelector : DataTemplateSelector
	{
		public DataTemplate CoffeeTemplate { get; set; } = default!;
		public DataTemplate MilkTemplate { get; set; } = default!;
		public DataTemplate LatteTemplate { get; set; } = default!;

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			if (item is Coffee)
				return CoffeeTemplate;

			if (item is Milk)
				return MilkTemplate;

			if (item is Latte)
				return LatteTemplate;

			throw new ArgumentOutOfRangeException();
		}
	}

	public abstract class DrinkBase
	{
		protected DrinkBase(string name) => Name = name;

		public string Name { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}

	class Coffee : DrinkBase
	{
		public Coffee(string name) : base(nameof(Coffee) + name) { }
	}

	class Milk : DrinkBase
	{
		public Milk(string name) : base(nameof(Milk) + name) { }
	}

	class Latte : DrinkBase
	{
		public Latte(string name) : base(nameof(Latte) + name) { }

		public ObservableCollection<DrinkBase> Ingredients { get; set; } = new ObservableCollection<DrinkBase>();
	}
}
