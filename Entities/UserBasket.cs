using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlioDesktopApp.Entities
{
    public class UserBasket
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public double Rate { get; set; } = 5.0;
        public int Desc { get; set; } = 0;
        public decimal Price { get; set; }
    }
}
