using osu.Framework.Allocation;
using osu.Framework.Platform;
using Sym.Server.Config;

namespace Sym.Server
{
    public class SymServerBase : osu.Framework.Game
    {
        protected SymcolServerConfigManager SymServerConfigManager;

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load()
        {
            dependencies.Cache(this);
            dependencies.Cache(SymServerConfigManager);

            //Window.CursorState = CursorState.Hidden;
            Window.Title = @"Sym Server";
        }

        public override void SetHost(GameHost host)
        {
            if (SymServerConfigManager == null)
                SymServerConfigManager = new SymcolServerConfigManager(host.Storage);

            base.SetHost(host);
        }

        protected override void Dispose(bool isDisposing)
        {
            SymServerConfigManager?.Save();
            base.Dispose(isDisposing);
        }
    }
}
