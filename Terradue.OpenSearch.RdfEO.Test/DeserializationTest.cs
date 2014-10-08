using NUnit.Framework;
using System;
using System.IO;
using Terradue.OpenSearch.Result;
using System.Xml;
using System.Linq;

namespace Terradue.OpenSearch.RdfEO.Test {
    [TestFixture()]
    public class DeserializationTest {

        [Test()]
        public void Cci() {

            FileStream cci = new FileStream("../samples/cci.rdf", FileMode.Open);

            RdfXmlDocument rdfDoc;

            XmlReader reader = XmlReader.Create(cci);

            rdfDoc = RdfXmlDocument.Load(reader);

            Assert.AreEqual("Terradue CAS version 0.0.0.0", rdfDoc.ElementExtensions.ReadElementExtensions<string>("creator", "http://purl.org/dc/elements/1.1/")[0]);

            Assert.AreEqual("SAR_SLC_19960228_145405_19960228_145452_125_DESCENDING.slc", rdfDoc.Items.First().Identifier);

        }
    }
}

