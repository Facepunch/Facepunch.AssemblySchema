namespace Facepunch.AssemblySchema;

public partial class Schema
{
    public class Field : BaseMember
	{
        public string FieldType { get; set; }

		internal static Field From(Builder builder, Type t, FieldDefinition member)
		{
			var m = new Field();
			m.Name = member.Name;
			m.FieldType = member.FieldType?.FullName ?? "void";
			m.IsPublic = member.IsPublic;
			m.FullName = $"{t.FullName}.{m.Name}";
			m.IsStatic = member.IsStatic;
			m.Attributes = Attribute.From(member.CustomAttributes);
			m.Documentation = builder.FindDocumentation($"F:{member.DeclaringType.FullName}.{member.Name}");
			return m;
		}
	}
}
