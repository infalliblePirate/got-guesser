using Microsoft.AspNetCore.Identity;
using GotExplorer.DAL.Interfaces;
namespace GotExplorer.DAL.Models
{
    public class User : IdentityUser<int>, IModel
    {
    }
}
