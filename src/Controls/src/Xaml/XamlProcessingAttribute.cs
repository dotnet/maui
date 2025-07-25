using System;
using System.Runtime.Versioning;

namespace Microsoft.Maui.Controls.Xaml;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class, Inherited = false)]
//when we remove this attribute, we'll have to obsolete XamlCompilationAttribute
#if !NETSTANDARD
[System.Runtime.Versioning.RequiresPreviewFeatures]
#endif
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