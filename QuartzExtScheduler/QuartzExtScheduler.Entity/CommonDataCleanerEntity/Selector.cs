using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartzExtScheduler.Entity.CommonDataCleanerEntity
{
    public class Selector
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string DayOfWeek { get; set; }

        public SelectorType Type { get; set; }

        public string InnerSql { get; set; }
    }
}
