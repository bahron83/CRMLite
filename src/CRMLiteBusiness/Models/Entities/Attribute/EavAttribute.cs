using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMLiteBusiness
{
    public class EavAttribute
    {
        public int AttributeID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AttributeType { get; set; }
        public string EntityType { get; set; }
    }
}