using WebSocketDemo.Middleware;

namespace WebSocketDemo.Extensions;

public static class MiddlewareExtension
{
    public static IApplicationBuilder UseEddyMiddleware(this IApplicationBuilder builder)
    {
        // 使用UseMiddleware将自定义中间件添加到请求处理管道中
        return builder.UseMiddleware<MyWebsocketMiddleware>();
    }
}
