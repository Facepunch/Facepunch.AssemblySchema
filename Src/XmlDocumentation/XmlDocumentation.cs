using System.Net;
using System.Text.RegularExpressions;
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

			// Some XML doc generators (like DocFX or Sandcastle) use '.' for nested types instead of '+',
			// while the C# compiler (csc /doc) uses '+'. We normalize to '.' here so both formats match.

			doc.Entries[d.Name.Replace( '+', '.' )] = d;
		}

		return doc;

	}

	public static string XmlToPlainText( string xml )
	{
		if ( xml is null )
			return default;

		// Remove HTML tags
		xml = Regex.Replace( xml, "<a href=\"[^\"]*\">(.*?)</a>", "$1", RegexOptions.IgnoreCase );

		// Replace <see cref="..."/> with its cref value
		xml = Regex.Replace( xml, "<see cref=\"([A-Z]\\:)?([^\"]*)\"\\s*/>", "$2", RegexOptions.IgnoreCase );

		// decode entities
		xml = WebUtility.HtmlDecode( xml );

		return xml;
	}
}
