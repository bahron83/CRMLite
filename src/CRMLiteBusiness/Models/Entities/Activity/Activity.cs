using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMLiteBusiness
{
    public abstract class Activity
    {        
        public int ActivityID { get; set; }
        public int CompanyID { get; set; }
        public int ContactID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCompleted { get; set; }
        public string Notes { get; set; }

        public virtual Company Company { get; set; }
        public virtual Contact Contact { get; set; }
        public virtual User AssignedUser { get; set; }
    }
}