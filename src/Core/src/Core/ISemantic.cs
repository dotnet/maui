using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	public interface ISemantic
	{
		/// <summary>
		/// Adds semantics to every FrameworkElement for accessibility
		/// </summary>
		Semantics Semantics { get; }

		void UpdateSemanticInfo(SemanticInfoRequest request);
	}
}
