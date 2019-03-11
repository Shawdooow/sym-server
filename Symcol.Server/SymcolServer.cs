using Symcol.Server.Screens;

namespace Symcol.Server
{
    public class SymcolServer : SymcolServerBase
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            HomeScreen homeScreen = new HomeScreen();

            Add(homeScreen);
        }
    }
}
