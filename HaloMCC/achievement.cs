//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HaloMCC
{
    using System;
    using System.Collections.Generic;
    
    public partial class achievement
    {
        public short Id { get; set; }
        public string name { get; set; }
        public byte category { get; set; }
        public byte game { get; set; }
        public string levelmap { get; set; }
        public string reward { get; set; }
    
        public virtual category category1 { get; set; }
    }
}
