using SharpNeat.Phenomes;

namespace DashAI
{
    public class BlackBoxBrain : IBrain
    {
        IBlackBox phenome;
        Game game;

        public BlackBoxBrain(IBlackBox phenome, Game game)
        {
            this.phenome = phenome;
            this.game = game;
        }

        public double Run()
        {
            while (!game.hasEnded)
            {
                Step();
            }
            return game.fitness;
        }
        public void Step()
        {
            phenome.ResetState();

            for (int y = 0; y < NeatConsts.ViewY; y++)
            {
                for (int x = 0; x < NeatConsts.ViewX; x++)
                {
                    phenome.InputSignalArray[y * NeatConsts.ViewX + x] = game.map.map[game.player.position.y + 3 - y, game.player.position.x + x];
                }
            }
            phenome.Activate();
            game.Step(phenome.OutputSignalArray[0] > 0.5);
        }
    }
}
