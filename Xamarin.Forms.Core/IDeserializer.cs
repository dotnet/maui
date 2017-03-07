using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Forms.Internals
{
	public interface IDeserializer
	{
		Task<IDictionary<string, object>> DeserializePropertiesAsync();
		Task SerializePropertiesAsync(IDictionary<string, object> properties);
	}
}