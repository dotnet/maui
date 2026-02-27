using System;
using System.Collections.Generic;
using Microsoft.Build.Utilities;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	class ILContext
	{
		public ILContext(ILProcessor il, MethodBody body, ModuleDefinition module, XamlCache cache, FieldDefinition parentContextValues = null)
		{
			IL = il;
			Body = body;
			Values = [];
			Variables = [];
			Scopes = [];
			TypeExtensions = [];
			ParentContextValues = parentContextValues;
			Module = module;
			Cache = cache;
			CompileBindingsWithSource = false;
		}

		public XamlCache Cache { get; private set; }

		public Dictionary<IValueNode, object> Values { get; private set; }

		public Dictionary<ElementNode, VariableDefinition> Variables { get; private set; }

		public Dictionary<INode, Tuple<VariableDefinition, IList<string>>> Scopes { get; private set; }

		public Dictionary<INode, TypeReference> TypeExtensions { get; }

		public FieldDefinition ParentContextValues { get; private set; }

		public object Root { get; set; } //FieldDefinition or VariableDefinition

		public ILProcessor IL { get; private set; }

		public MethodBody Body { get; private set; }

		public ModuleDefinition Module { get; private set; }
		public string XamlFilePath { get; internal set; }

		public TaskLoggingHelper LoggingHelper { get; internal set; }

		public bool ValidateOnly { get; set; }

		public bool CompileBindingsWithSource { get; set; }
	}
}
