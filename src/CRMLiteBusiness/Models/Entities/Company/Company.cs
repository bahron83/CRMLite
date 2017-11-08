using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMLiteBusiness
{
    public enum Group { Customers, Leads, Competitors }
    public enum NoEmployees { Low, Mid, High }
    public enum Turnover { Low, Mid, High }

    public class Company
    {
        public int CompanyID { get; set; }
        public string Name { get; set; }
        public string VatNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        public Turnover? Turnover { get; set; }
        public NoEmployees? NoEmployees { get; set; }
        public string Industry { get; set; }        
        public Group? Group { get; set; }

        public virtual ICollection<Contact> Contacts { get; set; }
        public virtual ICollection<Contract> Contracts { get; set; }
        public virtual ICollection<Activity> Activities { get; set; }
    }

    public class CompanyWithContactCount : Company
    {
        public int ContactCount { get; set; }
    }
}