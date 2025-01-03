using MediatR;

namespace Instally.App.Application
{
    public static class Master
    {
        public static IServiceProvider ServiceProvider { get; set; }
        public static IMediator Mediator { get; set; }
        public static IConfiguration Config { get; set; }
    }
}
