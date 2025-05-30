namespace Facepunch.AssemblySchema;

public partial class Schema
{
	public class Attribute
	{
		public string FullName { get; set; }
		public object[] ConstructorArguments { get; set; }

		internal static Attribute From( CustomAttribute x )
		{
			var a = new Attribute();
			a.FullName = x.AttributeType.FullName;
			a.ConstructorArguments = x.ConstructorArguments.Select( x => $"{x.Value}" ).ToArray();
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
