using GotExplorer.DAL.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GotExplorer.DAL.Entities
{
    public class Level : IEntity
    {
        public int Id { get; set; }

        [Column(TypeName = "VARCHAR(200)")]
        public string Name { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public List<Model3D> Models { get; set; } = new List<Model3D>();

        [Timestamp]
        public uint Version { get; set; }
    }
}
