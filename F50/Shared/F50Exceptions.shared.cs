using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.F50
{
	public class NotImplentedInReferenceAssembly : NotImplementedException
	{
		public NotImplentedInReferenceAssembly()
			: base("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.")
		{

		}
	}
}
