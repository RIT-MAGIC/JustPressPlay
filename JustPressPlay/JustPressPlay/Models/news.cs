//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace JustPressPlay.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class news
    {
        public int id { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public string image { get; set; }
        public System.DateTime created_date { get; set; }
        public bool active { get; set; }
        public int creator_id { get; set; }
    
        public virtual user creator_user_id { get; set; }
    }
}
