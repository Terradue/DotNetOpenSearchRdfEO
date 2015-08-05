//
//  RdfOpenSearchEngineExtension.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using Mono.Addins;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Xml;
using System.Diagnostics;
using Terradue.OpenSearch.Result;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using Terradue.OpenSearch.Response;
using Terradue.ServiceModel.Syndication;
using System.Xml.Linq;
using System.IO;

[assembly:Addin]
[assembly:AddinDependency("OpenSearchEngine", "1.0")]
namespace Terradue.OpenSearch.Engine.Extensions {
    /// <summary>
    /// Rdf open search engine extension.
    /// </summary>
    /// <description>
    /// Extension that allows to query and transform Rdf OpenSearchable source to Rdf XML document (Atom).
    /// </description>
    [Extension(typeof(IOpenSearchEngineExtension))]
    [ExtensionNode("RDF", "RDF native query")]
    public class RdfOpenSearchEngineExtension : OpenSearchEngineExtension<RdfXmlDocument> {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Terradue.OpenSearch.EngineExtensions.RdfOpenSearchEngineExtension"/> class.
        /// </summary>
        public RdfOpenSearchEngineExtension() {
        }

        #region OpenSearchEngineExtension implementation

        public override string Identifier {
            get {
                return "rdf";
            }
        }

        public override string Name {
            get {
                return "RDF Xml Document";
            }
        }

        public override IOpenSearchResultCollection ReadNative(IOpenSearchResponse response) {
            if (response.ContentType == "application/rdf+xml")
                return TransformRdfResponseToRdfXmlDocument(response);

            throw new NotSupportedException("RDF extension does not transform OpenSearch response from " + response.ContentType);
        }

        #region implemented abstract members of OpenSearchEngineExtension

        public override IOpenSearchResultCollection CreateOpenSearchResultFromOpenSearchResult(IOpenSearchResultCollection results) {
            if (results is RdfXmlDocument)
                return results;

            return RdfXmlDocument.CreateFromOpenSearchResultCollection(results);
        }

        #endregion

        public override string DiscoveryContentType {
            get {
                return "application/rdf+xml";
            }
        }

        public override OpenSearchUrl FindOpenSearchDescriptionUrlFromResponse(IOpenSearchResponse response) {
            RdfXmlDocument rdfDoc = TransformRdfResponseToRdfXmlDocument(response);

            SyndicationLink link = rdfDoc.Links.SingleOrDefault(l => l.RelationshipType == "search");

            if (link == null)
                return null;
            return new OpenSearchUrl(link.Uri);
        }

        #endregion

        /// <summary>
        /// Transforms the rdf response to rdf xml document.
        /// </summary>
        /// <returns>The rdf response to rdf xml document.</returns>
        /// <param name="response">Response.</param>
        public static RdfXmlDocument TransformRdfResponseToRdfXmlDocument(IOpenSearchResponse response) {
            RdfXmlDocument rdfDoc;

            if (response.ObjectType == typeof(byte[])) {

                XmlReader reader = XmlReader.Create(new MemoryStream((byte[])response.GetResponseObject()));

                rdfDoc = RdfXmlDocument.Load(reader);

                rdfDoc.OpenSearchable = response.Entity;
                rdfDoc.QueryTimeSpan = response.RequestTime;
       
                return rdfDoc;
            }

            return null;
        }
    }
}