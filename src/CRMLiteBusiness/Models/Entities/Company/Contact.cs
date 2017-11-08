using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMLiteBusiness
{
    public class Contact
    {
        public int ContactID { get; set; }
        public int CompanyID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Mobile1 { get; set; }
        public string Mobile2 { get; set; }
        public string Email { get; set; }
        public Role? Role { get; set; }

        public virtual Company Company { get; set; }
        public virtual ICollection<Activity> Activities { get; set; }
    }
}