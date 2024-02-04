global using Mono.Cecil;

namespace Facepunch.AssemblySchema;

public partial class Builder
{
	private void ProcessAssembly(Schema info, AssemblyDefinition assembly)
	{
		foreach (var type in assembly.MainModule.Types)
		{
			info.Types.AddRange(ProcessType(assembly, type));
		}
	}

	internal Documentation FindDocumentation(string name)
	{
		foreach (var doc in Documentation)
		{
			if (doc.Entries.TryGetValue(name, out var entry))
				return entry;
		}

		return null;
	}

	bool IsAttribute(TypeDefinition typeDef)
	{
		if (typeDef?.BaseType is null) return false;
		if (typeDef.BaseType.FullName == "System.Attribute") return true;

		return IsAttribute(typeDef.BaseType.Resolve());
	}

	private IEnumerable<Schema.Type> ProcessType(AssemblyDefinition a, TypeDefinition type)
	{
		if (type.IsNestedPrivate || type.FullName == "<Module>")
			return Array.Empty<Schema.Type>();

		if (type.CustomAttributes.Any(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute"))
			return Array.Empty<Schema.Type>();

		var t = new Schema.Type();

		t.Source = type;
		t.IsPublic = type.IsPublic;
		t.IsStatic = type.IsSealed && type.IsAbstract;
		t.FullName = type.FullName;
		t.Namespace = type.Namespace;
		t.Name = type.Name;
		t.IsAttribute = type.IsClass && IsAttribute(type);
		t.IsClass = type.IsClass;
		t.IsEnum = type.IsEnum;
		t.IsValueType = type.IsValueType;
		t.IsInterface = type.IsInterface;
		t.IsAbstract = type.IsAbstract;
		t.IsSealed = type.IsSealed;
		t.DeclaringType = type.DeclaringType?.FullName;
		t.Group = "class";
		t.Attributes = Schema.Attribute.From(type.CustomAttributes);
		t.Documentation = FindDocumentation($"T:{type.FullName}");

		if (type.IsValueType) t.Group = "struct";
		if (type.IsInterface) t.Group = "interface";
		if (type.IsEnum) t.Group = "enum";

		if (type.HasGenericParameters)
		{
			var p = "<";

			p += string.Join(",", type.GenericParameters.Select(x => $"{x.Name}"));
			p += ">";

			var paramCount = type.GenericParameters.Count;
			var replace = $"`{paramCount}";
			t.Name = t.Name.Replace(replace, p);
			t.FullName = t.FullName.Replace(replace, p);
		}

		foreach (var member in type.Methods)
		{
			ProcessMethod(t, member);
		}

		foreach (var property in type.Properties)
		{
			ProcessProperty(t, property);
		}

		foreach (var field in type.Fields)
		{
			ProcessField(t, field);
		}

		var types = new List<Schema.Type> { t };

		foreach (var nestedType in type.NestedTypes)
		{
			if (nestedType.IsNestedPrivate) continue;

			types.AddRange(ProcessType(a, nestedType));
		}

		return types;
	}

	private void ProcessMethod(Schema.Type t, MethodDefinition member)
	{
		if (member.IsPrivate) return;
		if (member.IsSpecialName) return;
		if (member.Name == "GetHashCode") return;
		if (member.Name == "ToString") return;
		if (member.Name == "Equals") return;
		if (member.DeclaringType.Properties.Any(x => x.GetMethod == member || x.SetMethod == member))
			return;

		var m = Schema.Method.From(this, t, member);


		t.Methods ??= new List<Schema.Method>();
		t.Methods.Add(m);
	}

	private void ProcessProperty(Schema.Type t, PropertyDefinition member)
	{
		var m = Schema.Property.From(this, t, member);
		t.Properties ??= new List<Schema.Property>();
		t.Properties.Add(m);
	}

	private void ProcessField(Schema.Type t, FieldDefinition member)
	{
		if (member.IsPrivate) return;
		if (member.Name == "value__" && t.IsEnum) return;

		var m = Schema.Field.From(this, t, member);

		t.Fields ??= new();
		t.Fields.Add(m);
	}
}

