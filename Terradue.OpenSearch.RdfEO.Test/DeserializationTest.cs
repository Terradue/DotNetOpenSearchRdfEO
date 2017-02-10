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
    public class DeserializationTest {

//        [Test()]
//        public void Cci() {
//
//            FileStream cci = new FileStream("../samples/cci.rdf", FileMode.Open);
//
//            RdfXmlDocument rdfDoc;
//
//            XmlReader reader = XmlReader.Create(cci);
//
//            rdfDoc = RdfXmlDocument.Load(reader);
//
//            Assert.AreEqual("SAR_SLC_19960228_145405_19960228_145452_125_DESCENDING.slc", rdfDoc.Items.First().Identifier);
//
//            cci.Close();
//
//        }

        [Test()]
        public void TestAsar() {

            FileStream cci = new FileStream("../samples/asa_ims_1p.rdf", FileMode.Open);

            RdfXmlDocument rdfDoc;

            XmlReader reader = XmlReader.Create(cci);

            rdfDoc = RdfXmlDocument.Load(reader);

            Terradue.ServiceModel.Ogc.Sar21.SarEarthObservationType saEO = rdfDoc.Datasets[0].ElementExtensions.ReadElementExtensions<Terradue.ServiceModel.Ogc.Sar21.SarEarthObservationType>("EarthObservation", OgcHelpers.SAR21, OgcHelpers.Sar21Serializer)[0];

            Assert.AreEqual("IS6", saEO.procedure.Eop21EarthObservationEquipment.sensor.Sensor.swathIdentifier.Text);

            Assert.AreEqual("IMS", saEO.EopMetaDataProperty.EarthObservationMetaData.productType);


        }

        [Test()]
        public void TestAlosPsr() {

            FileStream cci = new FileStream("../samples/alos_psr.rdf", FileMode.Open);

            RdfXmlDocument rdfDoc;

            XmlReader reader = XmlReader.Create(cci);

            rdfDoc = RdfXmlDocument.Load(reader);

            Terradue.ServiceModel.Ogc.Sar21.SarEarthObservationType saEO = rdfDoc.Datasets[0].ElementExtensions.ReadElementExtensions<Terradue.ServiceModel.Ogc.Sar21.SarEarthObservationType>("EarthObservation", OgcHelpers.SAR21, OgcHelpers.Sar21Serializer)[0];

            Assert.AreEqual("139", saEO.procedure.Eop21EarthObservationEquipment.acquisitionParameters.SarAcquisition.wrsLongitudeGrid.Value);

            Assert.AreEqual(50962910768, rdfDoc.Datasets[0].Links.First(l => l.RelationshipType == "enclosure").Length);

        }

        [Test()]
        public void TestASA_IM__0P() {

            FileStream cci = new FileStream("../samples/asa_im__0p.rdf", FileMode.Open);

            RdfXmlDocument rdfDoc;

            XmlReader reader = XmlReader.Create(cci);

            rdfDoc = RdfXmlDocument.Load(reader);

            Terradue.ServiceModel.Ogc.Sar21.SarEarthObservationType saEO = rdfDoc.Datasets[0].ElementExtensions.ReadElementExtensions<Terradue.ServiceModel.Ogc.Sar21.SarEarthObservationType>("EarthObservation", OgcHelpers.SAR21, OgcHelpers.Sar21Serializer)[0];

            Assert.AreEqual("257", saEO.procedure.Eop21EarthObservationEquipment.acquisitionParameters.SarAcquisition.wrsLongitudeGrid.Value);

            Assert.AreEqual(193003137, rdfDoc.Datasets[0].Links.First(l => l.RelationshipType == "enclosure").Length);

            Assert.AreEqual("2012-04-07T18:20:55.040Z", rdfDoc.Datasets[0].ElementExtensions.FirstOrDefault(e => e.OuterName == "dtend").GetObject<string>());

            rdfDoc.SerializeToString();



        }

        [Test()]
        public void TestER02_SAR_IM__0P() {

            FileStream cci = new FileStream("../samples/ER02_SAR_IM__0P.rdf", FileMode.Open);

            RdfXmlDocument rdfDoc;

            XmlReader reader = XmlReader.Create(cci);

            rdfDoc = RdfXmlDocument.Load(reader);

            Terradue.ServiceModel.Ogc.Sar21.SarEarthObservationType saEO = rdfDoc.Datasets[0].ElementExtensions.ReadElementExtensions<Terradue.ServiceModel.Ogc.Sar21.SarEarthObservationType>("EarthObservation", OgcHelpers.SAR21, OgcHelpers.Sar21Serializer)[0];

            Assert.AreEqual("336", saEO.procedure.Eop21EarthObservationEquipment.acquisitionParameters.SarAcquisition.wrsLongitudeGrid.Value);

            Assert.AreEqual(0, rdfDoc.Datasets[0].Links.First(l => l.RelationshipType == "enclosure").Length);



            rdfDoc.SerializeToString();



        }


        [Test()]
        public void TestRDF2() {

            FileStream cci = new FileStream("../samples/rdf2.xml", FileMode.Open);

            RdfXmlDocument rdfDoc;

            XmlReader reader = XmlReader.Create(cci);

            rdfDoc = RdfXmlDocument.Load(reader);

            Terradue.ServiceModel.Ogc.Eop20.EarthObservationType saEO = rdfDoc.Datasets[0].ElementExtensions.ReadElementExtensions<Terradue.ServiceModel.Ogc.Eop20.EarthObservationType>("EarthObservation", OgcHelpers.EOP20, OgcHelpers.Eop20Serializer)[0];

            rdfDoc.SerializeToString();



        }

        [Test()]
        public void TestRDF3() {

            FileStream cci = new FileStream("../samples/rdf3.xml", FileMode.Open);

            RdfXmlDocument rdfDoc;

            XmlReader reader = XmlReader.Create(cci);

            rdfDoc = RdfXmlDocument.Load(reader);

            Terradue.ServiceModel.Ogc.Sar21.SarEarthObservationType saEO = rdfDoc.Datasets[0].ElementExtensions.ReadElementExtensions<Terradue.ServiceModel.Ogc.Sar21.SarEarthObservationType>("EarthObservation", OgcHelpers.SAR21, OgcHelpers.Sar21Serializer)[0];


            rdfDoc.SerializeToString();



        }   

        [Test()]
        public void TestS1A() {

            FileStream cci = new FileStream("../samples/s1a.rdf", FileMode.Open);

            RdfXmlDocument rdfDoc;

            XmlReader reader = XmlReader.Create(cci);

            rdfDoc = RdfXmlDocument.Load(reader);

            Terradue.ServiceModel.Ogc.Sar21.SarEarthObservationType saEO = rdfDoc.Datasets[0].ElementExtensions.ReadElementExtensions<Terradue.ServiceModel.Ogc.Sar21.SarEarthObservationType>("EarthObservation", OgcHelpers.SAR21, OgcHelpers.Sar21Serializer)[0];


            rdfDoc.SerializeToString();



        }  

        [Test()]
        public void FromDORPOR()
        {

            FileStream cci = new FileStream("../samples/DOR_POR_AXVF-P20110514_021400_20110511_215526_20110513_002326.rdf", FileMode.Open);

            RdfXmlDocument rdfDoc;

            XmlReader reader = XmlReader.Create(cci);

            rdfDoc = RdfXmlDocument.Load(reader);

            Terradue.ServiceModel.Ogc.Eop21.EarthObservationType saEO = rdfDoc.Datasets[0].ElementExtensions.ReadElementExtensions<Terradue.ServiceModel.Ogc.Eop21.EarthObservationType>("EarthObservation", OgcHelpers.EOP21, OgcHelpers.Eop21Serializer)[0];


            rdfDoc.SerializeToString();



        }
    }
}

