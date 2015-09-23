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

            Assert.AreEqual("IMS", saEO.metaDataProperty1.EarthObservationMetaData.productType);


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

        [Test()]
        public void TestER02_SAR_IM__0P() {

            FileStream cci = new FileStream("../samples/ER02_SAR_IM__0P.rdf", FileMode.Open);

            RdfXmlDocument rdfDoc;

            XmlReader reader = XmlReader.Create(cci);

            rdfDoc = RdfXmlDocument.Load(reader);

            SarEarthObservationType saEO = rdfDoc.Datasets[0].ElementExtensions.ReadElementExtensions<SarEarthObservationType>("EarthObservation", MetadataHelpers.SAR, MetadataHelpers.SarSerializer)[0];

            Assert.AreEqual("336", saEO.SarEarthObservationEquipment.SarEarthObservationEquipment.SarAcquisitionParameters.SarAcquisition.wrsLongitudeGrid.Value);

            Assert.AreEqual(0, rdfDoc.Datasets[0].Links.First(l => l.RelationshipType == "enclosure").Length);



            rdfDoc.SerializeToString();



        }


        [Test()]
        public void TestRDF2() {

            FileStream cci = new FileStream("../samples/rdf2.xml", FileMode.Open);

            RdfXmlDocument rdfDoc;

            XmlReader reader = XmlReader.Create(cci);

            rdfDoc = RdfXmlDocument.Load(reader);

            Terradue.Metadata.EarthObservation.Ogc.Eop20.EarthObservationType saEO = rdfDoc.Datasets[0].ElementExtensions.ReadElementExtensions<Terradue.Metadata.EarthObservation.Ogc.Eop20.EarthObservationType>("EarthObservation", MetadataHelpers.EOP20, MetadataHelpers.EopSerializer20)[0];

            rdfDoc.SerializeToString();



        }

        [Test()]
        public void TestRDF3() {

            FileStream cci = new FileStream("../samples/rdf3.xml", FileMode.Open);

            RdfXmlDocument rdfDoc;

            XmlReader reader = XmlReader.Create(cci);

            rdfDoc = RdfXmlDocument.Load(reader);

            Terradue.Metadata.EarthObservation.Ogc.Sar.SarEarthObservationType saEO = rdfDoc.Datasets[0].ElementExtensions.ReadElementExtensions<Terradue.Metadata.EarthObservation.Ogc.Sar.SarEarthObservationType>("EarthObservation", MetadataHelpers.SAR, MetadataHelpers.SarSerializer)[0];


            rdfDoc.SerializeToString();



        }   
    }
}

