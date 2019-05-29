using NUnit.Framework;
using Terradue.OpenSearch.Engine;

namespace Terradue.OpenSearch.RdfEO.Test {

    [TestFixture]
    public class GenericTest {

        [Ignore]
        public void GenericOpenSearchableTest() {

            OpenSearchEngine ose = new OpenSearchEngine();

            ose.LoadPlugins();
            OpenSearchUrl url = new OpenSearchUrl("http://catalogue.terradue.int/catalogue/search/MER_FRS_1P/rdf?startIndex=0&q=MER_FRS_1P&start=1992-01-01&stop=2014-10-24&bbox=-72,47,-57,58");
            OpenSearchableFactorySettings factory = new OpenSearchableFactorySettings(ose);
                     
            IOpenSearchable entity = new GenericOpenSearchable(url,factory);
            var osr = ose.Query(entity, new System.Collections.Specialized.NameValueCollection(), "rdf");

            //Assert.AreEqual(15, osr.TotalResults);
            Assert.That(osr.Links.Count > 0);

        }
    }
}