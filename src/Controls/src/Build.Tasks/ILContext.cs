using System;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	class ILContext
	{
		public ILContext(ILProcessor il, MethodBody body, ModuleDefinition module, FieldDefinition parentContextValues = null)
		{
			IL = il;
			Body = body;
			Values = new Dictionary<IValueNode, object>();
			Variables = new Dictionary<IElementNode, VariableDefinition>();
			Scopes = new Dictionary<INode, Tuple<VariableDefinition, IList<string>>>();
			TypeExtensions = new Dictionary<INode, TypeReference>();
			ParentContextValues = parentContextValues;
			Module = module;
		}

		public Dictionary<IValueNode, object> Values { get; private set; }

		public Dictionary<IElementNode, VariableDefinition> Variables { get; private set; }

		public Dictionary<INode, Tuple<VariableDefinition, IList<string>>> Scopes { get; private set; }

		public Dictionary<INode, TypeReference> TypeExtensions { get; }

		public FieldDefinition ParentContextValues { get; private set; }

		public object Root { get; set; } //FieldDefinition or VariableDefinition

		public ILProcessor IL { get; private set; }

		public MethodBody Body { get; private set; }

		public ModuleDefinition Module { get; private set; }
		public bool DefineDebug { get; internal set; }
		public string XamlFilePath { get; internal set; }
	}
}