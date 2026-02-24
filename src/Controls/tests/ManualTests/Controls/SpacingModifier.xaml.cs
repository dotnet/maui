using Microsoft.Maui.ManualTests.Helpers;

namespace Microsoft.Maui.ManualTests.Controls
{
	public partial class SpacingModifier : ContentView
	{
		public static readonly BindableProperty SpacingTextProperty = BindableProperty.Create(nameof(SpacingText), typeof(string), typeof(SpacingModifier), "0", propertyChanged: OnSpacingTextPropertyChanged);

		public string SpacingText
		{
			get => (string)GetValue(SpacingTextProperty);
			set => SetValue(SpacingTextProperty, value);
		}

		public CollectionView CV { get; set; }

		public SpacingModifier()
		{
			InitializeComponent();
		}

		void OnUpdateButtonClicked(object sender, EventArgs e)
		{
			if (!ParseIndexes(out int[] indexes))
			{
				return;
			}
			if (CV.ItemsLayout is LinearItemsLayout linearItemsLayout)
			{
				linearItemsLayout.ItemSpacing = indexes[0];
			}
			else if (CV.ItemsLayout is GridItemsLayout gridItemsLayout)
			{
				gridItemsLayout.VerticalItemSpacing = indexes[0];
				gridItemsLayout.HorizontalItemSpacing = indexes[1];
			}
		}

		bool ParseIndexes(out int[] indexes)
		{
			if (CV.ItemsLayout is LinearItemsLayout)
			{
				return IndexParser.ParseIndexes(entry.Text, 1, out indexes);
			}
			return IndexParser.ParseIndexes(entry.Text, 2, out indexes);
		}

		static void OnSpacingTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var element = (SpacingModifier)bindable;
			element.entry.Text = (string)newValue;
		}
	}
}
