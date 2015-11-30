using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace ConDep.Dsl.Remote.Node.Model
{

    public class Link : IEquatable<Link>
    {
        public string Rel { get; set; }
        public string Href { get; set; }
        public IEnumerable<Link> Links { get; set; } = new List<Link>();

        public string Method { get; set; }

        public HttpMethod HttpMethod
        {
            get
            {
                switch (Method)
                {
                    case "DELETE": return HttpMethod.Delete;
                    case "GET": return HttpMethod.Get;
                    case "HEAD": return HttpMethod.Head;
                    case "OPTIONS": return HttpMethod.Options;
                    case "POST": return HttpMethod.Post;
                    case "PUT": return HttpMethod.Put;
                    case "TRACE": return HttpMethod.Trace;
                    default : throw new NotSupportedException(Method);
                }
            }
        }

        public bool Equals(Link other)
        {
            if (other == null) return false;
            if (!Rel.Equals(other.Rel, StringComparison.InvariantCultureIgnoreCase)) return false;
            if (!Href.Equals(other.Href, StringComparison.InvariantCultureIgnoreCase)) return false;
            if (!Method.Equals(other.Method, StringComparison.InvariantCultureIgnoreCase)) return false;
            if (Links.Intersect(other.Links).Any()) return false;
            return true;
        }
    }
}