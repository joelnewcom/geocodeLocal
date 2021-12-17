using System;
using GeoCodeLocal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeoCodeLocalTest;

[TestClass]
public class RunnerTest
{
    Runner runner = new Runner("", "");

    [TestMethod]
    public void CreateQueryParamsTest()
    {
        Assert.AreEqual("http://localhost:8080/search?street=street&city=city&postalcode=postalcode", runner.CreateQueryParams("street", "city", "postalcode"));
        Assert.AreEqual("http://localhost:8080/search?city=city&postalcode=postalcode", runner.CreateQueryParams("", "city", "postalcode"));
        Assert.AreEqual("http://localhost:8080/search?postalcode=postalcode", runner.CreateQueryParams("", "", "postalcode"));
        Assert.AreEqual("http://localhost:8080/search?street=street&postalcode=postalcode", runner.CreateQueryParams("street", "NULL", "postalcode"));
        Assert.AreEqual("http://localhost:8080/search?street=street&postalcode=postalcode", runner.CreateQueryParams("street", null, "postalcode"));
        Assert.AreEqual("http://localhost:8080/search?street=street&postalcode=postalcode", runner.CreateQueryParams("street", "", "postalcode"));
    }
}