using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartzExtScheduler.Entity.CommonDataCleanerEntity
{
    public class Item
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public IList<Var> Variables { get; set; }

        public IList<Selector> Selectors { get; set; }

        public Target Target { get; set; }
    }
}
