namespace Facepunch.AssemblySchema;

public partial class Schema
{
    public class Property : BaseMember
	{
        public string PropertyType { get; set; }

		internal static Property From( Builder builder, Type t, PropertyDefinition member)
		{
			var m = new Property();
			m.Name = member.Name;
			m.PropertyType = member.PropertyType?.FullName ?? "void";
			m.IsPublic = member.GetMethod?.IsPublic ?? false;
			m.IsPublic = m.IsPublic || (member.SetMethod?.IsPublic ?? false);
			m.FullName = $"{t.FullName}.{m.Name}";
			m.IsStatic = member.GetMethod?.IsStatic ?? member.SetMethod.IsStatic;
			m.Attributes = Attribute.From(member.CustomAttributes);
			m.Documentation = builder.FindDocumentation($"P:{member.DeclaringType.FullName}.{member.Name}");

			return m;
		}
	}
}

