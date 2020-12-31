using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace ExitPath.Server.Multiplayer
{
    public class RealmRunner : BackgroundService
    {
        private const int TickMS = 1000 / Realm.TPS;

        private readonly Realm realm;

        public RealmRunner(Realm realm)
        {
            this.realm = realm;
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
                await this.realm.Tick();
            }
        }
    }
}
