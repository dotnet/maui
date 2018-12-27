namespace Xamarin.Forms.Platform.Android
{
	internal class TemplatedItemViewHolder : SelectableViewHolder
	{
		public View View { get; }

		public TemplatedItemViewHolder(global::Android.Views.View itemView, View rootElement) : base(itemView)
		{
			View = rootElement;
		}

		protected override void OnSelectedChanged()
		{
			base.OnSelectedChanged();

			VisualStateManager.GoToState(View, IsSelected ? "Selected" : "Normal");
		}
	}
}