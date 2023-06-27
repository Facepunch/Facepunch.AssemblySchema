using System.Xml;

namespace Facepunch.AssemblySchema;

public class XmlDocumentation
{
	public Dictionary<string, Documentation> Entries = new Dictionary<string, Documentation>();

	internal static XmlDocumentation ReadFromString( byte[] xmlSource )
	{
		using var stream = new MemoryStream( xmlSource );

		var doc = new XmlDocumentation();

		var xml = new XmlDocument();
		xml.Load( stream );

		foreach ( XmlNode m in xml.SelectSingleNode( ".//members" ) )
		{
			var d = Documentation.ParseFromNode( m );
			if ( d is null ) continue;

			doc.Entries[d.Name] = d;
		}

		return doc;

	}
}
