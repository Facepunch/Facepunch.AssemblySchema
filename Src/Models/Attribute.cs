using System.Text.Json.Serialization;

namespace Facepunch.AssemblySchema;

public partial class Schema
{
	public class Attribute
	{
		[JsonPropertyName( "n" )]
		public string FullName { get; set; }

		[JsonPropertyName( "a" )]
		public object[] ConstructorArguments { get; set; }

		internal static Attribute From( CustomAttribute x )
		{
			var a = new Attribute();
			a.FullName = Builder.GetTypeName( x.AttributeType, null );
			a.ConstructorArguments = x.ConstructorArguments.Select( x => $"{x.Value}" ).ToArray();
			if ( a.ConstructorArguments.Length == 0 ) a.ConstructorArguments = null;

			return a;
		}

		internal static List<Attribute> From( Mono.Collections.Generic.Collection<CustomAttribute> x )
		{
			if ( x.Count == 0 ) return null;

			return x.Select( x => From( x ) ).ToList();
		}

		Type _attributeType;
		public Type GetAttributeType() => _attributeType;

		internal void Restore( BaseMember member, Type type, Schema schema )
		{
			_attributeType = schema.FindType( FullName, true );
			_attributeType?.RegisterUsage( member );
		}
	}

}
