using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace ExitPath.Server.Multiplayer
{
    public class RealmRunner : BackgroundService
    {
        private const int TPS = 20;
        private const int TickMS = 1000 / TPS;

        private readonly Realm realm;

        public RealmRunner(Realm realm)
        {
            this.realm = realm;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                await Task.Delay(TickMS, stoppingToken);
                await this.realm.Tick();
            }
        }
    }
}
