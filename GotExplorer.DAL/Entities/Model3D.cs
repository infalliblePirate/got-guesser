﻿using GotExplorer.DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace GotExplorer.DAL.Entities
{
    public class Model3D : IEntity
    {
        public int Id { get; set; }

        [Column(TypeName = "VARCHAR(100)")]
        public string Name { get; set; }

        [Column(TypeName = "VARCHAR(255)")]
        public string Path { get; set; }
    }
}
