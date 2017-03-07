namespace Xamarin.Forms
{
	public interface IMenuItemController
	{
		bool IsEnabled { get; set; }
		string IsEnabledPropertyName { get; }

		void Activate();
	}
}
