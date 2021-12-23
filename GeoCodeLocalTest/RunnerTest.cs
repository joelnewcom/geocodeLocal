using System;
using GeoCodeLocal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GeoCodeLocalTest;

[TestClass]
public class RunnerTest
{
    Runner runner = new Runner("", Mode.reset, new Mock<ILineParser>().Object);

    [TestMethod]
    public void CreateQueryParamsTest()
    {
        Assert.AreEqual("http://localhost:8080/search?" + Runner.EXCLUDES + "&street=street&city=city&postalcode=postalcode", runner.CreateQueryParams("street", "city", "postalcode"));
        Assert.AreEqual("http://localhost:8080/search?" + Runner.EXCLUDES + "&city=city&postalcode=postalcode", runner.CreateQueryParams("", "city", "postalcode"));
        Assert.AreEqual("http://localhost:8080/search?" + Runner.EXCLUDES + "&postalcode=postalcode", runner.CreateQueryParams("", "", "postalcode"));
        Assert.AreEqual("http://localhost:8080/search?" + Runner.EXCLUDES + "&street=street&postalcode=postalcode", runner.CreateQueryParams("street", "NULL", "postalcode"));
        Assert.AreEqual("http://localhost:8080/search?" + Runner.EXCLUDES + "&street=street&postalcode=postalcode", runner.CreateQueryParams("street", null, "postalcode"));
        Assert.AreEqual("http://localhost:8080/search?" + Runner.EXCLUDES + "&street=street&postalcode=postalcode", runner.CreateQueryParams("street", "", "postalcode"));
    }
}