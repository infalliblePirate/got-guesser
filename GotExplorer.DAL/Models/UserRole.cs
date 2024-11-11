using GotExplorer.DAL.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace GotExplorer.DAL.Models
{
    public class UserRole : IdentityRole<int>, IModel
    {
    }
}
