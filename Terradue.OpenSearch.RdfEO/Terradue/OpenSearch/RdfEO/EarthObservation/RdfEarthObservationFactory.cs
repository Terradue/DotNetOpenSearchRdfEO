using System;
using Terradue.Metadata.EarthObservation.Ogc.Eop;
using Terradue.ServiceModel.Syndication;
using System.Xml.Linq;
using Terradue.Metadata.EarthObservation.Ogc.Sar;
using Terradue.OpenSearch.Result;
using System.Xml;
using Terradue.OpenSearch.RdfEO.Result;
using Terradue.Metadata.EarthObservation.Ogc.Om;
using System.Collections.Generic;
using Terradue.GeoJson.Geometry;
using System.Xml.Serialization;
using Terradue.Metadata.EarthObservation.Ogc.Opt;
using Terradue.Metadata.EarthObservation.Ogc.Alt;
using Terradue.Metadata.EarthObservation;
using System.IO;

namespace Terradue.OpenSearch.RdfEO {
    public class RdfEarthObservationFactory {

        public static XmlReader GetXmlReaderEarthObservationTypeFromRdf(XElement rdf, XElement series){

            var eo = GetEarthObservationTypeFromRdf(rdf, series);

            var ser = MetadataHelpers.GetXmlSerializerFromType(eo.GetType());

            MemoryStream ms = new MemoryStream();

            ser.Serialize(ms, eo);

            ms.Seek(0, SeekOrigin.Begin);

            return XmlReader.Create(ms);

        }
        
        public static EarthObservationType GetEarthObservationTypeFromRdf(XElement rdf, XElement series) {
            
            EarthObservationType eo = new EarthObservationType();

            var sensor = series.Element(XName.Get("sensor", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));

            if (sensor != null) {

                switch (sensor.Value) {

                    case "AATSR":
                    case "MERIS":
                    case "OLI_TIRS":
                    case "AVHRR":
                    case "SEVIRI":
                    case "PRISM":
                    case "SPOT":
                    case "AVNIR-2":
                        return GetOptEarthObservationTypeFromRdf(rdf, series);

                    case "ASAR":
                    case "PALSAR2":
                    case "PALSAR":
                    case "SAR":
                    case "TSX-SAR":
                    case "MIRAS":
                    case "AMI":
                    case "ASPS":
                        return GetSarEarthObservationTypeFromRdf(rdf, series);
                    case "SIRAL":
                    case "RA2":
                    case "DORIS":
                        return GetAltEarthObservationTypeFromRdf(rdf, series);
                    
                    
                    case "MIPAS":
                    case "SCIAMACHY":
                    case "GOMOS":
                    case "GOME":
                        throw new NotImplementedException();
                        //return GetAtmEarthObservationTypeFromRdf(rdf);
                    
                    default:
                        return GetGenericEarthObservationTypeFromRdf(rdf, series);

                }
                    
            }

           
            return eo;
        }

        public static EarthObservationType GetGenericEarthObservationTypeFromRdf(XElement rdf, XElement series) {

            EarthObservationType eo = new EarthObservationType();

            // Equipment
            eo.EopProcedure = GetEarthObservationEquipmentPropertyTypeFromRdf(rdf, series);

            // Phenomenon
            eo.phenomenonTime = GetEOPhenomenonTypeFromRdf(rdf, series);

            // Metadata
            eo.metaDataProperty1 = GetMetadataTypeFromRdf(rdf, series);

            // Result
            eo.EopResult = GetEopResultTypeFromRdf(rdf, series);

            // Footprint
            eo.featureOfInterest = GetFeatureOfInterest(rdf, series);

            return eo;
        }

        public static EarthObservationType GetAltEarthObservationTypeFromRdf(XElement rdf, XElement series) {

            AltEarthObservationType altEO = new AltEarthObservationType();

            // Equipment
            altEO.EopProcedure = GetEarthObservationEquipmentPropertyTypeFromRdf(rdf, series);
            altEO.EopProcedure.EarthObservationEquipment.sensor.Sensor.sensorType = "RADAR";

            // Phenomenon
            altEO.phenomenonTime = GetEOPhenomenonTypeFromRdf(rdf, series);

            // Metadata
            altEO.metaDataProperty1 = GetMetadataTypeFromRdf(rdf, series);

            // Result
            altEO.EopResult = GetEopResultTypeFromRdf(rdf, series);

            // Footprint
            altEO.featureOfInterest = GetFeatureOfInterest(rdf, series);

            return altEO;
        }

        public static EarthObservationType GetOptEarthObservationTypeFromRdf(XElement rdf, XElement series) {

            OptEarthObservationType optEO = new OptEarthObservationType();

            // Equipment
            optEO.EopProcedure = GetEarthObservationEquipmentPropertyTypeFromRdf(rdf, series);
            optEO.EopProcedure.EarthObservationEquipment.sensor.Sensor.sensorType = "OPTICAL";

            // Phenomenon
            optEO.phenomenonTime = GetEOPhenomenonTypeFromRdf(rdf, series);

            // Metadata
            optEO.metaDataProperty1 = GetMetadataTypeFromRdf(rdf, series);

            // Result
            optEO.EopResult = GetEopResultTypeFromRdf(rdf, series);

            // Footprint
            optEO.featureOfInterest = GetFeatureOfInterest(rdf, series);

            return optEO;
        }

        public static EarthObservationType GetSarEarthObservationTypeFromRdf(XElement rdf, XElement series) {

            SarEarthObservationType sarEO = new SarEarthObservationType();

            // Equipment
            sarEO.SarEarthObservationEquipment = GetSarEarthObservationEquipmentPropertyTypeFromRdf(rdf, series);

            // Phenomenon
            sarEO.phenomenonTime = GetEOPhenomenonTypeFromRdf(rdf, series);

            // Metadata
            sarEO.metaDataProperty1 = GetMetadataTypeFromRdf(rdf, series);

            // Result
            sarEO.EopResult = GetEopResultTypeFromRdf(rdf, series);

            // Footprint
            sarEO.featureOfInterest = GetFeatureOfInterest(rdf, series);

            return sarEO;
        }

        public static EarthObservationEquipmentPropertyType GetEarthObservationEquipmentPropertyTypeFromRdf(XElement rdf, XElement series) {

            var platform = series.Element(XName.Get("platform", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
            var subject = series.Element(XName.Get("subject", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
            var sensor = series.Element(XName.Get("sensor", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));

            EarthObservationEquipmentPropertyType earthObservationEquipment = new SarEarthObservationEquipmentPropertyType();
            earthObservationEquipment.EarthObservationEquipment = new EarthObservationEquipmentType();

            earthObservationEquipment.EarthObservationEquipment.platform = GetPlatformFromRdf(rdf, series);

            earthObservationEquipment.EarthObservationEquipment.instrument = GetInstrumentFromRdf(rdf, series);

            earthObservationEquipment.EarthObservationEquipment.sensor = GetSensorFromRdf(rdf, series);

            earthObservationEquipment.EarthObservationEquipment.acquisitionParameters = new AcquisitionPropertyType();
            earthObservationEquipment.EarthObservationEquipment.acquisitionParameters.Acquisition = new AcquisitionType();

            AcquisitionType acq = GetAcquisitionFromRdf(rdf, series);

            return earthObservationEquipment;
        }

        public static SarEarthObservationEquipmentPropertyType GetSarEarthObservationEquipmentPropertyTypeFromRdf(XElement rdf, XElement series) {

            var platform = series.Element(XName.Get("platform", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
            var subject = series.Element(XName.Get("subject", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
            var sensor = series.Element(XName.Get("sensor", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));

            SarEarthObservationEquipmentPropertyType sarEarthObservationEquipment = new SarEarthObservationEquipmentPropertyType();
            sarEarthObservationEquipment.SarEarthObservationEquipment = new SarEarthObservationEquipmentType();

            sarEarthObservationEquipment.SarEarthObservationEquipment.platform = GetPlatformFromRdf(rdf, series);

            sarEarthObservationEquipment.SarEarthObservationEquipment.instrument = GetInstrumentFromRdf(rdf, series);

            sarEarthObservationEquipment.SarEarthObservationEquipment.sensor = GetSensorFromRdf(rdf, series);

            sarEarthObservationEquipment.SarEarthObservationEquipment.SarAcquisitionParameters = new SarAcquisitionPropertyType();
            sarEarthObservationEquipment.SarEarthObservationEquipment.SarAcquisitionParameters.SarAcquisition = new SarAcquisitionType();

            sarEarthObservationEquipment.SarEarthObservationEquipment.sensor.Sensor.sensorType = "RADAR";

            AcquisitionType acq = GetAcquisitionFromRdf(rdf, series);

            sarEarthObservationEquipment.SarEarthObservationEquipment.SarAcquisitionParameters.SarAcquisition.orbitNumber = acq.orbitNumber;
            sarEarthObservationEquipment.SarEarthObservationEquipment.SarAcquisitionParameters.SarAcquisition.wrsLongitudeGrid = acq.wrsLongitudeGrid;
            sarEarthObservationEquipment.SarEarthObservationEquipment.SarAcquisitionParameters.SarAcquisition.orbitDirection = acq.orbitDirection;

            var polc = rdf.Element(XName.Get("polarisationChannels", "http://earth.esa.int/sar"));

            if (polc != null )
                sarEarthObservationEquipment.SarEarthObservationEquipment.SarAcquisitionParameters.SarAcquisition.polarisationChannels = polc.Value;

            return sarEarthObservationEquipment;
        }


        public static PlatformPropertyType GetPlatformFromRdf(XElement rdf, XElement series) {


            var xplatform = series.Element(XName.Get("platform", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
            var subject = series.Element(XName.Get("subject", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));


            PlatformPropertyType platform = new PlatformPropertyType();
            platform.Platform = new PlatformType();

            var pf = "";
            if (xplatform != null)
                pf = xplatform.Value;
            else if (subject != null) {
                pf = subject.Value;
            }
            switch (pf) {

                case "ENVISAT":
                    platform.Platform.serialIdentifier = "2002-009A";
                    platform.Platform.shortName = "ENVISAT";
                    break;
                case "SENTINEL S1":
                    platform.Platform.serialIdentifier = "2014-016A";
                    platform.Platform.shortName = "ENVISAT";
                    break;
                case "SENTINEL S2":
                    platform.Platform.serialIdentifier = "2015-028A";
                    platform.Platform.shortName = "ENVISAT";
                    break;
                case "ERS1":
                case "ERS":
                    platform.Platform.serialIdentifier = "1991-050A";
                    platform.Platform.shortName = "ERS1";
                    break;
                case "ERS2":
                    platform.Platform.serialIdentifier = "1995-021A";
                    platform.Platform.shortName = "ERS2";
                    break;
                case "ALOS":
                    platform.Platform.serialIdentifier = "2006-002A";
                    platform.Platform.shortName = "ALOS";
                    break;
                case "ALOS2":
                    platform.Platform.serialIdentifier = "2014-029A";
                    platform.Platform.shortName = "ALOS2";
                    break;
                case "SMOS":
                    platform.Platform.serialIdentifier = "2009-059A";
                    platform.Platform.shortName = "SMOS";
                    break;
                case "METOP":
                    platform.Platform.serialIdentifier = "2012-049A";
                    platform.Platform.shortName = "METOP-B";
                    break;
                case "TERRA":
                    platform.Platform.serialIdentifier = "1999-068A";
                    platform.Platform.shortName = "Terra";
                    break;
                case "TERRASAR-X":
                    platform.Platform.serialIdentifier = "2007-026A";
                    platform.Platform.shortName = "Terra SAR-X";
                    break;
                case "AQUA":
                    platform.Platform.serialIdentifier = "2002-022A";
                    platform.Platform.shortName = "Aqua";
                    break;
                case "LANDSAT 8":
                    platform.Platform.serialIdentifier = "2013-008A";
                    platform.Platform.shortName = "Landsat 8";
                    break;
                case "SWARM":
                    platform.Platform.serialIdentifier = "2013-067A";
                    platform.Platform.shortName = "Swarm";
                    break;
                default :
                    platform.Platform.shortName = pf;
                    break;
            }

            return platform;
        }


        public static InstrumentPropertyType GetInstrumentFromRdf(XElement rdf, XElement series) {


            var sensor = series.Element(XName.Get("sensor", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));

            InstrumentPropertyType instrument = new InstrumentPropertyType();
            instrument.Instrument = new InstrumentType();
            instrument.Instrument.shortName = sensor.Value;

            return instrument;

        }

        public static SensorPropertyType GetSensorFromRdf(XElement rdf, XElement series) {

            var sensorx = series.Element(XName.Get("platform", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
            var seriesid = series.Element(XName.Get("identifier", "http://purl.org/dc/elements/1.1/"));
            var swathid = rdf.Element(XName.Get("swathIdentifier", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));

            SensorPropertyType sensor = new SensorPropertyType();
            sensor.Sensor = new SensorType();

            if (swathid != null) {
                sensor.Sensor.swathIdentifier = new Terradue.GeoJson.Gml.CodeListType();
                sensor.Sensor.swathIdentifier.Text = new string[]{ swathid.Value };
            }

            if (seriesid != null) {

                switch (seriesid.Value){

                    case "ER01_SAR_RAW_0P":
                    case "ER01_SAR_SLC_1P":
                    case "ER02_SAR_RAW_0P":
                    case "ER02_SAR_SLC_1P":                     
                    case "ER02_SAR_IMP_1P":
                    case "ER01_SAR_IMP_1P":
                    case "ER01_SAR_IMS_1P":
                    case "ER02_SAR_IMS_1P":
                    case "ER02_SAR_IM__0P":
                    case "ER01_SAR_IM__0P": 
                        sensor.Sensor.operationalMode = new Terradue.GeoJson.Gml.CodeListType();
                        sensor.Sensor.operationalMode.Text = new string[]{ "IM" };
                        break;
                    case "ASA_APC_0P":
                    case "ASA_APG_1P":
                    case "ASA_APH_0P":
                    case "ASA_APM_1P":
                    case "ASA_APP_1P":
                    case "ASA_APV_0P":
                    case "ASA_GM1_1P":
                    case "ASA_IMM_1P":
                    case "ASA_IMP_1P":
                    case "ASA_IMS_1P":
                    case "ASA_IM__0P":
                    case "ASA_WSM_1P":
                    case "ASA_WSS_1P":
                    case "ASA_WS__0C":
                    case "ASA_WS__0P":
                    case "ASA_WVI_1P":
                        sensor.Sensor.operationalMode = new Terradue.GeoJson.Gml.CodeListType();
                        sensor.Sensor.operationalMode.Text = new string[]{ seriesid.Value.Substring(4, 2) };
                        break;
                  

                }
        
            }

            return sensor;
            
        }

        public static AcquisitionType GetAcquisitionFromRdf(XElement rdf, XElement series){
            
            var orbitn = rdf.Element(XName.Get("orbitNumber", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
            var orbitd = rdf.Element(XName.Get("orbitDirection", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
            var track = rdf.Element(XName.Get("trackNumber", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
            var wrslog = rdf.Element(XName.Get("wrsLongitudeGrid", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));

            AcquisitionType acq = new AcquisitionType();

            if (orbitn != null)
                acq.orbitNumber = orbitn.Value;

            if (track != null){
                acq.wrsLongitudeGrid = new Terradue.GeoJson.Gml.CodeWithAuthorityType();
                acq.wrsLongitudeGrid.Value = track.Value;
            }

            if (wrslog != null){
                acq.wrsLongitudeGrid = new Terradue.GeoJson.Gml.CodeWithAuthorityType();
                acq.wrsLongitudeGrid.Value = wrslog.Value;
            }

            if (orbitd != null)
                acq.orbitDirection = (OrbitDirectionValueType)Enum.Parse(typeof(OrbitDirectionValueType), orbitd.Value.ToUpper());

            return acq;

        }

        public static TimeObjectPropertyType GetEOPhenomenonTypeFromRdf(XElement rdf, XElement series) {

            var start = rdf.Element(XName.Get("dtstart", "http://www.w3.org/2002/12/cal/ical#"));
                                    var end = rdf.Element(XName.Get("dtend", "http://www.w3.org/2002/12/cal/ical#"));


            TimeObjectPropertyType phenomenon = new Terradue.Metadata.EarthObservation.Ogc.Om.TimeObjectPropertyType();
            phenomenon.GmlTimePeriod = new Terradue.GeoJson.Gml.TimePeriodType();
            phenomenon.GmlTimePeriod.beginPosition = new Terradue.GeoJson.Gml.TimePositionType();
            phenomenon.GmlTimePeriod.beginPosition.Value = start.Value;
            phenomenon.GmlTimePeriod.endPosition = new Terradue.GeoJson.Gml.TimePositionType();
            phenomenon.GmlTimePeriod.endPosition.Value = end.Value;

            return phenomenon;
        }

        public static EarthObservationMetaDataPropertyType GetMetadataTypeFromRdf(XElement rdf, XElement series) {

            EarthObservationMetaDataPropertyType metadata = new EarthObservationMetaDataPropertyType();

            var seriesid = series.Element(XName.Get("identifier", "http://purl.org/dc/elements/1.1/")).Value;
            var identifier = rdf.Element(XName.Get("identifier", "http://purl.org/dc/elements/1.1/")).Value;

            metadata.EarthObservationMetaData = new EarthObservationMetaDataType();
            metadata.EarthObservationMetaData.acquisitionType = AcquisitionTypeValueType.NOMINAL;
            metadata.EarthObservationMetaData.parentIdentifier = seriesid;
            metadata.EarthObservationMetaData.identifier = identifier.Replace(".N1","");

            var pcenter = rdf.Element(XName.Get("processingCenter", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
            var pversion = rdf.Element(XName.Get("processorVersion", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));

            List<ProcessingInformationPropertyType> processings = new List<ProcessingInformationPropertyType>();

            ProcessingInformationPropertyType processing = new ProcessingInformationPropertyType();
            processing.ProcessingInformation = new ProcessingInformationType();

            if (pcenter != null) {
                processing.ProcessingInformation.processingCenter = new Terradue.GeoJson.Gml.CodeListType();
                processing.ProcessingInformation.processingCenter.Text = new string[]{ pcenter.Value};
            }

            if (pversion != null) {
                processing.ProcessingInformation.processorVersion = pversion.Value;
            }



            if (seriesid.EndsWith("0P"))
                processing.ProcessingInformation.processingLevel = "L0";
            if (seriesid.EndsWith("1P"))
                processing.ProcessingInformation.processingLevel = "L1";
            if (seriesid.EndsWith("2P"))
                processing.ProcessingInformation.processingLevel = "L2";
            if (seriesid.EndsWith("3P"))
                processing.ProcessingInformation.processingLevel = "L3";

            if (seriesid.StartsWith("AL"))
                processing.ProcessingInformation.processingLevel = "L" + seriesid.Substring(7, 1);

            if (seriesid.StartsWith("L3"))
                processing.ProcessingInformation.processingLevel = "L3";

            if (seriesid.Contains("LV1"))
                processing.ProcessingInformation.processingLevel = "L1";

            if (seriesid.Contains("LV2"))
                processing.ProcessingInformation.processingLevel = "L2";

            var format = series.Element(XName.Get("format", "http://purl.org/dc/elements/1.1/"));

            if (format != null)
                processing.ProcessingInformation.nativeProductFormat = format.Value;

            processings.Add(processing);

            metadata.EarthObservationMetaData.processing = processings.ToArray();

            metadata.EarthObservationMetaData.status = "ARCHIVED"; 


            return metadata;
        }

        public static EarthObservationResultPropertyType GetEopResultTypeFromRdf(XElement rdf, XElement series) {


            EarthObservationResultPropertyType result = new EarthObservationResultPropertyType();

            result.EarthObservationResult = new EarthObservationResultType();


            return result;
        }

        public static Terradue.GeoJson.Gml.FeaturePropertyType GetFeatureOfInterest(XElement rdf, XElement series) {

            var spatial = rdf.Element(XName.Get("spatial", "http://purl.org/dc/terms/"));

            var geom = GeometryFactory.WktToGeometry(spatial.Value);

            FootprintPropertyType feature = new FootprintPropertyType();
            feature.Footprint = new FootprintType();
            feature.Footprint.multiExtentOf = new Terradue.GeoJson.Gml.MultiSurfacePropertyType();
            feature.Footprint.multiExtentOf.MultiSurface = geom.ToMultiSurface();


            return feature;
        }
    }
}

