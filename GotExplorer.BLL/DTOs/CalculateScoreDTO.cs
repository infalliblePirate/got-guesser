using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GotExplorer.BLL.DTOs
{
    public class CalculateScoreDTO
    {
        public int GameId { get; set; }
        public int LevelId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
}