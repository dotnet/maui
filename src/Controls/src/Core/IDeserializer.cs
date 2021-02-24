using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IDeserializer
	{
		Task<IDictionary<string, object>> DeserializePropertiesAsync();
		Task SerializePropertiesAsync(IDictionary<string, object> properties);
	}
}