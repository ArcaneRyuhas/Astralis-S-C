//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataAccessProject
{
    using System;
    using System.Collections.Generic;
    
    public partial class Plays
    {
        public string nickName { get; set; }
        public string gameId { get; set; }
        public Nullable<int> team { get; set; }
        public int playId { get; set; }
    
        public virtual Game Game { get; set; }
        public virtual User User { get; set; }
    }
}