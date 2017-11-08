using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMLiteBusiness
{
    public class EavAttributeOption
    {
        public int OptionId { get; set; }
        public string OptionLabel { get; set; }
        public virtual EavAttribute Attribute { get; set; }
        public int Value { get; set; }
    }
}