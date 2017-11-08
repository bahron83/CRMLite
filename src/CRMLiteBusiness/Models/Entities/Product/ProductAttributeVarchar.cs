using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMLiteBusiness
{
    public class ProductAttributeVarchar : ProductAttribute<string>
    {
        public override EavAttribute Attribute { get; set; }
        public override string Value { get; set; }
    }
}