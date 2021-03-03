
namespace Microsoft.Maui.Controls
{
	public interface IConfigElement<out T> where T : Element
	{
		T Element { get; }
	}
}
