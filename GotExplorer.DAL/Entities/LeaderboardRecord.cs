using GotExplorer.DAL.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GotExplorer.DAL.Entities
{
    public class LeaderboardRecord : IEntity<int>
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
        
        public int Score { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}