using Sym.Server.Screens;

namespace Sym.Server
{
    public class SymServer : SymServerBase
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            HomeScreen homeScreen = new HomeScreen();

            Add(homeScreen);
        }
    }
}
