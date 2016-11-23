using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartzExtScheduler.Entity.CommonDataCleanerEntity
{
    public class Target
    {
        public string TableName { get; set; }

        public string BeforeSql { get; set; }

        public string AfterSql { get; set; }

        public string MainSql { get; set; }
    }
}
