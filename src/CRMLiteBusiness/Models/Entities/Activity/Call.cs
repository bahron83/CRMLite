using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CRMLiteBusiness
{
    public enum CallResult { NotInterested, ToRecall, SetVisit }

    public class Call : Activity
    {
        public CallResult? Result { get; set; }
        public DateTime NextCall { get; set; }

        public virtual Call RefCall { get; set; }
    }

    //is next call is set, a new Call instance is created with StartDate = NextCall
}