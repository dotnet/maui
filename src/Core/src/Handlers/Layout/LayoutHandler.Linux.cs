namespace Microsoft.Maui.Handlers
{
	public partial class LayoutHandler : ViewHandler<ILayout, LayoutView>
	{
		protected override LayoutView CreateNativeView()
		{
			return new LayoutView();
		}

		public void Add(IView child)
		{

		}

		public void Remove(IView child)
		{

		}
	}
}