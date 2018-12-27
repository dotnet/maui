using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	internal class TextViewHolder : SelectableViewHolder
	{
		public TextView TextView { get; }

		public TextViewHolder(TextView itemView) : base(itemView)
		{
			TextView = itemView;
			TextView.Clickable = true;
		}
	}
}