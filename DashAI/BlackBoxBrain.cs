using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.Linq;

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

            foreach (int typeID in NeatConsts.typeIds)
            {
                for (int y = 0; y < NeatConsts.ViewY; y++)
                {
                    for (int x = 0; x < NeatConsts.ViewX; x++)
                    {
                        var xpos = game.player.position.x + x;
                        var ypos = game.player.position.y + (NeatConsts.ViewY/2) - y;
                        var index = typeID * (y * NeatConsts.ViewX + x);
                        if (ypos < 0 || xpos < 0 || ypos >= game.map.map.GetLength(0) || xpos >= game.map.map.GetLength(1))
                            phenome.InputSignalArray[index] = 0;
                        else
                            phenome.InputSignalArray[index] = (game.map.map[ypos, xpos]) == typeID ? 1 : 0;
                    }
                }
            }


            phenome.Activate();
            game.Step(phenome.OutputSignalArray[0] > 0.5);
        }
    }
}
