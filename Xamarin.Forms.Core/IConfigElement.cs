
namespace Xamarin.Forms
{
	public interface IConfigElement<out T> where T : Element
	{
		T Element { get; }
	}
}