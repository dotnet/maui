using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Controls.Sample.Behaviors
{
	public class NumericValidationBehavior : Behavior<Entry>
	{
		protected override void OnAttachedTo(Entry entry)
		{
			entry.TextChanged += OnEntryTextChanged;
			base.OnAttachedTo(entry);
		}

		protected override void OnDetachingFrom(Entry entry)
		{
			entry.TextChanged -= OnEntryTextChanged;
			base.OnDetachingFrom(entry);
		}

		void OnEntryTextChanged(object? sender, TextChangedEventArgs args)
		{
			bool isValid = double.TryParse(args.NewTextValue, out var result);
			((Entry)sender!).TextColor = isValid ? Colors.Transparent : Colors.Red;
		}
	}
}
