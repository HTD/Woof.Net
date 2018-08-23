using System.Collections;
using System.Collections.Generic;

namespace Woof.Net.Http {

    /// <summary>
    /// Represents a specialized collection of <see cref="SiteBinding"/> items.
    /// </summary>
    public class SiteBindingCollection : IEnumerable<SiteBinding> {

        /// <summary>
        /// Site bindings.
        /// </summary>
        public readonly List<SiteBinding> Items = new List<SiteBinding>();

        /// <summary>
        /// Creates an empty site binding collection.
        /// </summary>
        public SiteBindingCollection() { }

        /// <summary>
        /// Creates a site binding collection from any other site binding collection.
        /// </summary>
        /// <param name="siteBindings">Site bindings collection.</param>
        public SiteBindingCollection(IEnumerable<SiteBinding> siteBindings) => Items.AddRange(siteBindings);

        /// <summary>
        /// Adds a new binding to internal items list.
        /// </summary>
        /// <param name="siteBinding">Site binding.</param>
        public void Add(SiteBinding siteBinding) => Items.Add(siteBinding);

        /// <summary>
        /// Adds a new binding to internal items list.
        /// </summary>
        /// <param name="documentRoot">Document root location.</param>
        public void Add(string documentRoot) => Items.Add(new SiteBinding { DocumentRoot = documentRoot });

        /// <summary>
        /// Adds a new binding to internal items list.
        /// </summary>
        /// <param name="documentRoot">Document root location.</param>
        /// <param name="pathPrefix">Path prefix within the server prefix.</param>
        public void Add(string documentRoot, string pathPrefix) => Items.Add(new SiteBinding { DocumentRoot = documentRoot, PathPrefix = pathPrefix });

        /// <summary>
        /// Clears all bindings.
        /// </summary>
        public void Clear() => Items.Clear();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        public IEnumerator<SiteBinding> GetEnumerator() => ((IEnumerable<SiteBinding>)Items).GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<SiteBinding>)Items).GetEnumerator();

    }

}