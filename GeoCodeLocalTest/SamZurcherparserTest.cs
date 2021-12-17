using GeoCodeLocal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeoCodeLocalTest
{
    [TestClass]
    public class SamZurcherparserTest
    {
        SamZurcherParser samZurcherParser = new SamZurcherParser();

        [TestMethod]
        public void IsValidTest()
        {
            Assert.IsTrue(samZurcherParser.IsValid("0;1;2;3;4"));
            Assert.IsFalse(samZurcherParser.IsValid("0;1;2;3"));
        }

        [TestMethod]
        public void ParseTest()
        {
            Address address = samZurcherParser.Parse("cantonShort;postalCode;city;streetName;streetNumber");
            Assert.IsNotNull(address.Id);
            Assert.AreEqual("streetName" + " " + "streetNumber", address.Street);
            Assert.AreEqual("city", address.City);
            Assert.AreEqual("postalCode", address.Postalcode);
        }
    }
}