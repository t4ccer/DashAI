using System;

namespace DashAI
{
    public class Player
    {
        public Vector2 position;
        public int jumpPhase = 0;

        public Player(Vector2 position)
        {
            this.position = position ?? throw new ArgumentNullException(nameof(position));
        }
    }
}
