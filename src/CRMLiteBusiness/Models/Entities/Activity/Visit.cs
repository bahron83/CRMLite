using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMLiteBusiness
{
    public enum VisitResult { NotInterested, ToRecall, ContractSet }

    public class Visit : Activity
    {
        public VisitResult? Result { get; set; }

        public virtual Call ReferenceCall { get; set; }
    }
}