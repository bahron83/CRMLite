using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMLiteBusiness
{
    public class ContractItem : Product
    {
        public int ContractID { get; set; }

        public virtual Contract Contract { get; set; }
    }
}