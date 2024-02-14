using Facepunch.AssemblySchema;
using System.IO.Compression;
using System.Text.Json;

namespace Test;

[TestClass]
public class ProcessorTest
{
	public Schema GetSchema()
	{
		using var s = new Facepunch.AssemblySchema.Builder();
		s.AddAssembly(System.IO.File.ReadAllBytes(GetType().Assembly.Location));
		s.AddDocumentation(System.IO.File.ReadAllBytes(GetType().Assembly.Location.Replace(".dll", ".xml")));

		return s.Build();
	}

	[TestMethod]
	public void BasicTests()
	{
		var data = GetSchema();

		Assert.IsNotNull(data);
		Assert.AreNotEqual(0, data.Types.Count);

		foreach (var type in data.Types)
		{
			Console.WriteLine($"{type.FullName}");
		}

		var town = data.Types.FirstOrDefault(x => x.Name == "Town");
		Assert.IsNotNull(town);
		Assert.IsNotNull(town.Documentation);
		Assert.IsNotNull(town.Documentation.Summary);

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

		var tagAttr = data.Types.FirstOrDefault(x => x.Name == "TagAttribute");
		Assert.IsNotNull(tagAttr);
		Assert.IsTrue(tagAttr.IsAttribute);

		Assert.IsNull(sp.GetBaseType());

		// Rebuild transient data
		data.Rebuild();

		Assert.AreEqual(sp.BaseType, "Town");
		Assert.IsNotNull(sp.GetBaseType());
		Assert.IsNotNull(sp.GetBaseType());

		Assert.IsNotNull(town.GetDerivedTypes());
		Assert.IsTrue(town.GetDerivedTypes().Contains(sp));

		Assert.AreNotEqual(0, town.GetUsage().Count()); // town is used as an argument in a parameter

	}

	[TestMethod]
	public void JsonEncodable()
	{
		var data = GetSchema();

		var json = System.Text.Json.JsonSerializer.Serialize(data, new JsonSerializerOptions
		{
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
			WriteIndented = true
		});

		Console.WriteLine(json);

		Assert.IsNotNull(json);
		Assert.AreNotEqual(0, json.Length);
	}

	[TestMethod]
	public async Task FromUrlZip()
	{
		var url = "https://cdn.facepunch.com/sbox/releases/2024-02-03-12-14-30.zip";

		var httpClient = new HttpClient();
		var zipBytes = await httpClient.GetByteArrayAsync(url);

		using var stream = new MemoryStream(zipBytes);
		using var archive = new ZipArchive(stream);

		Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();

		foreach (var entry in archive.Entries)
		{
			using (var ms = new MemoryStream())
			{
				entry.Open().CopyTo(ms);
				var bytes = ms.ToArray();

				if (entry.FullName.EndsWith(".dll")) files[entry.FullName] = bytes;
				if (entry.FullName.EndsWith(".xml")) files[entry.FullName] = bytes;
			}
		}

		using var processor = new Facepunch.AssemblySchema.Builder();

		foreach (var file in files.Where(x => x.Key.EndsWith(".xml")))
		{
			processor.AddDocumentation(file.Value);
		}

		foreach (var file in files.Where(x => x.Key.EndsWith(".dll")))
		{
			processor.AddAssembly(file.Value);
		}

		var result = processor.Build();

		result.Rebuild();

		// can we get a list of components
		var component = result.Types.FirstOrDefault(x => x.Name == "Component");
		Assert.IsNotNull(component);

		foreach (var c in component.GetDerivedTypes())
		{
			Console.WriteLine($"{c.FullName}");
		}

		var physicsBody = result.Types.FirstOrDefault(x => x.Name == "PhysicsBody");
		Assert.IsNotNull(physicsBody);

		foreach (var c in physicsBody.GetUsage().Where(x => x.IsPublic))
		{
			Console.WriteLine($"{c.FullName}");
		}

	}

	[TestMethod]
	public void ChildClasses()
	{
		var data = GetSchema();

		var house = data.Types.FirstOrDefault(x => x.Name == "House");
		Assert.IsNotNull(house);
		Assert.AreEqual("Town.House", house.FullName);
		Assert.AreEqual("House", house.Name);
	}
}