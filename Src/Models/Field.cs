namespace Facepunch.AssemblySchema;

public partial class Schema
{
	public class Field : BaseMember
	{
		public string FieldType { get; set; }

		internal static Field From( Builder builder, Type t, FieldDefinition member )
		{
			var m = new Field();
			m.Name = member.Name;
			m.FieldType = Builder.GetTypeName( member.FieldType, "void" );
			m.IsPublic = member.IsPublic;
			m.IsProtected = member.IsFamily;
			m.FullName = $"{t.FullName}.{m.Name}";
			m.IsStatic = member.IsStatic;
			m.Attributes = Attribute.From( member.CustomAttributes );
			m.DocumentationId = builder.GetDocumentationId( member );
			m.Documentation = builder.FindDocumentation( m.DocumentationId );

			return m;
		}

		Type _fieldType;
		public Type GetFieldType() => _fieldType;

		internal override void Restore( Type type, Schema schema )
		{
			base.Restore( type, schema );

			_fieldType = schema.FindType( FieldType );

			if ( _fieldType is not null )
			{
				_fieldType.RegisterUsage( this );
			}
		}
	}
}
