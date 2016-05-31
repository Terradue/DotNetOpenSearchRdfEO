using System;
using Terradue.ServiceModel.Syndication;
using System.Xml.Linq;
using Terradue.OpenSearch.Result;
using System.Xml;
using Terradue.OpenSearch.RdfEO.Result;
using System.Collections.Generic;
using Terradue.GeoJson.Geometry;
using System.Xml.Serialization;
using Terradue.Metadata.EarthObservation;
using System.IO;
using System.Text.RegularExpressions;
using Terradue.ServiceModel.Ogc;

namespace Terradue.OpenSearch.RdfEO {
    public class RdfEarthObservationFactory {

        public static XmlReader GetXmlReaderEarthObservationTypeFromRdf(XElement rdf, XElement series) {

            var eo = GetEarthObservationFromRdf(rdf, series);

            var ser = OgcHelpers.GetXmlSerializerFromType(eo.GetType());

            MemoryStream ms = new MemoryStream();

            ser.Serialize(ms, eo);

            ms.Seek(0, SeekOrigin.Begin);

            return XmlReader.Create(ms);

        }

        public static Terradue.ServiceModel.Ogc.Eop21.EarthObservationType GetEarthObservationFromRdf(XElement rdf, XElement series) {
            
            Terradue.ServiceModel.Ogc.Eop21.EarthObservationType eo = new Terradue.ServiceModel.Ogc.Eop21.EarthObservationType();

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
                        return GetEarthObservationTypeFromRdf(rdf, series, "OPTICAL");

                    case "ASAR":
                    case "PALSAR2":
                    case "PALSAR":
                    case "SAR":
                    case "TSX-SAR":
                    case "MIRAS":
                    case "AMI":
                    case "ASPS":
                        return GetEarthObservationTypeFromRdf(rdf, series, "RADAR");
                    case "SIRAL":
                    case "RA2":
                    case "DORIS":
                        return GetEarthObservationTypeFromRdf(rdf, series, "ALTIMETRIC");
                    
                    
                    case "MIPAS":
                    case "SCIAMACHY":
                    case "GOMOS":
                    case "GOME":
                        throw new NotImplementedException();
                //return GetAtmEarthObservationTypeFromRdf(rdf);


                }
                    
            }

            var identifier = rdf.Element(XName.Get("identifier", "http://purl.org/dc/elements/1.1/"));

            if (identifier != null) {
                if (identifier.Value.StartsWith("S1A")) {
                    return GetEarthObservationTypeFromRdf(rdf, series, "RADAR"); 
                }
            }

            return GetEarthObservationTypeFromRdf(rdf, series, "");
        }

        public static Terradue.ServiceModel.Ogc.Eop21.EarthObservationType GetEarthObservationTypeFromRdf(XElement rdf, XElement series, string type) {

            Terradue.ServiceModel.Ogc.Eop21.EarthObservationType eo =new Terradue.ServiceModel.Ogc.Eop21.EarthObservationType();

            if (type == "RADAR")
                eo = new Terradue.ServiceModel.Ogc.Sar21.SarEarthObservationType();
            if (type == "OPTICAL")
                eo = new Terradue.ServiceModel.Ogc.Opt21.OptEarthObservationType();
            if (type == "ALTIMETRIC")
                eo = new Terradue.ServiceModel.Ogc.Alt21.AltEarthObservationType();
            if (type == "ATMOSPHERIC")
                eo = new Terradue.ServiceModel.Ogc.Atm21.AtmEarthObservationType();
            if (type == "LIMB")
                eo = new Terradue.ServiceModel.Ogc.Lmb21.LmbEarthObservationType();
            if (type == "SSP")
                eo = new Terradue.ServiceModel.Ogc.Ssp21.SspEarthObservationType();
             

            // Equipment
            eo.procedure = new Terradue.ServiceModel.Ogc.Om.OM_ProcessPropertyType();
            eo.procedure.Eop21EarthObservationEquipment = GetEop21EarthObservationEquipmentFromRdf(rdf, series, type);

            // Phenomenon
            eo.phenomenonTime = GetEOPhenomenonTypeFromRdf(rdf, series);

            // Metadata
            eo.EopMetaDataProperty = GetMetadataTypeFromRdf(rdf, series);

            // Result
            eo.result = GetEopResultTypeFromRdf(rdf, series, type);

            // Footprint
            eo.featureOfInterest = GetFeatureOfInterest(rdf, series, type);

            return eo;
        }

        public static Terradue.ServiceModel.Ogc.Eop21.EarthObservationEquipmentType GetEop21EarthObservationEquipmentFromRdf(XElement rdf, XElement series, string type) {

            var platform = series.Element(XName.Get("platform", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
            var subject = series.Element(XName.Get("subject", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
            var sensor = series.Element(XName.Get("sensor", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));

            Terradue.ServiceModel.Ogc.Eop21.EarthObservationEquipmentType equipment = new Terradue.ServiceModel.Ogc.Eop21.EarthObservationEquipmentType();

            equipment.platform = GetPlatformFromRdf(rdf, series);

            equipment.instrument = GetInstrumentFromRdf(rdf, series);

            equipment.sensor = GetSensorFromRdf(rdf, series);

            equipment.acquisitionParameters = new Terradue.ServiceModel.Ogc.Eop21.AcquisitionPropertyType();

            equipment.acquisitionParameters.Acquisition = GetAcquisitionFromRdf(rdf, series, type);

            equipment.sensor.Sensor.sensorType = type;

            var polc = rdf.Element(XName.Get("polarisationChannels", "http://earth.esa.int/sar"));

            if (polc != null && type == "RADAR")
                equipment.acquisitionParameters.SarAcquisition.polarisationChannels = polc.Value;

            return equipment;
        }


        public static Terradue.ServiceModel.Ogc.Eop21.PlatformPropertyType GetPlatformFromRdf(XElement rdf, XElement series) {


            var xplatform = series.Element(XName.Get("platform", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
            var subject = series.Element(XName.Get("subject", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));


            Terradue.ServiceModel.Ogc.Eop21.PlatformPropertyType platform = new Terradue.ServiceModel.Ogc.Eop21.PlatformPropertyType();
            platform.Platform = new Terradue.ServiceModel.Ogc.Eop21.PlatformType();

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


        public static Terradue.ServiceModel.Ogc.Eop21.InstrumentPropertyType GetInstrumentFromRdf(XElement rdf, XElement series) {


            var sensor = series.Element(XName.Get("sensor", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));

            if (sensor == null)
                return null;

            Terradue.ServiceModel.Ogc.Eop21.InstrumentPropertyType instrument = new Terradue.ServiceModel.Ogc.Eop21.InstrumentPropertyType();
            instrument.Instrument = new Terradue.ServiceModel.Ogc.Eop21.InstrumentType();
            instrument.Instrument.shortName = sensor.Value;

            return instrument;

        }

        public static Terradue.ServiceModel.Ogc.Eop21.SensorPropertyType GetSensorFromRdf(XElement rdf, XElement series) {

            var sensorx = series.Element(XName.Get("platform", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
            var seriesid = series.Element(XName.Get("identifier", "http://purl.org/dc/elements/1.1/"));
            var swathid = rdf.Element(XName.Get("swathIdentifier", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));

            Terradue.ServiceModel.Ogc.Eop21.SensorPropertyType sensor = new Terradue.ServiceModel.Ogc.Eop21.SensorPropertyType();
            sensor.Sensor = new Terradue.ServiceModel.Ogc.Eop21.SensorType();

            if (swathid != null) {
                sensor.Sensor.swathIdentifier = new Terradue.ServiceModel.Ogc.Gml321.CodeListType();
                sensor.Sensor.swathIdentifier.Text = swathid.Value;
            }

            if (seriesid != null) {

                switch (seriesid.Value) {

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
                        sensor.Sensor.operationalMode = new Terradue.ServiceModel.Ogc.Gml321.CodeListType();
                        sensor.Sensor.operationalMode.Text = "IM";
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
                        sensor.Sensor.operationalMode = new Terradue.ServiceModel.Ogc.Gml321.CodeListType();
                        sensor.Sensor.operationalMode.Text = seriesid.Value.Substring(4, 2);
                        break;
                  

                }
        
            }

            return sensor;
            
        }

        public static Terradue.ServiceModel.Ogc.Eop21.AcquisitionType GetAcquisitionFromRdf(XElement rdf, XElement series, string type) {
            
            var orbitn = rdf.Element(XName.Get("orbitNumber", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
            var orbitd = rdf.Element(XName.Get("orbitDirection", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
            var track = rdf.Element(XName.Get("trackNumber", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
            var wrslog = rdf.Element(XName.Get("wrsLongitudeGrid", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));

            Terradue.ServiceModel.Ogc.Eop21.AcquisitionType acq = null;

            if (type == "RADAR")
                acq = new Terradue.ServiceModel.Ogc.Sar21.SarAcquisitionType();
            else
                acq = new Terradue.ServiceModel.Ogc.Eop21.AcquisitionType();

            if (orbitn != null)
                acq.orbitNumber = orbitn.Value;

            if (track != null) {
                acq.wrsLongitudeGrid = new Terradue.ServiceModel.Ogc.Gml321.CodeWithAuthorityType();
                acq.wrsLongitudeGrid.Value = track.Value;
            }

            if (wrslog != null) {
                acq.wrsLongitudeGrid = new Terradue.ServiceModel.Ogc.Gml321.CodeWithAuthorityType();
                acq.wrsLongitudeGrid.Value = wrslog.Value;
            }

            if (orbitd != null)
                acq.orbitDirection = (Terradue.ServiceModel.Ogc.Eop21.OrbitDirectionValueType)Enum.Parse(typeof(Terradue.ServiceModel.Ogc.Eop21.OrbitDirectionValueType), orbitd.Value.ToUpper());

            return acq;

        }

        public static Terradue.ServiceModel.Ogc.Om.TimeObjectPropertyType GetEOPhenomenonTypeFromRdf(XElement rdf, XElement series) {

            var start = rdf.Element(XName.Get("dtstart", "http://www.w3.org/2002/12/cal/ical#"));
            var end = rdf.Element(XName.Get("dtend", "http://www.w3.org/2002/12/cal/ical#"));


            Terradue.ServiceModel.Ogc.Om.TimeObjectPropertyType phenomenon = new Terradue.ServiceModel.Ogc.Om.TimeObjectPropertyType();
            phenomenon.GmlTimePeriod = new Terradue.ServiceModel.Ogc.Gml321.TimePeriodType();
            phenomenon.GmlTimePeriod.beginPosition = new Terradue.ServiceModel.Ogc.Gml321.TimePositionType();
            phenomenon.GmlTimePeriod.beginPosition.Value = start.Value;
            phenomenon.GmlTimePeriod.endPosition = new Terradue.ServiceModel.Ogc.Gml321.TimePositionType();
            phenomenon.GmlTimePeriod.endPosition.Value = end.Value;

            return phenomenon;
        }

        public static Terradue.ServiceModel.Ogc.Eop21.EarthObservationMetaDataPropertyType GetMetadataTypeFromRdf(XElement rdf, XElement series) {

            Terradue.ServiceModel.Ogc.Eop21.EarthObservationMetaDataPropertyType metadata = new Terradue.ServiceModel.Ogc.Eop21.EarthObservationMetaDataPropertyType();

            var seriesid = series.Element(XName.Get("identifier", "http://purl.org/dc/elements/1.1/")).Value;
            var identifier = rdf.Element(XName.Get("identifier", "http://purl.org/dc/elements/1.1/")).Value;

            metadata.EarthObservationMetaData = new Terradue.ServiceModel.Ogc.Eop21.EarthObservationMetaDataType();
            metadata.EarthObservationMetaData.acquisitionType = Terradue.ServiceModel.Ogc.Eop21.AcquisitionTypeValueType.NOMINAL;
            metadata.EarthObservationMetaData.parentIdentifier = seriesid;
            metadata.EarthObservationMetaData.identifier = identifier;
            metadata.EarthObservationMetaData.productType = GetProductType(seriesid);

            var pcenter = rdf.Element(XName.Get("processingCenter", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
            var pversion = rdf.Element(XName.Get("processorVersion", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));

            List<Terradue.ServiceModel.Ogc.Eop21.ProcessingInformationPropertyType> processings = new List<Terradue.ServiceModel.Ogc.Eop21.ProcessingInformationPropertyType>();

            Terradue.ServiceModel.Ogc.Eop21.ProcessingInformationPropertyType processing = new Terradue.ServiceModel.Ogc.Eop21.ProcessingInformationPropertyType();
            processing.ProcessingInformation = new Terradue.ServiceModel.Ogc.Eop21.ProcessingInformationType();

            if (pcenter != null) {
                processing.ProcessingInformation.processingCenter = new Terradue.ServiceModel.Ogc.Gml321.CodeListType();
                processing.ProcessingInformation.processingCenter.Text = pcenter.Value;
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

        public static Terradue.ServiceModel.Ogc.Om.OM_ResultPropertyType GetEopResultTypeFromRdf(XElement rdf, XElement series, string type) {


            Terradue.ServiceModel.Ogc.Om.OM_ResultPropertyType result = new Terradue.ServiceModel.Ogc.Om.OM_ResultPropertyType();

            if (type == "OPTICAL")
                result.Opt21EarthObservationResult = new Terradue.ServiceModel.Ogc.Opt21.OptEarthObservationResultType();
            else if (type == "ATMOSPHERIC")
                result.Atm21EarthObservationResult = new Terradue.ServiceModel.Ogc.Atm21.AtmEarthObservationResultType();
            else
                result.Eop21EarthObservationResult = new Terradue.ServiceModel.Ogc.Eop21.EarthObservationResultType();


            return result;
        }

        public static Terradue.ServiceModel.Ogc.Gml321.FeaturePropertyType GetFeatureOfInterest(XElement rdf, XElement series, string type) {

            var spatial = rdf.Element(XName.Get("spatial", "http://purl.org/dc/terms/"));

            var geom = WktExtensions.WktToGeometry(spatial.Value);

            Terradue.ServiceModel.Ogc.Gml321.FeaturePropertyType feature = new Terradue.ServiceModel.Ogc.Gml321.FeaturePropertyType();
            if (type == "ALTIMETRIC")
                feature.Eop21Footprint = new Terradue.ServiceModel.Ogc.Alt21.AltFootprintType();
            else
                feature.Eop21Footprint = new Terradue.ServiceModel.Ogc.Eop21.FootprintType();
            feature.Eop21Footprint.multiExtentOf = new Terradue.ServiceModel.Ogc.Gml321.MultiSurfacePropertyType();
            feature.Eop21Footprint.multiExtentOf.MultiSurface = Terradue.GeoJson.Gml321.Gml321Extensions.ToGmlMultiSurface(geom);


            return feature;
        }

        public static string GetProductType(string seriesid) {
            if (seriesid != null) {

                Regex regex = new Regex("(ER0[1-2]_)?(SAR|ASA)_(?<type>[^_]*)_*([0-3].?)");
                Match match = regex.Match(seriesid);

                if (match.Success) {
                    return match.Groups["type"].Value;
                }

            }

            return null;
        }
    }
}

