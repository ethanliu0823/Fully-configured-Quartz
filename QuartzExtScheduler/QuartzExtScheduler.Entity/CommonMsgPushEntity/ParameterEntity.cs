using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartzExtScheduler.Entity.CommonMsgPushEntity
{
    /// <summary>
    /// 参数结构
    /// </summary>
    public class ParameterEntity
    {
        /// <summary>
        /// 参数
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 参数值来源
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 参数对应值
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// shorturl
        /// </summary>
        public bool ConvertShortURL { get; set; }
    }
}
