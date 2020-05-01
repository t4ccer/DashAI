using System;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace DashAI
{
    public class Game : IDisposable
    {
        public IMap map = new BmpMap(NeatConsts.MapName);
        public Player player;
        public bool hasEnded = false;
        public bool hasWon = false;
        bool drawInConsole = false;
        RenderWindow window;

        public double fitness => player.position.x/(double)(map.map.GetLength(1)-NeatConsts.ViewX);
        public Game(bool drawInConsole = false)
        {
            this.drawInConsole = drawInConsole;
            player = new Player(new Vector2(1, map.map.GetLength(0) - 4));
            if(drawInConsole)
                window = new RenderWindow(new VideoMode((uint)(NeatConsts.TileSize * map.map.GetLength(1)), (uint)(NeatConsts.TileSize * map.map.GetLength(0))), "DashAI");
        }
        public void Step(bool jump)
        {

            HandleJump(jump);
            player.position.x++;
            if (HandleWin())
                return;
            if (drawInConsole)
                DrawInSFML();
            HandleDeath();
        }

        private void HandleJump(bool jump)
        {
            switch (player.jumpPhase)
            {
                case 0:
                    if (jump && (map.map[player.position.y + 1, player.position.x] == 1 || map.map[player.position.y, player.position.x] == 3))
                    {
                        player.jumpPhase = 1;
                        player.position.y--;
                    }
                    break;
                case 1:
                    player.jumpPhase = 2;
                    player.position.y--;
                    break;
            }

            if (map.map[player.position.y + 1, player.position.x] != 1)
            {
                if (player.jumpPhase == 0)
                    player.position.y++;
            }
            if (player.jumpPhase == 2)
                player.jumpPhase = 0;

        }
        private void DrawConsoleMap()
        {
            Console.Clear();
            for (int y = 0; y < map.map.GetLength(0); y++)
            {
                for (int x = 0; x < map.map.GetLength(1); x++)
                {
                    if (player.position.x == x && player.position.y == y)
                    {
                        Console.Write("o ");
                        continue;
                    }

                    switch (map.map[y, x])
                    {
                        case 0:
                            Console.Write(' ');
                            break;
                        case 1:
                            Console.Write('x');
                            break;
                        case 2:
                            Console.Write('y');
                            break;
                    }
                    Console.Write(' ');
                }
                Console.WriteLine();
            }
        }
        private void DrawInSFML()
        {
            window.SetActive();
            window.Clear();
            for (int y = 0; y < map.map.GetLength(0); y++)
            {
                for (int x = 0; x < map.map.GetLength(1); x++)
                {
                    var rect = new RectangleShape(new Vector2f(NeatConsts.TileSize, NeatConsts.TileSize));
                    rect.FillColor = new Color(255, 255, 255);
                    rect.Position = new Vector2f(NeatConsts.TileSize * x, NeatConsts.TileSize * y);
                    if (player.position.x == x && player.position.y == y)
                    {
                        rect.FillColor = new Color(255, 175, 0);
                        window.Draw(rect);
                        continue;
                    }

                    switch (map.map[y, x])
                    {
                        case 0:
                            rect.FillColor = new Color(255, 255, 255);
                            break;
                        case 1:
                            rect.FillColor = new Color(0, 0, 0);
                            break;
                        case 2:
                            rect.FillColor = new Color(255, 0, 0);
                            break;
                        case 3:
                            rect.FillColor = new Color(0, 255, 0);
                            break;
                    }
                    window.Draw(rect);
                }
            }
            window.DispatchEvents();
            window.Display();
            if(NeatConsts.RecordPlay)
                window.Capture().SaveToFile($"{NeatConsts.experimentName}/step{player.position.x}.png");
        }
        private void HandleDeath()
        {
            if (map.map[player.position.y, player.position.x] == 2 || map.map[player.position.y, player.position.x] == 1)
                hasEnded = true;
        }
        private bool HandleWin()
        {
            if (player.position.x + NeatConsts.ViewX == map.map.GetLength(1))
            {
                hasEnded = true;
                hasWon = true;
                return true;
            }
            return false;
        }

        public void Dispose() => window?.Dispose();
    }
}
