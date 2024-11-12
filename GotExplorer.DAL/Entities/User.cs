using Microsoft.AspNetCore.Identity;
using GotExplorer.DAL.Interfaces;
using System.ComponentModel;
namespace GotExplorer.DAL.Entities
{
    public class User : IdentityUser<int>, IEntity
    {
        public int ImageId { get; set; } = 1;
        public Image Image { get; set; }
    }
}
