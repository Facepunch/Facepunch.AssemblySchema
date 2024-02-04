
Takes dlls and xmls, creates a schema that can be json serialized and transformed.

```csharp
using var s = new Facepunch.AssemblySchema.Builder();

s.AddAssembly(pathtoassembly);
s.AddDocumentation(pathtoxml);

s.AddAssembly(pathtoassembly);
s.AddDocumentation(pathtoxml);

// build it
Facepunch.AssemblySchema.Schema schema = s.Build();

// get json
var json = System.Text.Json.JsonSerializer.Serialize(schema);

// build internal connection data (should be done after deserialize)
schema.Rebuild();

// print all types
foreach (var type in schema.Types)
{
	Console.WriteLine(type.FullName);
}

var t = data.Types.FirstOrDefault(x => x.Name == "BaseRifle");

// print everywhere this type is used in public
foreach (var member in t.GetUsage().Where(x => x.IsPublic))
{
	Console.WriteLine(member.FullName);
}
```
