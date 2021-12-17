using GeoCodeLocal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeoCodeLocalTest
{
    [TestClass]
    public class ZKPParserTest
    {
        ZKPExtractParser zKPExtractParser = new ZKPExtractParser();

        [TestMethod]
        public void IsValidTest()
        {
            Assert.IsTrue(zKPExtractParser.IsValid("0,1,2,3,4"));
            Assert.IsFalse(zKPExtractParser.IsValid("0,1,2,3"));
        }

        [TestMethod]
        public void ParseTest()
        {
            Address address = zKPExtractParser.Parse("id,streetname,number,postalcode,city");
            Assert.AreEqual("id", address.Id);
            Assert.AreEqual("streetname" + " " + "number", address.Street);
            Assert.AreEqual("city", address.City);
            Assert.AreEqual("postalcode", address.Postalcode);
        }
    }
}