namespace Rubujo.YouTube.Utility.Extensions;

/// <summary>
/// Exception 的擴充方法
/// </summary>
public static class ExceptionExtension
{
    /// <summary>
    /// 取得例外訊息
    /// </summary>
    /// <param name="exception">Exception</param>
    /// <returns>字串</returns>
    public static string GetExceptionMessage(this Exception exception)
    {
        if (exception == null)
        {
            return string.Empty;
        }

        string exceptionMessage = exception.Message,
            innerExcpetionMessage = exception.GetInnerExceptionMessage();

        if (!string.IsNullOrEmpty(innerExcpetionMessage))
        {
            exceptionMessage = $"{exceptionMessage}{Environment.NewLine}{innerExcpetionMessage}";
        }

        return exceptionMessage;
    }

    /// <summary>
    /// 取得內部的例外訊息
    /// </summary>
    /// <param name="exception">Exception</param>
    /// <returns>字串</returns>
    public static string GetInnerExceptionMessage(this Exception exception)
    {
        if (exception == null)
        {
            return string.Empty;
        }

        Exception? innerException = exception.InnerException;

        if (innerException == null)
        {
            return string.Empty;
        }

        return innerException.Message;
    }
}