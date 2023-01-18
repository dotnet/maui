#nullable disable
using Android.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal class TextViewHolder : SelectableViewHolder
	{
		public TextView TextView { get; }

		public TextViewHolder(TextView itemView, bool isSelectionEnabled = true) : base(itemView, isSelectionEnabled)
		{
			TextView = itemView;
			TextView.Clickable = isSelectionEnabled;
		}
	}
}