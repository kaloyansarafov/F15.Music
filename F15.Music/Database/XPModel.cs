using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using MySQL.Data.EntityFrameworkCore;


namespace F15
{
    [Table("Xp")]
    public class Xp
    {
        [Column("DiscordID")]
        [Required()]
        [Key()]
        public string DiscordId { get; set; }
        
        [Column("XpAmount")]
        [Required()]
        public int XpAmount { get; set; }

    }
}
