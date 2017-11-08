using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMLiteBusiness
{
    public enum ContractType
    {
        Connectivity, Phone, Assistance
    }

    public class Contract
    {
        public int ContractID { get; set; }
        public int CompanyID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ContractType? ContractType { get; set; }
        public double Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Notes { get; set; }

        public virtual Company Company { get; set; }
        public virtual ICollection<ContractItem> ContractItems { get; set; }
    }
}