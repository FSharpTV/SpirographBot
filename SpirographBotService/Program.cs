using SpirographBot;
using Topshelf;
using NLog;

namespace SpirographBotService
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            logger.Info("Starting Spirograph Robobot Service");

            HostFactory.Run(x =>
            {
                x.Service<Bot>(service =>
                {
                    service.ConstructUsing(name => new Bot());
                    service.WhenStarted(bot => bot.Start());
                    service.WhenStopped(bot => bot.Stop());
                });

                x.EnableServiceRecovery(r =>
                {
                    r.RestartService(1);
                });

                x.RunAsLocalSystem();

                x.SetDescription("Spirograph Twitter Robobot");
                x.SetDisplayName("SpirographRobobot");
                x.SetServiceName("SpirographRobobot");
            });
        }
    }
}
