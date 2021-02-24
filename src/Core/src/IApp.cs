using System;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	public interface IApp
	{
		IServiceProvider? Services { get; }
	}
}