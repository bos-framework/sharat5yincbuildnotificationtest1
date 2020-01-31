using BOS.Base.Client;
using BOS.IA.Client.ClientModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BOS.StarterCode.Models.BOSModels
{
    public class Module : IModule
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsDefault { get; set; }
        public bool IsPermitted { get; set; } = false;
        public string PagePath { get; set; }
        public bool HasLandingPage { get; set; }
        public bool IsSite { get; set; }
        public bool IsParentUpdated { get; set; }
        public bool Deleted { get; set; }
        public bool IsLinkOnly { get; set; }
        public Guid? ParentId { get; set; }
        public IModule ParentModule { get; set; } = null;
        [JsonProperty("Operations", ItemConverterType = typeof(ConcreteConverter<IOperation, Operation>))]
        public List<IOperation> Operations { get; set; }
        [JsonProperty("ChildComponents", ItemConverterType = typeof(ConcreteConverter<IModule, Module>))]
        public List<IModule> ChildComponents { get; set; } = new List<IModule>();
    }
}
