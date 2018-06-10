using osu.Framework.Allocation;
using osu.Framework.Platform;
using Symcol.Server.Config;

namespace Symcol.Server
{
    public class SymcolServerBase : osu.Framework.Game
    {
        protected SymcolServerConfigManager SymcolServerConfigManager;

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));

        [BackgroundDependencyLoader]
        private void load()
        {
            dependencies.Cache(this);
            dependencies.Cache(SymcolServerConfigManager);

            //Window.CursorState = CursorState.Hidden;
            Window.Title = @"SymcolServer";
        }

        public override void SetHost(GameHost host)
        {
            if (SymcolServerConfigManager == null)
                SymcolServerConfigManager = new SymcolServerConfigManager(host.Storage);

            base.SetHost(host);
        }

        protected override void Dispose(bool isDisposing)
        {
            SymcolServerConfigManager?.Save();
            base.Dispose(isDisposing);
        }
    }
}
