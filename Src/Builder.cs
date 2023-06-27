global using Mono.Cecil;

using Mono.Collections.Generic;

namespace Facepunch.AssemblySchema;

public partial class Builder
{
	List<AssemblyDefinition> Assemblies { get; } = new();
	List<XmlDocumentation> Documentation { get; } = new();

	public void AddAssembly( byte[] assemblyData )
	{
		using var ms = new MemoryStream( assemblyData );
		var assembly = AssemblyDefinition.ReadAssembly( ms, new Mono.Cecil.ReaderParameters { InMemory = true, ReadingMode = Mono.Cecil.ReadingMode.Immediate } );
		Assemblies.Add( assembly );
	}

	public void AddDocumentation( byte[] bytes )
	{
		var d = AssemblySchema.XmlDocumentation.ReadFromString( bytes );
		Documentation.Add( d );
	}

	/// <summary>
	/// Do the work and create the schema
	/// </summary>
	public Schema Build()
	{
		var info = new Schema();
		info.Types ??= new();

		foreach ( var a in Assemblies )
		{
			ProcessAssembly( info, a );
		}

		foreach ( var t in info.Types )
		{
			t.Polish( info );
		}

		return info;
	}
}

