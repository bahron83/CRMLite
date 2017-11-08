using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMLiteBusiness
{
    public enum Role
    {
        Admin, Sales, IT
    }

    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Role? Role { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }

        public virtual ICollection<Activity> Activities { get; set; }
    }
}