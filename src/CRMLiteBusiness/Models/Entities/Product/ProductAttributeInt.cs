using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMLiteBusiness
{
    public class ProductAttributeInt : ProductAttribute<int>
    {
        public override EavAttribute Attribute { get; set; }
        public override int Value { get; set; }
    }
}