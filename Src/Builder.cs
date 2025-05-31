using Mono.Cecil.Cil;

namespace Facepunch.AssemblySchema;

public partial class Builder : IAssemblyResolver
{
	List<AssemblyDefinition> Assemblies { get; } = new();
	List<XmlDocumentation> Documentation { get; } = new();

	public void AddAssembly( byte[] assemblyData, byte[] pdbData = null )
	{
		var readParams = new ReaderParameters
		{
			AssemblyResolver = this,
			InMemory = true,
			ReadingMode = ReadingMode.Deferred,
			ReadSymbols = pdbData != null,
			SymbolStream = pdbData != null ? new MemoryStream( pdbData ) : null,
			SymbolReaderProvider = pdbData != null ? new DefaultSymbolReaderProvider() : null,
			ThrowIfSymbolsAreNotMatching = false
		};

		var ms = new MemoryStream( assemblyData );
		var assembly = AssemblyDefinition.ReadAssembly( ms, readParams );
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

	public void Dispose()
	{
		foreach ( var a in Assemblies )
			a.Dispose();

		Assemblies.Clear();
		Documentation.Clear();

		return;
	}

	public AssemblyDefinition Resolve( AssemblyNameReference name )
	{
		return Assemblies.FirstOrDefault( x => x.Name.Name == name.Name );
	}

	public AssemblyDefinition Resolve( AssemblyNameReference name, ReaderParameters parameters )
	{
		return Resolve( name );
	}
}

