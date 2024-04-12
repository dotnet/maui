using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Cil;
using static System.String;
using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace Microsoft.Maui.Controls.Build.Tasks;

class OnIdiomExtension : ICompiledMarkupExtension
{
	public IEnumerable<Instruction> ProvideValue(IElementNode node, ModuleDefinition module, ILContext context, out TypeReference memberRef)
	{
		//if all idiom values have the same type, we can return that type for memberRef, otherwise we default to object
		memberRef = module.TypeSystem.Object;

		var visitor = new SetPropertiesVisitor(context, stopOnResourceDictionary: true);
		foreach (var cnode in node.Properties.Values.ToList())
		{
			if (!(cnode is IElementNode enode))
				continue;
			foreach (var jnode in enode.Properties.Values.ToList())
				jnode.Accept(visitor, jnode);
			foreach (var jnode in enode.CollectionItems)
				jnode.Accept(visitor, jnode);
		}

		var parentNode = node.Parent;
		if (!SetPropertiesVisitor.TryGetPropertyName(node, parentNode, out var propertyName))
			return null;

		var idioms = new[] { "Phone", "Tablet", "Desktop", "TV", "Watch", "Default" };
		var idiomProperties = node.Properties.Where(p => idioms.Contains(p.Key.LocalName)).ToList();

		//assign memberRef to the type of the first idiom value, then compare with the other ones
		memberRef = (idiomProperties.FirstOrDefault().Value as IElementNode).XmlType.GetTypeReference(context.Cache, module, node as IXmlLineInfo);
		foreach (var idiom in idiomProperties.Skip(1))
		{
			var idiomTypeRef = (idiom.Value as IElementNode).XmlType.GetTypeReference(context.Cache, module, node as IXmlLineInfo);
			if (!TypeRefComparer.Default.Equals(memberRef, idiomTypeRef))
			{
				//should this issue a warning?
				memberRef = module.TypeSystem.Object;
			}
		}

		var localName = propertyName.LocalName;
		TypeReference declaringTypeReference = null;
		FieldReference bpRef = null;
		PropertyDefinition propertyRef = null;
		if (parentNode is IElementNode && propertyName != XmlName.Empty)
		{
			var parentTypeRef = ((IElementNode)parentNode).XmlType.GetTypeReference(context.Cache, module, node as IXmlLineInfo);
			bpRef = SetPropertiesVisitor.GetBindablePropertyReference(parentTypeRef, propertyName.NamespaceURI, ref localName, out _, context, node as IXmlLineInfo);
			propertyRef = parentTypeRef.GetProperty(context.Cache, pd => pd.Name == localName, out declaringTypeReference);
		}

		var targetTypeRef = bpRef?.GetBindablePropertyType(context.Cache, node as IXmlLineInfo, module) ?? propertyRef?.PropertyType;

		var instructions = new List<Instruction>();
		
		//var idiom = DeviceInfo.Idiom;
		var deviceIdiomTypeRef = module.ImportReference(context.Cache, ("Microsoft.Maui.Essentials", "Microsoft.Maui.Devices", "DeviceIdiom"));
		var idiomVarDef = new VariableDefinition(deviceIdiomTypeRef);
		context.Body.Variables.Add(idiomVarDef);
		instructions.Add(Create(Call, module.ImportPropertyGetterReference(context.Cache, ("Microsoft.Maui.Essentials", "Microsoft.Maui.Devices", "DeviceInfo"),
				propertyName: "Idiom", isStatic: true)));
		instructions.Add(Create(Stloc, idiomVarDef));

		var ret_varDef = new VariableDefinition(memberRef);
		context.Body.Variables.Add(ret_varDef);
		
		var ldRetValue = Create(Nop);
		var instr_getDefault = Create(Nop);
		
		idioms = new[] { "Phone", "Tablet", "Desktop", "TV", "Watch" }; //avoid Default this time
		foreach(var idiom in idioms)
		{
			if (node.Properties.ContainsKey(new XmlName("", idiom)))
			{
				//if (idiom == DeviceIdiom.Phone)
				instructions.Add(Create(Ldloc, idiomVarDef));
				instructions.Add(Create(Call, module.ImportPropertyGetterReference(context.Cache, ("Microsoft.Maui.Essentials", "Microsoft.Maui.Devices", "DeviceIdiom"),
						propertyName: idiom, isStatic: true)));
				instructions.Add(Create(Call, module.ImportMethodReference(context.Cache, ("Microsoft.Maui.Essentials", "Microsoft.Maui.Devices", "DeviceIdiom"),
						methodName: "op_Equality",
						parameterTypes: new[] { ("Microsoft.Maui.Essentials", "Microsoft.Maui.Devices", "DeviceIdiom"), ("Microsoft.Maui.Essentials", "Microsoft.Maui.Devices", "DeviceIdiom") },
						isStatic: true)));
				instructions.Add(Create(Brfalse, instr_getDefault));
				instructions.Add(Create(Ldloc, context.Variables[node.Properties[new XmlName("", idiom)] as IElementNode]));			
				instructions.Add(Create(Stloc, ret_varDef));
				instructions.Add(Create(Br, ldRetValue));
			}
		}
	
		instructions.Add(instr_getDefault);
		if (node.Properties.ContainsKey(new XmlName("","Default"))){
			instructions.Add(Create(Ldloc, context.Variables[node.Properties[new XmlName("", "Default")] as IElementNode]));
			instructions.Add(Create(Stloc, ret_varDef));
		}
		else{
			instructions.Add(Create(Ldnull));
			instructions.Add(Create(Stloc, ret_varDef));
		}
		instructions.Add(ldRetValue);
		instructions.Add(Create(Ldloc, ret_varDef));

		return instructions;
	}
}
