using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
{
	class ILContext
	{
		public ILContext(ILProcessor il, MethodBody body, ModuleDefinition module, FieldDefinition parentContextValues = null)
		{
			IL = il;
			Body = body;
			Values = new Dictionary<IValueNode, object>();
			Variables = new Dictionary<IElementNode, VariableDefinition>();
			Scopes = new Dictionary<INode, VariableDefinition>();
			TypeExtensions = new Dictionary<INode, TypeReference>();
			ParentContextValues = parentContextValues;
			Module = module;
		}

		public Dictionary<IValueNode, object> Values { get; private set; }

		public Dictionary<IElementNode, VariableDefinition> Variables { get; private set; }

		public Dictionary<INode, VariableDefinition> Scopes { get; private set; }

		public Dictionary<INode, TypeReference> TypeExtensions { get; }

		public FieldDefinition ParentContextValues { get; private set; }

		public object Root { get; set; } //FieldDefinition or VariableDefinition

		public ILProcessor IL { get; private set; }

		public MethodBody Body { get; private set; }

		public ModuleDefinition Module { get; private set; }
	}
}