using NUnit.Framework;
using System;
using System.IO;
using Terradue.OpenSearch.Result;
using System.Xml;
using System.Linq;
using Terradue.OpenSearch.RdfEO.Result;
using Terradue.Metadata.EarthObservation.Ogc.Sar;
using Terradue.Metadata.EarthObservation;

namespace Terradue.OpenSearch.RdfEO.Test {
    [TestFixture()]
    public class DeserializationTest {

        [Test()]
        public void Cci() {

            FileStream cci = new FileStream("../samples/cci.rdf", FileMode.Open);

            RdfXmlDocument rdfDoc;

            XmlReader reader = XmlReader.Create(cci);

            rdfDoc = RdfXmlDocument.Load(reader);

            Assert.AreEqual("SAR_SLC_19960228_145405_19960228_145452_125_DESCENDING.slc", rdfDoc.Items.First().Identifier);

        }

        [Test()]
        public void TestAsar() {

            FileStream cci = new FileStream("../samples/asa_ims_1p.rdf", FileMode.Open);

            RdfXmlDocument rdfDoc;

            XmlReader reader = XmlReader.Create(cci);

            rdfDoc = RdfXmlDocument.Load(reader);

            SarEarthObservationType saEO = rdfDoc.Datasets[0].ElementExtensions.ReadElementExtensions<SarEarthObservationType>("EarthObservation", MetadataHelpers.SAR, MetadataHelpers.SarSerializer)[0];

            Assert.AreEqual("IS6", saEO.SarEarthObservationEquipment.SarEarthObservationEquipment.sensor.Sensor.swathIdentifier.Text[0]);

        }

        [Test()]
        public void TestAlosPsr() {

            FileStream cci = new FileStream("../samples/alos_psr.rdf", FileMode.Open);

            RdfXmlDocument rdfDoc;

            XmlReader reader = XmlReader.Create(cci);

            rdfDoc = RdfXmlDocument.Load(reader);

            SarEarthObservationType saEO = rdfDoc.Datasets[0].ElementExtensions.ReadElementExtensions<SarEarthObservationType>("EarthObservation", MetadataHelpers.SAR, MetadataHelpers.SarSerializer)[0];

            Assert.AreEqual("139", saEO.SarEarthObservationEquipment.SarEarthObservationEquipment.SarAcquisitionParameters.SarAcquisition.wrsLongitudeGrid.Value);

            Assert.AreEqual(50962910768, rdfDoc.Datasets[0].Links.First(l => l.RelationshipType == "enclosure").Length);

        }

        [Test()]
        public void TestASA_IM__0P() {

            FileStream cci = new FileStream("../samples/asa_im__0p.rdf", FileMode.Open);

            RdfXmlDocument rdfDoc;

            XmlReader reader = XmlReader.Create(cci);

            rdfDoc = RdfXmlDocument.Load(reader);

            SarEarthObservationType saEO = rdfDoc.Datasets[0].ElementExtensions.ReadElementExtensions<SarEarthObservationType>("EarthObservation", MetadataHelpers.SAR, MetadataHelpers.SarSerializer)[0];

            Assert.AreEqual("257", saEO.SarEarthObservationEquipment.SarEarthObservationEquipment.SarAcquisitionParameters.SarAcquisition.wrsLongitudeGrid.Value);

            Assert.AreEqual(193003137, rdfDoc.Datasets[0].Links.First(l => l.RelationshipType == "enclosure").Length);

            rdfDoc.SerializeToString();

        }
    }
}

