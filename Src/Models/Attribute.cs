namespace Facepunch.AssemblySchema;

public partial class Schema
{
	public class Attribute
	{
		public string FullName { get; set; }
		public object[] ConstructorArguments { get; set; }

		internal static Attribute From( CustomAttribute x )
		{
			// Ignore compiler generated attributes, we don't care about them.
			if ( x.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerFeatureRequiredAttribute" ) return null;



			// The compiler adds this shit to every ref struct - just ignore it!
			// https://turnerj.com/blog/ref-structs-are-technically-obsolete
			//
			if ( x.AttributeType.FullName == "System.ObsoleteAttribute" && x.ConstructorArguments.Count > 0 )
			{
				var str = x.ConstructorArguments[0].Value?.ToString();

				if ( str == "Types with embedded references are not supported in this version of your compiler." )
				{
					return null;
				}
			}

			var a = new Attribute();
			a.FullName = Builder.GetTypeName( x.AttributeType, null );
			a.ConstructorArguments = x.ConstructorArguments.Select( x => $"{x.Value}" ).ToArray();
			if ( a.ConstructorArguments.Length == 0 ) a.ConstructorArguments = null;



			return a;
		}

		internal static List<Attribute> From( Mono.Collections.Generic.Collection<CustomAttribute> x )
		{
			if ( x.Count == 0 ) return null;

			var list = x.Select( x => From( x ) ).Where( x => x != null ).ToList();
			if ( list.Count == 0 ) return null;

			return list;
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
