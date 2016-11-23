using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartzExtScheduler.Entity.NotSignUpEntity
{
    /// <summary>
    /// 模板
    /// </summary>
    public enum Template
    {
        /// <summary>
        /// 入库＜10，报名次数＜50 模板id 微信
        /// </summary>
        Sl10rl50Wx = 437,
        /// <summary>
        /// 入库＜10，成功案例＜5，报名次数＞20 模板id 微信
        /// </summary>
        Sl10rg20al5Wx = 440,
        /// <summary>
        /// 入库≥10 模板id 微信
        /// </summary>
        Sg10Wx = 442,
        /// <summary>
        /// 成功案例＜5，报名＜20 模板id 微信
        /// </summary>
        Al5rl20Wx = 446,
        /// <summary>
        /// 成功案例＜5，报名＞=20 模板id 微信
        /// </summary>
        Al5rg20Wx = 447,
        /// <summary>
        /// 成功案例＞5，报名＜20次 模板id 微信
        /// </summary>
        Ag5rl20Wx = 450,
        /// <summary>
        /// 成功案例＞5，报名＞=20次 模板id 微信
        /// </summary>
        Ag5rg20Wx = 451,
        /// <summary>
        /// 成功案例＜5，报名＜20 模板id 短信
        /// </summary>
        Al5rl20Sms = 453,
        /// <summary>
        /// 成功案例＞5，报名＜20次 模板id 短信
        /// </summary>
        Ag5rl20Sms = 454,
        /// <summary>
        /// 成功案例＞5，报名＞=20次 模板id 短信
        /// </summary>
        Ag5rg20Sms = 455
    }
}
