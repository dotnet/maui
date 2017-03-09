using System;
using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class EvalRequested : EventArgs
	{
		public string Script { get; }

		public EvalRequested(string script)
		{
			Script = script;
		}
	}
}