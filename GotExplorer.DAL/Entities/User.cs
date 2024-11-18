using Microsoft.AspNetCore.Identity;
using GotExplorer.DAL.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
namespace GotExplorer.DAL.Entities
{
    public class User : IdentityUser<int>, IEntity<int>
    {
        [ForeignKey("Image")]
        public int ImageId { get; set; } = 1;
        public Image Image { get; set; }
    }
}
