using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMLiteBusiness
{
    public abstract class ProductAttribute<T>
    {
        public int ProductID { get; set; }
        public int AttributeID { get; set; }

        public virtual Product Product { get; set; }
        public virtual EavAttribute Attribute { get; set; }
        public abstract T Value { get; set; }
    }
}