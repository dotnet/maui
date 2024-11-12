using System;

namespace Microsoft.Maui.Controls.Xaml;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class, Inherited = false)]
public sealed class XamlProcessingAttribute : Attribute
{
#pragma warning disable RS0016 // Add public types and members to the declared API
	public XamlProcessingAttribute(string foo)
#pragma warning restore RS0016 // Add public types and members to the declared API
	{

	}
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