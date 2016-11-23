using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartzExtScheduler.Entity.CommonMsgPushEntity
{
    /// <summary>
    /// SQL
    /// </summary>
    public class SqlEntity
    {
        /// <summary>
        /// SQL标识
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// SQL 
        /// </summary>
        public string Sql_String { get; set; }
        /// <summary>
        /// 功能描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// SQL参数
        /// </summary>
        public List<ParameterEntity> Sql_Parameters { get; set; }
        /// <summary>
        /// 内容参数
        /// </summary>
        public List<ParameterEntity> Content_Parameters { get; set; }
    }
}
