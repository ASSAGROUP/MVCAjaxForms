using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpencutInternal.Models
{
    public class AjaxResult
    {
        AjaxStatus _status;

        public AjaxResult(AjaxStatus status, string message)
        {
            Message = message;
            _status = status;
        }

        public string Message { get; set; }

        public string Status { get { return _status.ToString(); } }

        // remember to keep in sync the javascript version of this enum 
        public enum AjaxStatus
        {
            OK,
            ERROR,
            UNEXPECTED
        }
    }
}