using SharpNeat.Core;
using SharpNeat.Phenomes;

namespace DashAI
{
    public class GameEvaluator : IPhenomeEvaluator<IBlackBox>
    {
        public ulong EvaluationCount => 0;

        public bool StopConditionSatisfied => shouldEnd;
        bool shouldEnd = false;
        public FitnessInfo Evaluate(IBlackBox phenome)
        {
            Game game = new Game();

            var fitness = new BlackBoxBrain(phenome, game).Run();
            if (game.hasWon)
            {
                shouldEnd = true;
            }
            return new FitnessInfo(fitness, fitness);
        }
        public void Reset() { }
    }
}
