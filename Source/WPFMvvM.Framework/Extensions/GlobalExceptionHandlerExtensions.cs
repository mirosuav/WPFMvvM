
namespace WPFMvvM.Framework.Exceptions;

public static class GlobalExceptionHandlerExtensions
{
    public static void HandleError(this IExceptionHandler handler, string message, Exception ex) => handler.Handle(LogLevel.Error, message, ex);
    public static void HandleCritical(this IExceptionHandler handler, string message, Exception ex) => handler.Handle(LogLevel.Critical, message, ex);

}
