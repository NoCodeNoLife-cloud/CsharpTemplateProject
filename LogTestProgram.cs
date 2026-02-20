using CustomSerilogImpl.InstanceVal.Service.Configuration;
using CustomSerilogImpl.InstanceVal.Service.Enums;
using CustomSerilogImpl.InstanceVal.Service.Services;

class LogTestProgram
{
    static void Main(string[] args)
    {
        // 配置日志系统使用 Serilog 实现
        var config = LoggingConfiguration.CreateDevelopment();
        config.DefaultImplementation = LoggingFactory.LoggingImplementation.Serilog;
        config.Apply();
        
        Console.WriteLine("=== 测试 Serilog 彩色日志输出 ===\n");
        
        // 测试各种级别的日志
        LoggingFactory.Instance.LogDebug("这是调试信息 - Debug message with some data: 123, \"hello world\"");
        LoggingFactory.Instance.LogInformation("应用程序启动成功 - Application started successfully");
        LoggingFactory.Instance.LogWarning("配置文件未找到，使用默认设置 - Config file not found, using defaults");
        LoggingFactory.Instance.LogError("数据库连接失败 - Database connection failed");
        
        try
        {
            throw new InvalidOperationException("模拟业务异常 - Simulated business exception");
        }
        catch (Exception ex)
        {
            LoggingFactory.Instance.LogError("处理用户请求时发生错误", ex);
        }
        
        LoggingFactory.Instance.LogCritical("系统遇到严重错误，需要立即处理 - Critical system error requiring immediate attention");
        
        // 测试结构化日志
        var userData = new { UserId = 12345, UserName = "张三", Action = "登录" };
        LoggingFactory.Instance.LogInformation($"用户操作记录: {userData.UserName}(ID:{userData.UserId}) 执行了 {userData.Action} 操作");
        
        var orderData = new { OrderId = "ORD-2024-001", Amount = 999.99, Status = "已支付" };
        LoggingFactory.Instance.LogDebug($"订单处理详情: 订单号={orderData.OrderId}, 金额=${orderData.Amount}, 状态={orderData.Status}");
        
        Console.WriteLine("\n=== 测试完成 ===");
        Console.WriteLine("请观察上方日志输出的颜色效果");
        Console.ReadKey();
    }
}