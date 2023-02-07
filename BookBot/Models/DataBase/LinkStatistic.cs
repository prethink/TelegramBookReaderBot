using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookBot.Models.DataBase
{
    [Table("link_statistic")]
    public class LinkStatistic
    {
        [Column("id")]
        public long Id { get; set; }
        [Column("link")]
        public string Link { get; set; }
        [Column("description")]
        public string Description { get; set; }
        [Column("reg_count")]
        public long RegCount { get; set; }
    }
}
