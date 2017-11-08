using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMLiteBusiness
{
    public class ProductAttributeDecimal : ProductAttribute<double>
    {
        public override EavAttribute Attribute { get; set; }
        public override double Value { get; set; }
    }
}