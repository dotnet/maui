using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IDeserializer
	{
		[RequiresUnreferencedCode(TrimmerConstants.SerializerTrimmerWarning)]
		Task<IDictionary<string, object>> DeserializePropertiesAsync();
		[RequiresUnreferencedCode(TrimmerConstants.SerializerTrimmerWarning)]
		Task SerializePropertiesAsync(IDictionary<string, object> properties);
	}
}