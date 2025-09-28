using System.Reflection.PortableExecutable;
using Mono.Cecil.Cil;
using System.Text.Json.Serialization;

namespace Facepunch.AssemblySchema;

public partial class Schema
{
	public class BaseMember
	{
		public bool IsPublic { get; set; }
		public bool IsProtected { get; set; }
		public bool IsStatic { get; set; }
		public bool IsExtension { get; set; }
		public string FullName { get; set; }
		public string Name { get; set; }
		public string DeclaringType { get; set; }

		public List<Attribute> Attributes { get; set; }
		public Documentation Documentation { get; set; }

		[JsonPropertyName( "DocId" )]
		public string DocumentationId { get; set; }

		[JsonPropertyName( "Loc" )]
		public Location Location { get; set; }

		Type _declaringType;
		public Type GetDeclaringType() => _declaringType;

		internal virtual void Restore( Type type, Schema schema )
		{
			_declaringType = type;

			if ( Attributes is not null )
			{
				foreach ( var attr in Attributes )
				{
					attr.Restore( this, type, schema );
				}
			}
		}
	}

	public class Location
	{
		[JsonPropertyName( "File" )]
		public string File { get; set; }

		[JsonPropertyName( "Line" )]
		public int Line { get; set; }

		internal static Location From( Builder builder, MethodDefinition member, string projectPath )
		{
			SequencePoint sp = default;
			sp = member.DebugInformation?.SequencePoints?.FirstOrDefault();
			if ( sp == null ) return null;

			var file = sp.Document.Url.Replace( "\\", "/" );
			file = file[projectPath.Length..].TrimStart( '/' );

			return new Location() { File = file, Line = sp.StartLine };
		}

		internal static Location From( Builder builder, FieldDefinition field, string projectPath )
		{
			var declaringType = field.DeclaringType;

			var constructors = declaringType.Methods
				.Where( m => m.IsConstructor && m.HasBody )
				.Where( m => m.DebugInformation.HasSequencePoints );

			foreach ( var ctor in constructors )
			{
				var instructions = ctor.Body.Instructions;
				foreach ( var instruction in instructions )
				{
					if ( (instruction.OpCode == OpCodes.Stfld || instruction.OpCode == OpCodes.Stsfld) &&
					     instruction.Operand is FieldReference fr &&
					     fr.FullName == field.FullName )
					{
						SequencePoint sp = ctor.DebugInformation.SequencePoints
							.LastOrDefault( s => s.Offset <= instruction.Offset );

						if ( sp != null )
						{
							var file = sp.Document.Url.Replace( "\\", "/" );
							file = file[projectPath.Length..].TrimStart( '/' );

							return new Location() { File = file, Line = sp.StartLine };
						}
					}
				}
			}

			return null;
		}
	}
}
