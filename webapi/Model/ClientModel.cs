using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Model
{
    public class ClientModel
    {
        public Guid APIKEY { get; set; }
        public string URL { get; set; }
        public string RedirectURL { get; set; }
        public string scope { get; set; }
        public string State { get; set; }
    }
}
