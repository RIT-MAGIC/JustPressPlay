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
    
    public partial class achievement_template
    {
        public achievement_template()
        {
            this.achievement_template_children = new HashSet<achievement_template>();
            this.achievement_requirement = new HashSet<achievement_requirement>();
            this.achievement_caretaker = new HashSet<achievement_caretaker>();
            this.achievement_user_content_pending = new HashSet<achievement_user_content_pending>();
            this.achievement_instance = new HashSet<achievement_instance>();
            this.achievement_keyword = new HashSet<achievement_keyword>();
            this.quest_achievement_step = new HashSet<quest_achievement_step>();
        }
    
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
        public string icon_file_name { get; set; }
        public int type { get; set; }
        public bool featured { get; set; }
        public bool hidden { get; set; }
        public bool is_repeatable { get; set; }
        public int state { get; set; }
        public Nullable<int> parent_id { get; set; }
        public Nullable<int> threshold { get; set; }
        public int creator_id { get; set; }
        public System.DateTime created_date { get; set; }
        public Nullable<System.DateTime> posted_date { get; set; }
        public Nullable<System.DateTime> retire_date { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public Nullable<int> last_modified_by_id { get; set; }
        public Nullable<int> content_type { get; set; }
        public Nullable<int> system_trigger_type { get; set; }
        public Nullable<int> repeat_delay_days { get; set; }
        public int points_create { get; set; }
        public int points_explore { get; set; }
        public int points_learn { get; set; }
        public int points_socialize { get; set; }
        public string keywords { get; set; }
    
        public virtual user creator { get; set; }
        public virtual user last_modified_by { get; set; }
        public virtual ICollection<achievement_template> achievement_template_children { get; set; }
        public virtual achievement_template achievement_template_parent { get; set; }
        public virtual ICollection<achievement_requirement> achievement_requirement { get; set; }
        public virtual ICollection<achievement_caretaker> achievement_caretaker { get; set; }
        public virtual ICollection<achievement_user_content_pending> achievement_user_content_pending { get; set; }
        public virtual ICollection<achievement_instance> achievement_instance { get; set; }
        public virtual ICollection<achievement_keyword> achievement_keyword { get; set; }
        public virtual ICollection<quest_achievement_step> quest_achievement_step { get; set; }
    }
}
