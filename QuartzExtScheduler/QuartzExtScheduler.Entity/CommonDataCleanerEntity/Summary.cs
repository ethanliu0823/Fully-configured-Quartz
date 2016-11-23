using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartzExtScheduler.Entity.CommonDataCleanerEntity
{
    public class SummaryTask
    {
        public string Name { get; set; }

        public string ChildPath { get; set; }

        public string Description { get; set; }

        public string CronExpression { get; set; }

        public string ConnectionName { get; set; }
    }
}
