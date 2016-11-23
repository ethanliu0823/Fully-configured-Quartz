using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartzExtScheduler.Entity.CommonDataCleanerEntity
{
    public class Task
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public IList<Item> Items { get; set; }
    }
}
