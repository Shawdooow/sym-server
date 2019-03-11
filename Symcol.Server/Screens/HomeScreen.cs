using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osuTK;
using osuTK.Graphics;
using Symcol.Base.Graphics.Containers;
using Symcol.Base.Graphics.UserInterface;
using Symcol.Server.Screens.Pieces;

namespace Symcol.Server.Screens
{
    public class HomeScreen : Screen
    {
        private readonly FillFlowContainer<GameItem> games;
        private readonly FillFlowContainer<PlayerItem> players;

        private readonly SymcolClickableContainer add;

        private readonly SymcolWindow addWindow;
        private readonly TextBox gameNameBox;
        private readonly TextBox gameIDBox;
        private readonly TextBox addressBox;

        private readonly SymcolWindow confirmWindow;

        public HomeScreen()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Blue
                },
                new SymcolContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.48f, 0.90f),
                    Position = new Vector2(8, 0),

                    Masking = true,
                    CornerRadius = 12,

                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.5f
                        },
                        new ScrollContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.98f),

                            Child = games = new FillFlowContainer<GameItem>
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.X,
                                Spacing = new Vector2(0, 4)
                            }
                        }
                    }
                },

                new SymcolContainer
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.5f, 0.90f),
                    Position = new Vector2(-8, 0),

                    Masking = true,
                    CornerRadius = 12,

                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.5f
                        },
                        new ScrollContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.98f),

                            Child = players = new FillFlowContainer<PlayerItem>
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.X,
                            }
                        }
                    }
                },
                new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.04f,

                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.25f
                        },
                        new FillFlowContainer<SymcolClickableContainer>
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.33f,

                            Children = new SymcolClickableContainer[]
                            {
                                add = new SymcolClickableContainer
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    Width = 40,
                                    Action = () => addWindow.Toggle(),

                                    Child = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4.Red.Opacity(0.5f)
                                    }
                                },
                                new SymcolClickableContainer
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    Width = 40,
                                    Action = () => storage.OpenInNativeExplorer(),

                                    Child = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4.Green.Opacity(0.5f)
                                    }
                                },
                            }
                        },
                        new FillFlowContainer<SymcolClickableContainer>
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.33f,
                        },
                        new FillFlowContainer<SymcolClickableContainer>
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.33f,
                        },
                    }
                },
                addWindow = new SymcolWindow(new Vector2(200, 240))
            };

            addWindow.WindowTitle.Text = "Add new Game";
            addWindow.WindowContent.AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.25f
                }, 

                new SpriteText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,

                    Text = "Game Name",
                    TextSize = 24
                },
                gameNameBox = new TextBox
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,

                    RelativeSizeAxes = Axes.X,
                    Height = 24,

                    Text = "",
                    Position = new Vector2(0, 20),
                },

                new SpriteText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,

                    Text = "Game ID",
                    TextSize = 24,
                    Position = new Vector2(0, 20 * 3),
                },
                gameIDBox = new TextBox
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,

                    RelativeSizeAxes = Axes.X,
                    Height = 24,

                    Text = "",
                    Position = new Vector2(0, 20 * 4),
                },

                new SpriteText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,

                    Text = "Game Address",
                    TextSize = 24,
                    Position = new Vector2(0, 20 * 6),
                },
                addressBox = new TextBox
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,

                    RelativeSizeAxes = Axes.X,
                    Height = 24,

                    Text = "",
                    Position = new Vector2(0, 20 * 7),
                },

                new SymcolClickableContainer
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,

                    RelativeSizeAxes = Axes.X,
                    Height = 20,
                    Action = () =>
                    {
                        games.Add(new GameItem(gameNameBox.Text, gameIDBox.Text, addressBox.Text));
                    },

                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Red.Opacity(0.2f)
                        },
                        new SpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,

                            Text = "Add Game",
                            TextSize = 18,
                        }
                    }
                } 
            });

            games.Add(new GameItem("osu", "osu", "10.0.0.108:25590"));
            //games.Add(new GameItem("osu", "osu", "10.0.0.25:25590"));
        }

        private Storage storage;

        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            this.storage = storage;
        }
    }
}
