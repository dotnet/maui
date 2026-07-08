namespace Microsoft.Maui.Controls.BindingSourceGen;

using static Microsoft.Maui.Controls.BindingSourceGen.UnsafeAccessorsMethodName;

public sealed record Setter(string[] PatternMatchingExpressions, string AssignmentStatement)
{
	public static Setter From(
		IEnumerable<IPathPart> path,
		string sourceVariableName = "source",
		string assignedValueExpression = "value")
	{
		string accessAccumulator = sourceVariableName;
		List<string> patternMatchingExpressions = new();
		bool skipNextConditionalAccess = false;

		void AddPatternMatchingExpression(string pattern)
		{
			var tmpVariableName = $"p{patternMatchingExpressions.Count}";
			patternMatchingExpressions.Add($"{accessAccumulator} is {pattern} {tmpVariableName}");
			accessAccumulator = tmpVariableName;
		}

		var parts = path as IReadOnlyList<IPathPart> ?? path.ToArray();

		for (int i = 0; i < parts.Count; i++)
		{
			var part = parts[i];
			var skipConditionalAccess = skipNextConditionalAccess;
			skipNextConditionalAccess = false;
			bool isLastPart = i == parts.Count - 1;
			var nextPart = isLastPart ? null : parts[i + 1];

			if (part is Cast { TargetType: var targetType })
			{
				AddPatternMatchingExpression(targetType.GlobalName);

				// the current `is T` expression makes sure that the value is not null
				// so if a conditional access to a member/indexer follows, we can skip the next null check
				skipNextConditionalAccess = true;
			}
			else if (part is ConditionalAccess { Part: var innerPart })
			{
				if (!skipConditionalAccess)
				{
					AddPatternMatchingExpression("{}");
				}

				accessAccumulator = AccessExpressionBuilder.ExtendExpression(accessAccumulator, innerPart);

				// A value-type member (that is not a field) accessed mid-path through a possibly-null
				// receiver is an rvalue, so a following inline member/index assignment would fail with
				// CS1612 ("cannot modify the return value ... because it is not a variable"). Capture it
				// into a local first. If the next part is itself a conditional access, it introduces its
				// own local, so no extra capture is needed. Fields are variables (lvalues) and never need this.
				if (!isLastPart
					&& nextPart is not ConditionalAccess
					&& innerPart is MemberAccess { IsValueType: true, Kind: not AccessorKind.Field })
				{
					AddPatternMatchingExpression("{}");
				}
			}
			else if (part is MemberAccess { IsValueType: true } && !isLastPart)
			{
				// It is necessary to create a variable for value types in order to set their properties.
				// We can simply reuse the pattern matching mechanism to declare the variable.
				accessAccumulator = AccessExpressionBuilder.ExtendExpression(accessAccumulator, part);
				AddPatternMatchingExpression("{}");
			}
			else if (!isLastPart)
			{
				// For non-last parts, extend the expression using the getter path
				accessAccumulator = AccessExpressionBuilder.ExtendExpression(accessAccumulator, part);
			}
			// For the last part, we don't extend the expression here
			// The assignment is handled by BuildAssignmentStatement
		}

		return new Setter(
			patternMatchingExpressions.ToArray(),
			AssignmentStatement: BuildAssignmentStatement(accessAccumulator, path.Any() ? path.Last() : null, assignedValueExpression));
	}

	public static string BuildAssignmentStatement(string accessAccumulator, IPathPart? lastPart, string assignedValueExpression = "value") =>
		lastPart switch
		{
			MemberAccess { Kind: AccessorKind.Field, IsSetterInaccessible: true } memberAccess => $"{CreateUnsafeFieldAccessorMethodName(memberAccess.MemberName)}({accessAccumulator}) = {assignedValueExpression};",
			MemberAccess { Kind: AccessorKind.Property, IsSetterInaccessible: true } memberAccess => $"{CreateUnsafePropertyAccessorSetMethodName(memberAccess.MemberName)}({accessAccumulator}, {assignedValueExpression});",
			MemberAccess memberAccess => $"{accessAccumulator}.{memberAccess.MemberName} = {assignedValueExpression};",
			IndexAccess indexAccess => indexAccess.Index switch
			{
				int numericIndex => $"{accessAccumulator}[{numericIndex}] = {assignedValueExpression};",
				EnumIndex enumIndex => $"{accessAccumulator}[{enumIndex.FullyQualifiedEnumValue}] = {assignedValueExpression};",
				string stringIndex => $"{accessAccumulator}[\"{stringIndex}\"] = {assignedValueExpression};",
				_ => throw new NotSupportedException($"Unsupported index type: {indexAccess.Index.GetType()}"),
			},
			_ => $"{accessAccumulator} = {assignedValueExpression};",
		};
}
