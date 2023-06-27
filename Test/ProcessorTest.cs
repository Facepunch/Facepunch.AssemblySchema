using Facepunch.AssemblySchema;
using System.Text.Json;

namespace Test;

[TestClass]
public class ProcessorTest
{
	public Schema GetSchema()
	{
		var s = new Facepunch.AssemblySchema.Builder();
		s.AddAssembly(System.IO.File.ReadAllBytes( GetType().Assembly.Location ));
		s.AddDocumentation(System.IO.File.ReadAllBytes(GetType().Assembly.Location.Replace(".dll", ".xml")));

		return s.Build();
	}

	[TestMethod]
	public void BasicTests()
	{
		var data = GetSchema();

		Assert.IsNotNull(data);
		Assert.AreNotEqual( 0, data.Types.Count );

		foreach( var type in data.Types )
		{
			Console.WriteLine($"{type.FullName}");
		}

		var sp = data.Types.FirstOrDefault(x => x.Name == "Springfield");
		Assert.IsNotNull(sp);
		Assert.IsNotNull(sp.Documentation);
		Assert.IsNotNull(sp.Documentation.Summary);

		var maggie = sp.Fields.FirstOrDefault(x => x.Name == "Maggie");
		Assert.IsNotNull(maggie);
		Assert.IsNotNull(maggie.Documentation);
		Assert.IsNotNull(maggie.Documentation.Summary);

		var lisa = sp.Properties.FirstOrDefault(x => x.Name == "Lisa");
		Assert.IsNotNull(lisa);
		Assert.IsNotNull(lisa.Documentation);
		Assert.IsNotNull(lisa.Documentation.Summary);

		var bart = sp.Properties.FirstOrDefault(x => x.Name == "Bart");
		Assert.IsNotNull(bart);
		Assert.IsNotNull(bart.Documentation);
		Assert.IsNotNull(bart.Documentation.Summary);
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

		Console.WriteLine(json);

		Assert.IsNotNull(json);
		Assert.AreNotEqual(0, json.Length);
	}
}