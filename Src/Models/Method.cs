using System.Text.Json.Serialization;

namespace Facepunch.AssemblySchema;

public partial class Schema
{
	public class Method : BaseMember
	{
		[JsonIgnore]
		internal MethodDefinition Source { get; set; }

		public string ReturnType { get; set; }
		public bool IsVirtual { get; set; }
		public bool IsOverride { get; set; }
		public bool IsSealed { get; set; }
		public List<Parameter> Parameters { get; set; }

		[JsonPropertyName( "l" )]
		public Location Location { get; set; }

		public class Parameter
		{
			[JsonPropertyName( "n" )]
			public string Name { get; set; }

			[JsonPropertyName( "o" )]
			public bool IsOut { get; set; }

			[JsonPropertyName( "i" )]
			public bool IsIn { get; set; }

			[JsonPropertyName( "r" )]
			public bool IsRef { get; set; }

			[JsonPropertyName( "t" )]
			public string ParameterType { get; set; }

			[JsonPropertyName( "d" )]
			public string DefaultValue { get; set; }

			internal static Parameter From( ParameterDefinition x )
			{
				var a = new Parameter();
				a.Name = x.Name;
				a.IsIn = x.IsIn;
				a.IsOut = x.IsOut;
				a.IsRef = !x.IsOut && x.ParameterType.IsByReference && !x.IsIn;
				a.ParameterType = Builder.GetTypeName( x.ParameterType, null );

				if ( x.IsOptional )
				{
					a.DefaultValue = "default";
				}

				if ( x.HasConstant )
				{
					a.DefaultValue = x.Constant?.ToString() ?? "null";
				}

				return a;
			}

			Type _parameterType;
			public Type GetParameterType() => _parameterType;

			internal void Restore( Method method, Type type, Schema schema )
			{
				_parameterType = schema.FindType( ParameterType );
				if ( _parameterType is null ) return;
				_parameterType.RegisterUsage( method );
			}
		}

		internal static Method From( Builder builder, Type t, MethodDefinition member )
		{
			var m = new Method();
			m.Source = member;
			m.Name = member.Name;
			m.ReturnType = Builder.GetTypeName( member.ReturnType, "System.Void" );
			m.IsPublic = member.IsPublic;
			m.FullName = $"{t.FullName}.{m.Name}";
			m.IsStatic = member.IsStatic;
			m.IsVirtual = member.IsVirtual;
			m.IsOverride = member.HasOverrides;
			m.IsSealed = member.IsFinal;
			m.DeclaringType = t.FullName;
			m.Attributes = Schema.Attribute.From( member.CustomAttributes );

			m.Parameters = member.Parameters.Select( x => Parameter.From( x ) ).ToList();

			m.DocumentationId = builder.GetDocumentationId( member );
			m.Documentation = builder.FindDocumentation( m.DocumentationId );
			m.Location = Location.From( builder, member, t._projectPath );

			return m;
		}

		Type _returnType;
		public Type GetReturnType() => _returnType;

		internal override void Restore( Type type, Schema schema )
		{
			base.Restore( type, schema );

			_returnType = schema.FindType( ReturnType );
			_returnType?.RegisterUsage( this );

			if ( Parameters is not null )
			{
				foreach ( var p in Parameters )
				{
					p.Restore( this, type, schema );
				}
			}

			if ( IsExtension && Parameters.First()?.GetParameterType() is { } extensionTarget )
			{
				extensionTarget.Methods ??= new();
				extensionTarget.Methods.Add( this );
			}
		}
	}
}
