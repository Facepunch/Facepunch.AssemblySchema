using Facepunch.AssemblySchema;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Test;

[TestClass]
public class ProcessorTest
{
	public Schema GetSchema( bool loadSymbols = true )
	{
		using var s = new Facepunch.AssemblySchema.Builder();
		s.AddAssembly( System.IO.File.ReadAllBytes( GetType().Assembly.Location ), loadSymbols ? System.IO.File.ReadAllBytes( GetType().Assembly.Location.Replace( ".dll", ".pdb" ) ) : null );
		s.AddDocumentation( System.IO.File.ReadAllBytes( GetType().Assembly.Location.Replace( ".dll", ".xml" ) ) );

		return s.Build();
	}

	[TestMethod]
	public void SerializeSchema()
	{
		var s = GetSchema();

		var options = new JsonSerializerOptions
		{
			WriteIndented = true,
		};

		var json = JsonSerializer.Serialize( s, options );
		Assert.IsNotNull( json );

		var dejson = JsonSerializer.Deserialize<Schema>( json );
		Assert.IsNotNull( dejson );

		Console.WriteLine( json.Length );
		Console.WriteLine( json );
	}

	[TestMethod]
	public void SerializeSchemaMinimal()
	{
		var s = GetSchema();

		var options = new JsonSerializerOptions
		{
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
		};

		var json = JsonSerializer.Serialize( s, options );
		Assert.IsNotNull( json );

		var dejson = JsonSerializer.Deserialize<Schema>( json );
		Assert.IsNotNull( dejson );

		Console.WriteLine( json.Length );
		Console.WriteLine( json );
	}

	[TestMethod]
	public void BasicTests()
	{
		var data = GetSchema();

		Assert.IsNotNull( data );
		Assert.AreNotEqual( 0, data.Types.Count );

		foreach ( var type in data.Types )
		{
			Console.WriteLine( $"{type.FullName}" );
		}

		var town = data.Types.FirstOrDefault( x => x.Name == "Town" );
		Assert.IsNotNull( town );
		Assert.IsNotNull( town.Documentation );
		Assert.IsNotNull( town.Documentation.Summary );

		var sp = data.Types.FirstOrDefault( x => x.Name == "Springfield" );
		Assert.IsNotNull( sp );
		Assert.IsNotNull( sp.Documentation );
		Assert.IsNotNull( sp.Documentation.Summary );

		var maggie = sp.Fields.FirstOrDefault( x => x.Name == "Maggie" );
		Assert.IsNotNull( maggie );
		Assert.IsNotNull( maggie.Documentation );
		Assert.IsNotNull( maggie.Documentation.Summary );

		var lisa = sp.Properties.FirstOrDefault( x => x.Name == "Lisa" );
		Assert.IsNotNull( lisa );
		Assert.IsNotNull( lisa.Documentation );
		Assert.IsNotNull( lisa.Documentation.Summary );

		var bart = sp.Properties.FirstOrDefault( x => x.Name == "Bart" );
		Assert.IsNotNull( bart );
		Assert.IsNotNull( bart.Documentation );
		Assert.IsNotNull( bart.Documentation.Summary );

		var tagAttr = data.Types.FirstOrDefault( x => x.Name == "TagAttribute" );
		Assert.IsNotNull( tagAttr );
		Assert.IsTrue( tagAttr.IsAttribute );

		Assert.IsNull( sp.GetBaseType() );

		// Rebuild transient data
		data.Rebuild();

		Assert.AreEqual( sp.BaseType, "Town" );
		Assert.IsNotNull( sp.GetBaseType() );
		Assert.IsNotNull( sp.GetBaseType() );

		Assert.IsNotNull( town.GetDerivedTypes() );
		Assert.IsTrue( town.GetDerivedTypes().Contains( sp ) );

		Assert.AreNotEqual( 0, town.GetUsage().Count() ); // town is used as an argument in a parameter

	}

	[TestMethod]
	public void JsonEncodable()
	{
		var data = GetSchema();

		var json = System.Text.Json.JsonSerializer.Serialize( data, new JsonSerializerOptions
		{
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
			WriteIndented = true
		} );

		Console.WriteLine( json );

		Assert.IsNotNull( json );
		Assert.AreNotEqual( 0, json.Length );
	}

	[TestMethod]
	public async Task FromUrlZip()
	{
		var url = "https://cdn.facepunch.com/sbox/releases/2024-02-03-12-14-30.zip";

		var httpClient = new HttpClient();
		var zipBytes = await httpClient.GetByteArrayAsync( url );

		using var stream = new MemoryStream( zipBytes );
		using var archive = new ZipArchive( stream );

		Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();

		foreach ( var entry in archive.Entries )
		{
			using ( var ms = new MemoryStream() )
			{
				entry.Open().CopyTo( ms );
				var bytes = ms.ToArray();

				if ( entry.FullName.EndsWith( ".dll" ) ) files[entry.FullName] = bytes;
				if ( entry.FullName.EndsWith( ".xml" ) ) files[entry.FullName] = bytes;
			}
		}

		using var processor = new Facepunch.AssemblySchema.Builder();

		foreach ( var file in files.Where( x => x.Key.EndsWith( ".xml" ) ) )
		{
			processor.AddDocumentation( file.Value );
		}

		foreach ( var file in files.Where( x => x.Key.EndsWith( ".dll" ) ) )
		{
			processor.AddAssembly( file.Value );
		}

		var result = processor.Build();

		result.Rebuild();

		// can we get a list of components
		var component = result.Types.FirstOrDefault( x => x.Name == "Component" );
		Assert.IsNotNull( component );

		foreach ( var c in component.GetDerivedTypes() )
		{
			Console.WriteLine( $"{c.FullName}" );
		}

		var physicsBody = result.Types.FirstOrDefault( x => x.Name == "PhysicsBody" );
		Assert.IsNotNull( physicsBody );

		foreach ( var c in physicsBody.GetUsage().Where( x => x.IsPublic ) )
		{
			Console.WriteLine( $"{c.FullName}" );
		}

	}

	[TestMethod]
	public void ChildClasses()
	{
		var data = GetSchema();

		var house = data.Types.FirstOrDefault( x => x.Name == "House" );
		Assert.IsNotNull( house );
		Assert.AreEqual( "Town.House", house.FullName );
		Assert.AreEqual( "House", house.Name );
	}

	[TestMethod]
	public void Documentation()
	{
		var data = GetSchema();

		var house = data.Types.FirstOrDefault( x => x.Name == "Springfield" );
		Assert.IsNotNull( house );
		Assert.IsNotNull( house.Documentation );
		Assert.IsNotNull( house.Documentation.Summary );
	}

	[TestMethod]
	public void DocumentationChildClass()
	{
		var data = GetSchema();

		var house = data.Types.FirstOrDefault( x => x.Name == "House" );
		Assert.IsNotNull( house );
		Assert.IsNotNull( house.Documentation );
		Assert.IsNotNull( house.Documentation.Summary );
	}

	[TestMethod]
	public void DocumentationExamples()
	{
		var data = GetSchema();
		var sp = data.Types.FirstOrDefault( x => x.Name == "Springfield" );
		var bart = sp.Properties.FirstOrDefault( x => x.Name == "Bart" );

		Assert.IsNotNull( bart );
		Assert.IsNotNull( bart.Documentation );
		Assert.IsNotNull( bart.Documentation.Examples );
		Assert.AreEqual( 1, bart.Documentation.Examples.Length );

		foreach ( var ex in bart.Documentation.Examples )
		{
			Console.WriteLine( ex );
		}
	}

	[TestMethod]
	public void Documentation_PlainText()
	{
		var data = GetSchema();

		var house = data.Types.FirstOrDefault( x => x.Name == "House" );
		Assert.IsNotNull( house );
		Assert.IsTrue( house.Documentation.Summary.Contains( "<a" ) );
		Assert.IsFalse( house.Documentation.SummaryPlainText.Contains( "<a" ) );

		Console.WriteLine( house.Documentation.Summary );

		Console.WriteLine( "\n\n" );

		Console.WriteLine( house.Documentation.SummaryPlainText );
	}

	/// <summary>
	/// SpringfieldExtensions.IsGoodSpringfield should be an extension method
	/// </summary>
	[TestMethod]
	public void Extensions()
	{
		var data = GetSchema();
		var sp = data.Types.FirstOrDefault( x => x.Name == "SpringfieldExtensions" );
		var extensionMethod = sp.Methods.FirstOrDefault( x => x.Name == "IsGoodSpringfield" );

		Assert.IsNotNull( extensionMethod );
		Assert.IsTrue( extensionMethod.IsExtension );
		Assert.IsTrue( sp.IsExtension );
	}

	/// <summary>
	/// Springfield.IsGoodSpringfield should be an extension method
	/// </summary>
	[TestMethod]
	public void TargetTypeHasExtensions()
	{
		var data = GetSchema();
		data.Rebuild();
		var sp = data.Types.FirstOrDefault( x => x.Name == "Springfield" );
		var extensionMethod = sp.Methods.FirstOrDefault( x => x.Name == "IsGoodSpringfield" );

		Assert.IsNotNull( extensionMethod );
		Assert.IsTrue( extensionMethod.IsExtension );
		Assert.IsTrue( !sp.IsExtension );
	}

	[TestMethod]
	public void DocumentationComplicatedType()
	{
		var data = GetSchema();
		var sp = data.Types.FirstOrDefault( x => x.Name == "GpuBuffer" );

		Assert.IsNotNull( sp );
		Assert.IsNotNull( sp.Documentation );
		Assert.IsNotNull( sp.Documentation.Examples );

		Assert.IsFalse( sp.Documentation.SummaryPlainText.Contains( "&lt;" ) ); // should have been parsed properly
		Assert.AreEqual( 2, sp.Documentation.Examples.Count() );
		Assert.AreEqual( 2, sp.Documentation.SeeAlso.Count() );
	}

	[TestMethod]
	public void FindsEnumsWhenStripping()
	{
		var data = GetSchema();
		data.StripNonPublic();

		var sp = data.Types.FirstOrDefault( x => x.FullName == "GpuBuffer.UsageFlags" );
		Assert.IsNotNull( sp );
	}

	[TestMethod]
	public void DocumentationComplicatedConstructor()
	{
		var data = GetSchema();
		var sp = data.Types.FirstOrDefault( x => x.Name == "GpuBuffer" );
		Assert.IsNotNull( sp );
		Assert.IsNotNull( sp.Constructors );
		Assert.AreEqual( 1, sp.Constructors.Count );

		var cs = sp.Constructors[0];

		Assert.IsNotNull( cs );
		Assert.IsNotNull( cs.Documentation );
		Assert.IsNotNull( cs.Documentation.Summary );
		Assert.IsNotNull( cs.Documentation.Params );

		Console.WriteLine( cs.Documentation.Params.First() );
		Assert.AreEqual( 2, cs.Documentation.Params.Count() );

		Assert.AreEqual( "GpuBuffer.UsageFlags", cs.Parameters[2].ParameterType );
	}

	/// <summary>
	/// Load without symbols
	/// </summary>
	[TestMethod]
	public void WithoutSymbols()
	{
		var data = GetSchema( false );
		var sp = data.Types.FirstOrDefault( x => x.Name == "SpringfieldExtensions" );
		var extensionMethod = sp.Methods.FirstOrDefault( x => x.Name == "IsGoodSpringfield" );

		Assert.IsNotNull( extensionMethod );
		Assert.IsTrue( extensionMethod.IsExtension );
		Assert.IsTrue( sp.IsExtension );
	}

	/// <summary>
	/// File sceoped classes should get ignored
	/// </summary>
	[TestMethod]
	public void FileTypeIgnored()
	{
		var data = GetSchema();
		data.StripNonPublic();

		var sp = data.Types.FirstOrDefault( x => x.FullName.EndsWith( "MyFileClass" ) );

		Assert.IsNull( sp );

		var spn = data.Types.FirstOrDefault( x => x.FullName.EndsWith( "MyFileClassNested" ) );

		Assert.IsNull( spn );
	}

	/// <summary>
	/// Compiler added [Obsolete] flags on ref structs should be removed
	/// </summary>
	[TestMethod]
	public void RemoveRefObsolete()
	{
		var s = GetSchema();

		var fr = s.Types.FirstOrDefault( x => x.FullName.EndsWith( "Frink" ) );

		Assert.IsNotNull( fr );
		Assert.AreEqual( "System.Runtime.CompilerServices.IsByRefLikeAttribute", fr.Attributes[0].FullName );
	}
}
