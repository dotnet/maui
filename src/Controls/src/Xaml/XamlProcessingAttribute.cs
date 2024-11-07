using System;

namespace Microsoft.Maui.Controls.Xaml
{
	public enum XamlInflator
	{
		/// <summary>
		/// Picks the best Inflator available, don't change it unless you know what you're doing.
		/// </summary>
		Default = 0,
		Runtime = 1 << 0,
		XamlC = 1 << 1,
		SourceGen = 1 << 2,
	}

	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class, Inherited = false)]
	public sealed class XamlProcessingAttribute : Attribute
	{
		public XamlProcessingAttribute(XamlInflator inflator) 
		{ 
			if ((inflator & (inflator - 1)) != 0) //check if only one bit is set (http://graphics.stanford.edu/~seander/bithacks.html#DetermineIfPowerOf2
				throw new ArgumentException("Only one option can be set at a time");
			XamlInflator = inflator;
		}

		internal XamlProcessingAttribute(XamlInflator inflators, bool generateInflatorSwitch) 
		{
			XamlInflator = inflators; //used as a flag to indicate the infators used for unit test purposes
			GenerateInflatorSwitch = generateInflatorSwitch;
		}

		public XamlInflator XamlInflator { get; }
		internal bool GenerateInflatorSwitch { get; }
	}
}