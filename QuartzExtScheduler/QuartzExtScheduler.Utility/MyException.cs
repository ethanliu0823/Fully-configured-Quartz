using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuartzExtScheduler.Utility
{
    /// <summary>
    /// 公共异常类
    /// <remarks>所有该类的实例都会记录日志</remarks>
    /// </summary>
    public class MyException : System.Exception
    {
        /// <summary>
        /// 所有公用的日志对象
        /// 在异常类实例化的时候会写日志
        /// </summary>
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(MyException));

        /// <summary>
        /// 实例化一个异常类对象，会自动写入文件日志
        /// </summary>
        /// <param name="message">与当前异常相关的消息</param>
        private MyException(string message) :
            base(message)
        {
            Log.Error(message, this);
        }

        /// <summary>
        /// 实例化MyException
        /// </summary>
        /// <summary>会自动写入异常日志</summary>
        /// <param name="message">异常消息</param>
        /// <param name="innerException">内部异常</param>
        private MyException(string message, System.Exception innerException)
            : base(message, innerException)
        {
            Log.Error(message, this);
        }

        /// <summary>
        /// 获取MyException
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <returns>MyException实例</returns>
        public static System.Exception GetMyExceptionWrapper(string message)
        {
            return new MyException(message);
        }

        /// <summary>
        /// 获取MyException
        /// </summary>
        /// <remarks>如果异常本身就是MyException类型，则直接返回异常</remarks>
        /// <param name="ex">异常</param>
        /// <returns>MyException实例</returns>
        public static System.Exception GetMyExceptionWrapper(System.Exception ex)
        {
            return (ex is MyException) ? ex : new MyException(ex.Message, ex);
        }

        /// <summary>
        /// 获取MyException
        /// </summary>
        /// <remarks>如果异常本身就是MyException类型，则直接返回异常</remarks>
        /// <param name="message">异常消息</param>
        /// <param name="ex">异常</param>
        /// <returns>MyException实例</returns>
        public static System.Exception GetMyExceptionWrapper(string message, System.Exception ex)
        {
            return (ex is MyException) ? ex : new MyException(message, ex);
        }

        /// <summary>
        /// 写异常信息
        /// </summary>
        /// <param name="ex">异常</param>
        public static void WriteLog(System.Exception ex)
        {
            if (!(ex is MyException)) Log.Error(ex.Message, ex);
        }

        /// <summary>
        /// 写异常信息
        /// </summary>
        /// <param name="msg">与当前异常相关的消息</param>
        /// <param name="ex">异常</param>
        public static void WriteLog(string msg, System.Exception ex)
        {
            if (!(ex is MyException)) Log.Error(msg, ex);
        }
    }
}
