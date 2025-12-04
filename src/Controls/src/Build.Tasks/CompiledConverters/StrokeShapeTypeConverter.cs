using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using ShapeConverter = Microsoft.Maui.Controls.Shapes.StrokeShapeTypeConverter;

namespace Microsoft.Maui.Controls.XamlC;

class StrokeShapeTypeConverter : ICompiledTypeConverter
{
	public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
	{
		var module = context.Body.Method.Module;

		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();

			if (value.StartsWith(ShapeConverter.Ellipse, StringComparison.Ordinal))
			{
				yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "Ellipse"), parameterTypes: null));

				yield break;
			}

			if (value.StartsWith(ShapeConverter.Line, StringComparison.Ordinal))
			{
				var parts = value.Split(ShapeConverter.Delimiter, 2);
				if (parts.Length != 2)
				{
					yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "Line"), parameterTypes: null));
					yield break;
				}

				var pointCollectionConverter = new PointCollectionConverter();
				var points = pointCollectionConverter.ConvertFromString(parts[1]) as PointCollection;

				if (points == null || points.Count == 0)
				{
					yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "Line"), parameterTypes: null));
					yield break;
				}

				var p1 = points[0];
				if (points.Count == 1)
				{
					yield return Instruction.Create(OpCodes.Ldc_R8, p1.X);
					yield return Instruction.Create(OpCodes.Ldc_R8, p1.Y);
					yield return Instruction.Create(OpCodes.Ldc_R8, 0d);
					yield return Instruction.Create(OpCodes.Ldc_R8, 0d);
					yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "Line"), parameterTypes: new[] {
																								("mscorlib", "System", "Double"),
																								("mscorlib", "System", "Double"),
																								("mscorlib", "System", "Double"),
																								("mscorlib", "System", "Double")}));

					yield break;
				}

				var p2 = points[1];
				yield return Instruction.Create(OpCodes.Ldc_R8, p1.X);
				yield return Instruction.Create(OpCodes.Ldc_R8, p1.Y);
				yield return Instruction.Create(OpCodes.Ldc_R8, p2.X);
				yield return Instruction.Create(OpCodes.Ldc_R8, p2.Y);

				yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "Line"), parameterTypes: new[] {
																							("mscorlib", "System", "Double"),
																							("mscorlib", "System", "Double"),
																							("mscorlib", "System", "Double"),
																							("mscorlib", "System", "Double")}));

				yield break;
			}

			if (value.StartsWith(ShapeConverter.Path, StringComparison.Ordinal))
			{
				// NOTE: calls new PathGeometryConverter().ConvertFromInvariantString("...")
				// PathGeometry could be precompiled one day

				var parts = value.Split(ShapeConverter.Delimiter, 2);
				if (parts.Length != 2)
				{
					yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "Path"), parameterTypes: null));
					yield break;
				}

				yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "Path"), parameterTypes: null));
				yield return Instruction.Create(OpCodes.Dup);
				yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "PathGeometryConverter"), parameterTypes: null));
				yield return Instruction.Create(OpCodes.Ldstr, parts[1]);
				yield return Instruction.Create(OpCodes.Call, module.ImportMethodReference(context.Cache, ("System", "System.ComponentModel", "TypeConverter"), "ConvertFromInvariantString", parameterTypes: new[] {
					("mscorlib", "System", "String")}));
				yield return Instruction.Create(OpCodes.Call, module.ImportPropertySetterReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "Path"), "Data"));
				yield break;
			}

			if (value.StartsWith(ShapeConverter.Polygon, StringComparison.Ordinal))
			{
				var parts = value.Split(ShapeConverter.Delimiter, 2);
				if (parts.Length != 2)
				{
					yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "Polygon"), parameterTypes: null));
					yield break;
				}

				var pointCollectionConverter = new PointCollectionConverter();
				var points = pointCollectionConverter.ConvertFromString(parts[1]) as PointCollection;

				if (points == null || points.Count == 0)
				{
					yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "Polygon"), parameterTypes: null));
					yield break;
				}

				foreach (var instruction in CreatePointCollection(context, module, points))
				{
					yield return instruction;
				}

				yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "Polygon"), parameterTypes: new[] {
																							("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "PointCollection")}));
				yield break;
			}

			if (value.StartsWith(ShapeConverter.Polyline, StringComparison.Ordinal))
			{
				var parts = value.Split(ShapeConverter.Delimiter, 2);
				if (parts.Length != 2)
				{
					yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "Polyline"), parameterTypes: null));
					yield break;
				}

				var pointCollectionConverter = new PointCollectionConverter();
				var points = pointCollectionConverter.ConvertFromString(parts[1]) as PointCollection;

				if (points == null || points.Count == 0)
				{
					yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "Polyline"), parameterTypes: null));
					yield break;
				}

				foreach (var instruction in CreatePointCollection(context, module, points))
				{
					yield return instruction;
				}

				yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "Polyline"), parameterTypes: new[] {
																							("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "PointCollection")}));
				yield break;
			}

			if (value.StartsWith(ShapeConverter.Rectangle, StringComparison.Ordinal))
			{
				yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "Rectangle"), parameterTypes: null));
				yield break;
			}

			if (value.StartsWith(ShapeConverter.RoundRectangle, StringComparison.Ordinal))
			{
				var parts = value.Split(ShapeConverter.Delimiter, 2);
				if (parts.Length != 2)
				{
					yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "RoundRectangle"), parameterTypes: null));
					yield break;
				}

				yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "RoundRectangle"), parameterTypes: null));
				yield return Instruction.Create(OpCodes.Dup);

				var cornerRadiusTypeConverter = new Converters.CornerRadiusTypeConverter();
				var cornerRadius = (CornerRadius)cornerRadiusTypeConverter.ConvertFromString(parts[1]);

				yield return Instruction.Create(OpCodes.Ldc_R8, cornerRadius.TopLeft);
				yield return Instruction.Create(OpCodes.Ldc_R8, cornerRadius.TopRight);
				yield return Instruction.Create(OpCodes.Ldc_R8, cornerRadius.BottomLeft);
				yield return Instruction.Create(OpCodes.Ldc_R8, cornerRadius.BottomRight);

				yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui", "Microsoft.Maui", "CornerRadius"), parameterTypes: new[] {
					("mscorlib", "System", "Double"),
					("mscorlib", "System", "Double"),
					("mscorlib", "System", "Double"),
					("mscorlib", "System", "Double")}));

				yield return Instruction.Create(OpCodes.Call, module.ImportPropertySetterReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "RoundRectangle"), "CornerRadius"));
				yield break;
			}

			if (double.TryParse(value, out double radius))
			{
				yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "Rectangle"), parameterTypes: null));
				yield return Instruction.Create(OpCodes.Dup);

				yield return Instruction.Create(OpCodes.Ldc_R8, radius);
				yield return Instruction.Create(OpCodes.Ldc_R8, radius);
				yield return Instruction.Create(OpCodes.Ldc_R8, radius);
				yield return Instruction.Create(OpCodes.Ldc_R8, radius);

				yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui", "Microsoft.Maui", "CornerRadius"), parameterTypes: new[] {
					("mscorlib", "System", "Double"),
					("mscorlib", "System", "Double"),
					("mscorlib", "System", "Double"),
					("mscorlib", "System", "Double")}));

				yield return Instruction.Create(OpCodes.Call, module.ImportPropertySetterReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Shapes", "RoundRectangle"), "CornerRadius"));
				yield break;
			}
		}
		throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(IShape));
	}

	IEnumerable<Instruction> CreatePointCollection(ILContext context, ModuleDefinition module, PointCollection points)
	{
		var pointType = module.ImportReference(context.Cache, ("Microsoft.Maui.Graphics", "Microsoft.Maui.Graphics", "Point"));
		yield return Instruction.Create(OpCodes.Ldc_I4, points.Count);
		yield return Instruction.Create(OpCodes.Newarr, pointType);

		var pointTypeConverter = new PointTypeConverter();
		for (int i = 0; i < points.Count; i++)
		{
			yield return Instruction.Create(OpCodes.Dup);
			yield return Instruction.Create(OpCodes.Ldc_I4, i);

			foreach (var instruction in pointTypeConverter.CreatePoint(context, module, points[i]))
			{
				yield return instruction;
			}

			// Point is a struct, so we can't use Stelem_Ref
			yield return Instruction.Create(OpCodes.Stelem_Any, pointType);
		}

		yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, type: ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "PointCollection"), paramCount: 1));
	}
}
