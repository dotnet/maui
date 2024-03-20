namespace Microsoft.Maui.Controls
{
	public partial class BoxView
	{
		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();

			if (Handler != null)
			{
				foreach (var prop in AndroidBatchPropertyMapper<BoxView, IViewHandler>.SkipList)
				{
					Handler.UpdateValue(prop);
				}
			}
		}
	}
}

