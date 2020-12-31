using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExitPath.Server.Multiplayer
{
    public class RealmRunner : BackgroundService
    {
        private const int TickMS = 1000 / Realm.TPS;

        private readonly Realm realm;
        private readonly ILogger<RealmRunner> logger;

        public RealmRunner(Realm realm, ILogger<RealmRunner> logger)
        {
            this.realm = realm;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.WhenAll(this.TickRealm(stoppingToken), this.realm.PumpMessages(stoppingToken));
        }

        private async Task TickRealm(CancellationToken token)
        {
            while (true)
            {
                await Task.Delay(TickMS, token);
                try
                {
                    await this.realm.Tick();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error when ticking realm");
                }
            }
        }
    }
}
