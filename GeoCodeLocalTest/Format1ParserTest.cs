using GeoCodeLocal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeoCodeLocalTest
{
    [TestClass]
    public class Format1ParserTest
    {
        Format1Parser format1Parser = new Format1Parser();

        [TestMethod]
        public void IsValidTest()
        {
            Assert.IsTrue(format1Parser.IsValid("0,1,2,3,4"));
            Assert.IsFalse(format1Parser.IsValid("0,1,2,3"));
        }

        [TestMethod]
        public void ParseTest()
        {
            Address address = format1Parser.Parse("id,streetname,number,postalcode,city");
            Assert.AreEqual("id", address.Id);
            Assert.AreEqual("streetname" + " " + "number", address.Street);
            Assert.AreEqual("city", address.City);
            Assert.AreEqual("postalcode", address.Postalcode);
        }
    }
}