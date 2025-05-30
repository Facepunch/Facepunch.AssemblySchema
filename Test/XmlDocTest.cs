using Facepunch.AssemblySchema;

namespace Test;

[TestClass]
public class XmlDocTest
{
	[TestMethod]
	public void ToPlainText()
	{
		var pt = XmlDocumentation.XmlToPlainText( "House is full of <a href=\"http://ikea.com\">nice house furniture</a>. A house is in a <see cref=\"Town\"/>. You can change the <see cref=\"House.HouseNumber\"/> to change the house's house number." );
		Console.WriteLine( pt );
	}

	[TestMethod]
	public void ToPlainTextMultipleAnchors()
	{
		var pt = XmlDocumentation.XmlToPlainText( "House is full of <a href=\"http://ikea.com\">nice house furniture</a>. <a href=\"http://ikea.com\">Nice house furniture</a>." );
		Console.WriteLine( pt );
	}
}
