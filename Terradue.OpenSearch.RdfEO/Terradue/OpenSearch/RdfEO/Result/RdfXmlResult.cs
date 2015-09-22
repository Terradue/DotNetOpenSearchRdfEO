//
//  RdfXmlResult.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using Terradue.ServiceModel.Syndication;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.IO;
using Terradue.GeoJson.Geometry;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.RdfEO.Result {

    public class RdfXmlResult : IOpenSearchResultItem {

        public RdfXmlResult(IOpenSearchResultItem item) : base() {

            if (item.Title != null)
                Title = item.Title;
            if (item.LastUpdatedTime.Ticks != 0)
                LastUpdatedTime = item.LastUpdatedTime;
            if (!string.IsNullOrEmpty(item.Identifier))
                Identifier = item.Identifier;
            elementExtensions = item.ElementExtensions;
            links = item.Links;

        }

        public RdfXmlResult(XElement root, XElement series) : base() {
            if (root.Element(XName.Get("title", "http://purl.org/dc/elements/1.1/")) != null)
                Title = new TextSyndicationContent(root.Element(XName.Get("title", "http://purl.org/dc/elements/1.1/")).Value);
            if (root.Element(XName.Get("created", "http://purl.org/dc/terms/")) != null)
                PublishDate = DateTime.Parse(root.Element(XName.Get("created", "http://purl.org/dc/terms/")).Value);
            if (root.Element(XName.Get("modified", "http://purl.org/dc/terms/")) != null)
                LastUpdatedTime = DateTime.Parse(root.Element(XName.Get("modified", "http://purl.org/dc/terms/")).Value);
            if (root.Element(XName.Get("identifier", "http://purl.org/dc/elements/1.1/")) != null)
                Identifier = root.Element(XName.Get("identifier", "http://purl.org/dc/elements/1.1/")).Value;
            //
            if (root.Element(XName.Get("EarthObservation", Terradue.Metadata.EarthObservation.MetadataHelpers.ALT)) == null
                && root.Element(XName.Get("EarthObservation", Terradue.Metadata.EarthObservation.MetadataHelpers.ALT20)) == null
                && root.Element(XName.Get("EarthObservation", Terradue.Metadata.EarthObservation.MetadataHelpers.ATM)) == null
                && root.Element(XName.Get("EarthObservation", Terradue.Metadata.EarthObservation.MetadataHelpers.ATM20)) == null
                && root.Element(XName.Get("EarthObservation", Terradue.Metadata.EarthObservation.MetadataHelpers.EOP)) == null
                && root.Element(XName.Get("EarthObservation", Terradue.Metadata.EarthObservation.MetadataHelpers.EOP20)) == null
                && root.Element(XName.Get("EarthObservation", Terradue.Metadata.EarthObservation.MetadataHelpers.LMB)) == null
                && root.Element(XName.Get("EarthObservation", Terradue.Metadata.EarthObservation.MetadataHelpers.LMB20)) == null
                && root.Element(XName.Get("EarthObservation", Terradue.Metadata.EarthObservation.MetadataHelpers.OPT)) == null
                && root.Element(XName.Get("EarthObservation", Terradue.Metadata.EarthObservation.MetadataHelpers.OPT20)) == null
                && root.Element(XName.Get("EarthObservation", Terradue.Metadata.EarthObservation.MetadataHelpers.SAR)) == null
                && root.Element(XName.Get("EarthObservation", Terradue.Metadata.EarthObservation.MetadataHelpers.SAR20)) == null
                && root.Element(XName.Get("EarthObservation", Terradue.Metadata.EarthObservation.MetadataHelpers.SSP)) == null
                && root.Element(XName.Get("EarthObservation", Terradue.Metadata.EarthObservation.MetadataHelpers.SSP20)) == null) {
                ElementExtensions.Add(BuildEarthObservationReader(root, series));
            } else {
                elementExtensions = RdfXmlDocument.XElementsToElementExtensions(root.Elements());
            }
            links = new Collection<SyndicationLink>();
            if (root.Attribute(XName.Get("about", RdfXmlDocument.rdfns.NamespaceName)) != null) {
                links.Add(new SyndicationLink(new Uri(root.Attribute(XName.Get("about", RdfXmlDocument.rdfns.NamespaceName)).Value), "self", "Reference Link", "application/rdf+xml", 0));
            }
            foreach (XElement onlineResource in root.Elements(XName.Get("onlineResource", RdfXmlDocument.dclite4gns.NamespaceName))) {
                var sizex = root.Element(XName.Get("size", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"));
                long size = 0;
                if (sizex != null)
                    long.TryParse(sizex.Value, out size);
                links.Add(new SyndicationLink(new Uri(onlineResource.Elements().First().Attribute(XName.Get("about", RdfXmlDocument.rdfns.NamespaceName)).Value), "enclosure", "Data", "application/binary", size));
            }
        }

        public XElement GetElement() {
            XElement root = new XElement(XName.Get("DataSet", RdfXmlDocument.dclite4gns.NamespaceName));
            root.SetAttributeValue(XName.Get("about", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"), Id);
            if (Title != null) {
                root.Add(new XElement(XName.Get("title", "http://purl.org/dc/elements/1.1/"), Title.Text));
            }
            root.Add(new XElement(XName.Get("modified", "http://purl.org/dc/terms/"), LastUpdatedTime.ToString("yyyy-MM-ddThh:mm:ss.fffZ")));
            root.Add(new XElement(XName.Get("created", "http://purl.org/dc/terms/"), PublishDate.ToString("yyyy-MM-ddThh:mm:ss.fffZ")));
            root.Add(new XElement(XName.Get("identifier", "http://purl.org/dc/elements/1.1/"), Identifier));
            root.Add(new XElement(XName.Get("dtstart", "http://www.w3.org/2002/12/cal/ical#"), Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindStartDateFromOpenSearchResultItem(this)));
            root.Add(new XElement(XName.Get("dtend", "http://www.w3.org/2002/12/cal/ical#"), Terradue.Metadata.EarthObservation.OpenSearch.EarthObservationOpenSearchResultHelpers.FindEndDateFromOpenSearchResultItem(this)));

            XElement exts = XElement.Load(ElementExtensions.GetReaderAtExtensionWrapper());
            root.Add(exts.Elements());
            if (exts.Descendants(XName.Get("beginPosition", "http://www.opengis.net/gml/3.2")).Count() > 0) {
                root.Add(new XElement(XName.Get("dtstart", "http://www.w3.org/2002/12/cal/ical#"), exts.Descendants(XName.Get("beginPosition", "http://www.opengis.net/gml/3.2")).First().Value));
            }
            if (exts.Descendants(XName.Get("endPosition", "http://www.opengis.net/gml/3.2")).Count() > 0) {
                root.Add(new XElement(XName.Get("dtend", "http://www.w3.org/2002/12/cal/ical#"), exts.Descendants(XName.Get("endPosition", "http://www.opengis.net/gml/3.2")).First().Value));
            }
            if (exts.Descendants(XName.Get("multiExtentOf", "http://www.opengis.net/eop/2.0")).Count() > 0 && exts.Descendants(XName.Get("multiExtentOf", "http://www.opengis.net/eop/2.0")).Elements().Count() > 0) {
                XElement gml = exts.Descendants(XName.Get("multiExtentOf", "http://www.opengis.net/eop/2.0")).Elements().First();
                XmlDocument doc = new XmlDocument();
                XmlElement element = doc.ReadNode(gml.CreateReader()) as XmlElement;
                var feature = GeometryFactory.GmlToFeature(element);
                root.Add(new XElement(XName.Get("spatial", "http://purl.org/dc/terms/"), feature.ToWkt()));
            }
            if (exts.Descendants(XName.Get("multiExtentOf", "http://www.opengis.net/eop/2.1")).Count() > 0 && exts.Descendants(XName.Get("multiExtentOf", "http://www.opengis.net/eop/2.1")).Elements().Count() > 0) {
                XElement gml = exts.Descendants(XName.Get("multiExtentOf", "http://www.opengis.net/eop/2.1")).Elements().First();
                XmlDocument doc = new XmlDocument();
                XmlElement element = doc.ReadNode(gml.CreateReader()) as XmlElement;
                var feature = GeometryFactory.GmlToFeature(element);
                root.Add(new XElement(XName.Get("spatial", "http://purl.org/dc/terms/"), feature.ToWkt()));
            }
            root.SetAttributeValue(XName.Get("ical", XNamespace.Xmlns.NamespaceName), "http://www.w3.org/2002/12/cal/ical#");
            root.SetAttributeValue(XName.Get("dct", XNamespace.Xmlns.NamespaceName), "http://purl.org/dc/terms/");
            root.Add(GetOnlineResources());
            var enclosures = Links.Where(l => l.RelationshipType == "enclosure");
            if (enclosures.Count() > 0) {
                root.Add(new XElement(XName.Get("size", "http://www.genesi-dr.eu/spec/opensearch/extensions/eop/1.0/"), enclosures.First().Length));
            }
            return root;
        }

        IEnumerable<XElement> GetOnlineResources() {
            List<XElement> elements = new List<XElement>();
            var enclosures = Links.Where(l => l.RelationshipType == "enclosure");
            foreach (var enclosure in enclosures) {
                UriBuilder uri = new UriBuilder(enclosure.Uri);
                XElement onlineResource = new XElement(XName.Get("onlineResource", RdfXmlDocument.dclite4gns.NamespaceName));
                onlineResource.SetAttributeValue(XName.Get("ws", XNamespace.Xmlns.NamespaceName), "http://dclite4g.xmlns.com/ws.rdf#");
                XElement ws = new XElement(XName.Get(uri.Scheme.ToUpper(), RdfXmlDocument.wsns.NamespaceName));
                ws.SetAttributeValue(XName.Get("about", RdfXmlDocument.rdfns.NamespaceName), uri.ToString());
                onlineResource.Add(ws);
                elements.Add(onlineResource);
            }
            return elements;
        }

        public RdfXmlDocument Parent {
            get ;
            set ;
        }

        public XmlReader BuildEarthObservationReader(XElement root, XElement series) {

            return RdfEarthObservationFactory.GetXmlReaderEarthObservationTypeFromRdf(root, series);


        }

        #region IOpenSearchResult implementation

        string id;

        public string Id {
            get {
                if (string.IsNullOrEmpty(id)) {
                    var link = Links.FirstOrDefault(l => l.RelationshipType == "self");
                    id = link == null ? null : link.Uri.ToString();
                }
                return id;
            }
            set {
                id = value;
            }
        }

        public TextSyndicationContent Title {
            get ;
            set ;
        }

        public DateTime LastUpdatedTime {
            get ;
            set ;
        }

        public DateTime PublishDate {
            get ;
            set ;
        }

        public string Identifier {
            get ;
            set ;
        }

        SyndicationElementExtensionCollection elementExtensions = new SyndicationElementExtensionCollection();

        public SyndicationElementExtensionCollection ElementExtensions {
            get {
                return elementExtensions;
            }
            set {
                elementExtensions = value;
            }
        }

        Collection<Terradue.ServiceModel.Syndication.SyndicationLink> links = new Collection<SyndicationLink>();

        public Collection<Terradue.ServiceModel.Syndication.SyndicationLink> Links {
            get {
                return links;
            }
            set {
                links = value;
            }
        }

        public bool ShowNamespaces {
            get {
                return true;
            }
            set {
                ;
                ;
            }
        }

        readonly Collection<SyndicationCategory> categories = new Collection<SyndicationCategory>();

        public Collection<SyndicationCategory> Categories {
            get {
                return categories;
            }
        }

        readonly Collection<SyndicationPerson> authors = new Collection<SyndicationPerson>();

        public Collection<SyndicationPerson> Authors {
            get {
                return authors;
            }
        }


        TextSyndicationContent summary;

        public TextSyndicationContent Summary {
            get {
                return summary;
            }
            set {
                summary = value;
            }
        }

        readonly Collection<SyndicationPerson> contributors = new Collection<SyndicationPerson>();

        public Collection<SyndicationPerson> Contributors {
            get {
                return contributors;
            }
        }

        TextSyndicationContent copyright;

        public TextSyndicationContent Copyright {
            get {
                return copyright;
            }
            set {
                copyright = value;
            }
        }

        SyndicationContent content;

        public SyndicationContent Content {
            get {
                return content;
            }
            set {
                content = value;
            }
        }

        #endregion


    }
}
