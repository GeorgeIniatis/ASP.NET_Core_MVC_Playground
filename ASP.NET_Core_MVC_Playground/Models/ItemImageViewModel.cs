using ASP.NET_Core_MVC_Playground.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_Playground.Models
{
    public class ItemImageViewModel
    {
        public Item Item { get; set; }

        [Display(Name = "Image")]
        public string ImageFile { get; set; }

        [Display(Name = "Description")]
        public string TextFile { get; set; }

    }
}
