using System.Collections.Generic;
using System.Linq;

namespace ConDep.Dsl.Remote.Node.Model
{
    public static class LinkListExtensions
    {
        public static Link GetByRel(this List<Link> listOfLinks, string rel)
        {
            return listOfLinks.SingleOrDefault(link => link.Rel == rel);
        } 
    }
}