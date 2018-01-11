using NUnit.Framework;
using System;
using System.IO;
using Terradue.OpenSearch.Result;
using System.Xml;
using System.Linq;
using Terradue.OpenSearch.RdfEO.Result;
using Terradue.Metadata.EarthObservation;
using Terradue.ServiceModel.Ogc;

namespace Terradue.OpenSearch.RdfEO.Test {
    [TestFixture()]
    public class TransformationTest {


        [Test()]
        public void FromEmptyAtom()
        {

            FileStream cci = new FileStream("../samples/empty.atom", FileMode.Open);

            var feed = AtomFeed.Load(XmlReader.Create(cci));

            var rdfDoc = new RdfXmlDocument(feed);

            rdfDoc.SerializeToString();



        }
    }
}

