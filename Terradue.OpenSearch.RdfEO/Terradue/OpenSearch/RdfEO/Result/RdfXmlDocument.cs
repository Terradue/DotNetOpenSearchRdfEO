//
//  RdfXmlDocument.cs
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
using System.ComponentModel;
using System.Collections.Specialized;

namespace Terradue.OpenSearch.Result {
    /// <summary>
    /// Rdf xml document.
    /// </summary>
    public class RdfXmlDocument : IOpenSearchResultCollection {


        public static XNamespace rdfns = XNamespace.Get("http://www.w3.org/1999/02/22-rdf-syntax-ns#"),
            dclite4gns = XNamespace.Get("http://xmlns.com/2008/dclite4g#"),
            dcns = XNamespace.Get("http://purl.org/dc/elements/1.1/"),
            osns = XNamespace.Get("http://a9.com/-/spec/opensearch/1.1/"),
            atomns = XNamespace.Get("http://www.w3.org/2005/Atom"),
            wsns = XNamespace.Get("http://dclite4g.xmlns.com/ws.rdf#");

        List<RdfXmlResult> items;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Result.RdfXmlDocument"/> class.
        /// </summary>
        internal RdfXmlDocument(XDocument doc) : base() {

            XElement rdf, series, description;

            rdf = doc.Element(rdfns + "RDF");
            if (rdf == null)
                throw new FormatException("Not a RDF document");
            description = rdf.Element(rdfns + "Description");

            LoadDescription(description);

            series = rdf.Element(dclite4gns + "Series");

            IEnumerable<XElement> datasets = rdf.Elements(dclite4gns + "DataSet");

            items = LoadItems(datasets);


        }

        public RdfXmlDocument(IOpenSearchResultCollection results) : base() {

            Title = results.Title;
            Identifier = results.Identifier;
            Id = results.Id;

            duration = results.Duration;
            openSearchable = results.OpenSearchable;

            elementExtensions = results.ElementExtensions;

            if (results.Date.Ticks > 0)
                Date = results.Date;
            if (results.Items != null) {
                items = new List<RdfXmlResult>();
                foreach (var item in results.Items) {
                    var newItem = new RdfXmlResult(item);
                    items.Add(newItem);
                }
            }

        }

        /// <summary>
        /// Load the specified reader.
        /// </summary>
        /// <param name="reader">Reader.</param>
        public new static RdfXmlDocument Load(XmlReader reader) {
            return new RdfXmlDocument(XDocument.Load(reader));
        }

        void LoadDescription(XElement description) {

            foreach (XElement link in description.Elements(XName.Get("link", RdfXmlDocument.atomns.NamespaceName))) {
                links.Add(SyndicationLinkFromXElement(link));
            }


            elementExtensions = XElementsToElementExtensions(description.Elements());
        }

        public XElement GetDescription() {
            XElement description = new XElement(XName.Get("Description", rdfns.NamespaceName));
            foreach ( var link in Links ) {
                XElement atomlink = new XElement(XName.Get("link", RdfXmlDocument.atomns.NamespaceName));
                atomlink.SetAttributeValue(XName.Get("rel", RdfXmlDocument.atomns.NamespaceName), link.RelationshipType);
                atomlink.SetAttributeValue(XName.Get("type", RdfXmlDocument.atomns.NamespaceName), link.MediaType);
                atomlink.SetAttributeValue(XName.Get("href", RdfXmlDocument.atomns.NamespaceName), link.Uri);
                if ( link.Length > 0 )
                    atomlink.SetAttributeValue(XName.Get("length", RdfXmlDocument.atomns.NamespaceName), link.Length);
                description.Add(atomlink);
            }
            XElement exts = XElement.Load(ElementExtensions.GetReaderAtExtensionWrapper());
            description.Add(exts.Elements());
            description.SetAttributeValue(rdfns + "about", Identifier);
            description.Add(new XElement(XName.Get("title", dcns.NamespaceName), Title));
            description.Add(new XElement(XName.Get("date", RdfXmlDocument.dcns.NamespaceName), Date.ToString("yyyy-MM-ddThh:mm:ss.fZ")));
            return description;
        }

        void AlignNameSpaces(XElement rdf) {
            rdf.SetAttributeValue(XName.Get("rdf", XNamespace.Xmlns.NamespaceName), "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            rdf.SetAttributeValue(XName.Get("dclite4g", XNamespace.Xmlns.NamespaceName), "http://xmlns.com/2008/dclite4g#");
            rdf.SetAttributeValue(XName.Get("atom", XNamespace.Xmlns.NamespaceName), "http://www.w3.org/2005/Atom");
            rdf.SetAttributeValue(XName.Get("os", XNamespace.Xmlns.NamespaceName), "http://a9.com/-/spec/opensearch/1.1/");
            rdf.SetAttributeValue(XName.Get("time", XNamespace.Xmlns.NamespaceName), "http://a9.com/-/opensearch/extensions/time/1.0/");
            rdf.SetAttributeValue(XName.Get("dc", XNamespace.Xmlns.NamespaceName), "http://purl.org/dc/elements/1.1/");
            rdf.SetAttributeValue(XName.Get("dct", XNamespace.Xmlns.NamespaceName), "http://purl.org/dc/terms/");
            rdf.SetAttributeValue(XName.Get("dc", XNamespace.Xmlns.NamespaceName), "http://purl.org/dc/elements/1.1/");
        }

        static List<RdfXmlResult> LoadItems(IEnumerable<XElement> datasets) {

            List<RdfXmlResult> items = new List<RdfXmlResult>();

            foreach (XElement dataSet in datasets) {

                items.Add(new RdfXmlResult(dataSet));

            }

            return items;
        }

        #region IResultCollection implementation

        public string Id {
            get ;
            set ;
        }

        public IEnumerable<IOpenSearchResultItem> Items {
            get {
                return items.Cast<IOpenSearchResultItem>();
            }
            set {
                items = value.Cast<RdfXmlResult>().ToList();
            }
        }

        public TextSyndicationContent Title {
            get ;
            set ;
        }

        DateTime date;

        public DateTime Date {
            get {
                if (date.Ticks == 0)
                    return DateTime.UtcNow;
                return date;
            }
            set {
                date = value;
            }
        }

        public string Identifier {
            get {
                var identifier = ElementExtensions.ReadElementExtensions<string>("identifier", "http://purl.org/dc/elements/1.1/");
                return identifier.Count == 0 ? this.Id : identifier[0];
            }
            set {
                foreach (var ext in this.ElementExtensions.ToArray()) {
                    if (ext.OuterName == "identifier" && ext.OuterNamespace == "http://purl.org/dc/elements/1.1/") {
                        this.ElementExtensions.Remove(ext);
                        continue;
                    }
                }
                this.ElementExtensions.Add(new XElement(XName.Get("identifier", "http://purl.org/dc/elements/1.1/"), value).CreateReader());
            }
        }

        public long Count {
            get {
                return Items.Count();
            }
        }

        public long TotalResults {
            get {
                var totalResults = ElementExtensions.ReadElementExtensions<string>("totalResults", "http://a9.com/-/spec/opensearch/1.1/");
                return totalResults.Count == 0 ? 0 : long.Parse(totalResults[0]);
            }
            set {
                foreach (var ext in this.ElementExtensions.ToArray()) {
                    if (ext.OuterName == "totalResults" && ext.OuterNamespace == "http://a9.com/-/spec/opensearch/1.1//") {
                        this.ElementExtensions.Remove(ext);
                        continue;
                    }
                }
                this.ElementExtensions.Add(new XElement(XName.Get("totalResults", "http://a9.com/-/spec/opensearch/1.1/"), value).CreateReader());
            }
        }

        IOpenSearchable openSearchable;
        public IOpenSearchable OpenSearchable {
            get {
                return openSearchable;
            }
            set {
                openSearchable = value;
            }
        }

        NameValueCollection parameters;
        public NameValueCollection Parameters {
            get {
                return parameters;
            }
            set {
                parameters = value;
            }
        }

        TimeSpan duration;
        public TimeSpan Duration {
            get {
                var duration = ElementExtensions.ReadElementExtensions<double>("queryTime", "http://purl.org/dc/elements/1.1/");
                return duration.Count == 0 ? new TimeSpan() : TimeSpan.FromMilliseconds(duration[0]);
            }
            set {
                foreach (var ext in this.ElementExtensions.ToArray()) {
                    if (ext.OuterName == "queryTime" && ext.OuterNamespace == "http://purl.org/dc/elements/1.1/") {
                        this.ElementExtensions.Remove(ext);
                        continue;
                    }
                }
                this.ElementExtensions.Add(new XElement(XName.Get("queryTime", "http://purl.org/dc/elements/1.1/"), value.TotalMilliseconds).CreateReader());
            }
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

        public XDocument PrepareDocument() {

            XDocument doc = new XDocument();

            XElement rdf = new XElement(XName.Get("RDF", rdfns.NamespaceName));
            doc.Add(rdf);

            XElement description = GetDescription();
            rdf.Add(description);

            foreach (var item in items) {
                rdf.Add(item.GetElement());
            }

            AlignNameSpaces(rdf);

            return doc;
        }

        public string SerializeToString() {
            XDocument doc = PrepareDocument();
            return doc.ToString();

        }

        public void SerializeToStream(System.IO.Stream stream) {
            XDocument doc = PrepareDocument();
            doc.Save(stream);
        }

        public IOpenSearchResultCollection DeserializeFromStream(System.IO.Stream stream) {
            throw new NotImplementedException();
        }

        bool showNamespaces;

        public bool ShowNamespaces {
            get {
                return showNamespaces;
            }
            set {
                showNamespaces = value;
            }
        }

        public static IOpenSearchResultCollection CreateFromOpenSearchResultCollection(IOpenSearchResultCollection results) {
            if (results == null)
                throw new ArgumentNullException("results");

            RdfXmlDocument rdf = new RdfXmlDocument(results);

            return rdf;
        }

        public string ContentType {
            get {
                return "application/rdf+xml";
            }
        }

        Collection<SyndicationCategory> categories = new Collection<SyndicationCategory>();

        public Collection<SyndicationCategory> Categories {
            get {
                return categories;
            }
        }

        Collection<SyndicationPerson> authors = new Collection<SyndicationPerson>();

        public Collection<SyndicationPerson> Authors {
            get {
                return authors;
            }
            set{ }
        }


        readonly Collection<SyndicationPerson> contributors;
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

        TextSyndicationContent description;
        public TextSyndicationContent Description {
            get {
                return description;
            }
            set {
                description = value;
            }
        }

        string generator;
        public string Generator {
            get {
                return generator;
            }
            set {
                generator = value;
            }
        }
        #endregion

        internal static SyndicationLink SyndicationLinkFromXElement(XElement elem) {

            var atom = XNamespace.Get("http://www.w3.org/2005/Atom");

            SyndicationLink link = new SyndicationLink(new Uri(elem.Attribute(atom + "href").Value));
            if (elem.Attribute(atom + "rel") != null)
                link.RelationshipType = elem.Attribute(atom + "rel").Value;
            if (elem.Attribute(atom + "title") != null)
                link.Title = elem.Attribute(atom + "title").Value;
            if (elem.Attribute(atom + "type") != null)
                link.MediaType = elem.Attribute(atom + "type").Value;
            if (elem.Attribute(atom + "length") != null)
                link.Length = long.Parse(elem.Attribute(atom + "length").Value);
            return link;

        }

        public static SyndicationElementExtensionCollection XElementsToElementExtensions(IEnumerable<XElement> elements) {

            SyndicationElementExtensionCollection exts = new SyndicationElementExtensionCollection();

            foreach (var element in elements) {
                exts.Add(new SyndicationElementExtension(element.CreateReader()));
            }

            return exts;
        }
    }

}

