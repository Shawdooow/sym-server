using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.States;
using OpenTK;
using OpenTK.Graphics;
using Symcol.Core.Graphics.Containers;
using Symcol.Core.Networking;
using Symcol.Server.Mods;
using Symcol.Server.Networking;

namespace Symcol.Server.Screens.Pieces
{
    public class GameItem : SymcolClickableContainer
    {
        private readonly Box box;

        public readonly ServerNetworkingClientHandler Server;

        public GameItem(string name, string id, string address)
        {
            RelativeSizeAxes = Axes.X;
            Height = 100;

            Masking = true;
            BorderThickness = 4;
            BorderColour = Color4.White;
            CornerRadius = 16;

            Modset mod = ModStore.GetModset(name);
            if (mod != null)
            {
                Server = mod.GetServerNetworkingClientHandler();
                Server.ClientType = ClientType.Server;
                Server.Address = address;
            }
            else
            {
                Server = new ServerNetworkingClientHandler
                {
                    ClientType = ClientType.Server,
                    Address = address
                };
            }
            Server.ClientInfo.Gamekey = id;
            Server.RunningGame = new GameInfo
            {
                Name = name,
                Gamekey = id,
                //MaxPlayers = players
            };

            Children = new Drawable[]
            {
                Server,
                box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.5f
                },
                new SpriteText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,

                    Position = new Vector2(10, 2),

                    Text = name,
                    TextSize = 40
                }
            };

            Action = () =>
            {
                AlwaysPresent = true;
                this.FadeOutFromOne(200)
                    .Finally(f => Expire());
            };
        }

        protected override bool OnHover(InputState state)
        {
            box.FadeTo(0.2f);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            base.OnHoverLost(state);
            box.FadeTo(0.5f);
        }
    }
}
