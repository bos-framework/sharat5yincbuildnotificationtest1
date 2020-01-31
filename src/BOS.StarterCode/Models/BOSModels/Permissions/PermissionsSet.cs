using BOS.IA.Client.ClientModels;
using System;

namespace BOS.StarterCode.Models.BOSModels.Permissions
{
    public class PermissionsSet : IPermissionsSet
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public Guid ComponentId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string PagePath { get; set; }
        public bool IsDefault { get; set; }
        public bool IsLinkOnly { get; set; }
        public bool HasLandingPage { get; set; }
        public bool IsSite { get; set; }
        public bool IsParentUpdated { get; set; }
        public SetType Type { get; set; }
        public Guid? ParentId { get; set; }
    }
}
