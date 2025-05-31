using System.Text.Json.Serialization;

namespace Facepunch.AssemblySchema;

public partial class Schema
{
	public class Property : BaseMember
	{
		public string PropertyType { get; set; }
		public bool IsVirtual { get; set; }
		public bool IsOverride { get; set; }
		public bool IsSealed { get; set; }

		[JsonPropertyName( "l" )]
		public Location Location { get; set; }

		internal static Property From( Builder builder, Type t, PropertyDefinition member )
		{
			var m = new Property();
			m.Name = member.Name;
			m.PropertyType = Builder.GetTypeName( member.PropertyType, "void" );
			m.IsPublic = member.GetMethod?.IsPublic ?? false;
			m.IsPublic = m.IsPublic || (member.SetMethod?.IsPublic ?? false);
			m.FullName = $"{t.FullName}.{m.Name}";
			m.IsStatic = member.GetMethod?.IsStatic ?? member.SetMethod.IsStatic;
			m.IsVirtual = member.GetMethod?.IsVirtual ?? member.SetMethod.IsVirtual;
			m.IsOverride = member.GetMethod?.HasOverrides ?? member.SetMethod.HasOverrides;
			m.IsSealed = member.GetMethod?.IsFinal ?? member.SetMethod.IsFinal;
			m.Attributes = Attribute.From( member.CustomAttributes );
			m.DocumentationId = builder.GetDocumentationId( member );
			m.Documentation = builder.FindDocumentation( m.DocumentationId );
			m.Location = Location.From( builder, member.SetMethod ?? member.GetMethod, t._projectPath );

			return m;
		}

		Type _propertyType;
		public Type GetPropertyType() => _propertyType;

		internal override void Restore( Type type, Schema schema )
		{
			base.Restore( type, schema );

			_propertyType = schema.FindType( PropertyType );

			if ( _propertyType is not null )
			{
				_propertyType.RegisterUsage( this );
			}
		}
	}
}

