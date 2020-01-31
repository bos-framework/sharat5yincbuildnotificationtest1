using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BOS.StarterCode.Helpers.MultiTenancy
{
    public class WhiteLabel
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public string Logo { get; set; }
        public string ThemeCss { get; set; }
        public string CopyrightText { get; set; }
        public string Template { get; set; }
        public string JavaScriptUrl { get; set; }
    }
}
