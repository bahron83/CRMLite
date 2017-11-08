using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMLiteBusiness
{
    public class Installation : Activity
    {
        public virtual Contract Contract { get; set; }
    }
}