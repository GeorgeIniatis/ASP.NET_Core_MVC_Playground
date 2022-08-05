using ASP.NET_Core_MVC_Playground.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_Playground.Models
{
    public class OwnerSpreadsheetViewModel
    {
        public string TestVariable { get; set; }
        public IFormFile SpreadsheetFile { get; set; }
    }
}
