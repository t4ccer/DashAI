using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.DistanceMetrics;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using SharpNeat.SpeciationStrategies;

namespace DashAI
{

    class Program
    {
        public static Queue<(IBlackBox phenome, uint gen, double fitness)> blackBoxes = new Queue<(IBlackBox phenome, uint gen, double fitness)>();
        static void Main()
        {
            if(NeatConsts.ShowDuringTraining)
                new Thread(() =>
                {
                    while (true)
                    {
                        if (blackBoxes.Count == 0)
                        {
                            Thread.Sleep(10);
                            continue;
                        }
                        using var game = new Game(true);
                        var data = blackBoxes.Dequeue();
                        Console.WriteLine($"Showing gen {data.gen} with fitness {data.fitness}");
                        var brain = new BlackBoxBrain(data.phenome, game);

                        while (!game.hasEnded)
                        {
                            brain.Step();
                            Thread.Sleep(200);
                        }
                    }
                }).Start();

            Train();
            Play();
        }
        private static void Play()
        {
            var neatGenomeFactory = new NeatGenomeFactory(NeatConsts.ViewX * NeatConsts.ViewY, 1);
            var activationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(1);
            var genomeDecoder = new NeatGenomeDecoder(activationScheme);
            XmlReader xr;
            while (true)
            {
                try
                {
                    xr = XmlReader.Create("best.xml");
                    break;
                }
                catch (Exception)
                {

                }
            }
            var genome = NeatGenomeXmlIO.ReadCompleteGenomeList(xr, false, neatGenomeFactory)[0];
            var phenome = genomeDecoder.Decode(genome);

            using var game = new Game(true);
            var brain = new BlackBoxBrain(phenome, game);

            while (!game.hasEnded)
            {
                brain.Step();
                Thread.Sleep(200);
            }
        }
        private static void Train()
        {
            var neatGenomeFactory = new NeatGenomeFactory(NeatConsts.ViewX * NeatConsts.ViewY, 1);
            var genomeList = neatGenomeFactory.CreateGenomeList(NeatConsts.SpecCount, 0);

            var eaParams = new NeatEvolutionAlgorithmParameters();
            eaParams.SpecieCount = NeatConsts.SpecCount;

            var distanceMetric = new ManhattanDistanceMetric(1.0, 0.0, 10.0);
            var parallelOptions = new ParallelOptions();
            var speciationStrategy = new ParallelKMeansClusteringStrategy<NeatGenome>(distanceMetric, parallelOptions);
            var complexityRegulationStrategy = new NullComplexityRegulationStrategy();

            var ea = new NeatEvolutionAlgorithm<NeatGenome>(eaParams, speciationStrategy, complexityRegulationStrategy);
            var activationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(1);
            var genomeDecoder = new NeatGenomeDecoder(activationScheme);
            var phenomeEvaluator = new GameEvaluator();
            var genomeListEvaluator = new ParallelGenomeListEvaluator<NeatGenome, IBlackBox>(genomeDecoder, phenomeEvaluator, parallelOptions);
            ea.Initialize(genomeListEvaluator, neatGenomeFactory, genomeList);
            ea.UpdateScheme = new UpdateScheme(NeatConsts.LogRate);
            ea.StartContinue();
            ea.UpdateEvent += Ea_UpdateEvent;
            //while (ea.RunState != RunState.Paused)
            //{

            //}
            Console.ReadLine();
            ea.Stop();
            Thread.Sleep(1000);
        }
        private static void Ea_UpdateEvent(object sender, EventArgs e)
        {
            NeatEvolutionAlgorithm<NeatGenome> _ea = (NeatEvolutionAlgorithm<NeatGenome>)sender;
            if(NeatConsts.ShowDuringTraining)
            {
                var activationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(1);
                var genomeDecoder = new NeatGenomeDecoder(activationScheme);
                var genome = _ea.CurrentChampGenome;
                var phenome = genomeDecoder.Decode(genome);
                blackBoxes.Enqueue((phenome, _ea.CurrentGeneration, _ea.Statistics._maxFitness));
            }
            else
                Console.WriteLine($"gen={_ea.CurrentGeneration:N0} bestFitness={_ea.Statistics._maxFitness:N6}, meanFitness={_ea.Statistics._meanFitness}");



            CreateDotFile(_ea);
            CreateXmlNetworkFile(_ea);
        }
        private static void CreateXmlNetworkFile(NeatEvolutionAlgorithm<NeatGenome> _ea)
        {
            var doc = NeatGenomeXmlIO.SaveComplete(new List<NeatGenome>() { _ea.CurrentChampGenome }, false);
            doc.Save("best.xml");
        }
        private static void CreateDotFile(NeatEvolutionAlgorithm<NeatGenome> _ea)
        {
            var names = new Dictionary<uint, string>();
            var sb = new StringBuilder($"digraph Gen{_ea.CurrentGeneration} {{\n");
            sb.AppendLine("node[width=1, shape=circle];");
            foreach (var neuron in _ea.CurrentChampGenome.NeuronGeneList)
            {
                names[neuron.Id] = neuron.NodeType.ToString() + neuron.Id.ToString();
                sb.Append($"\"{names[neuron.Id]}\"");
                switch (neuron.NodeType)
                {
                    case SharpNeat.Network.NodeType.Bias:
                        sb.AppendLine("[style=filled, fillcolor=white]");
                        break;
                    case SharpNeat.Network.NodeType.Input:
                        sb.AppendLine("[style=filled, fillcolor=yellow]");
                        break;
                    case SharpNeat.Network.NodeType.Output:
                        sb.AppendLine("[style=filled, fillcolor=red]");
                        break;
                    case SharpNeat.Network.NodeType.Hidden:
                        sb.AppendLine("[style=filled, fillcolor=blue]");
                        break;
                    default:
                        sb.AppendLine();
                        break;
                }
            }
            foreach (var connection in _ea.CurrentChampGenome.ConnectionGeneList)
            {
                sb.AppendLine($"\"{names[connection.SourceNodeId]}\" -> \"{names[connection.TargetNodeId]}\" [label=\"{connection.Weight.ToString("N3")}\"] [color={((connection.Weight > 0) ? "red" : "blue")}]");
            }
            sb.AppendLine("}");
            File.WriteAllText("best.gv", sb.ToString());
        }
    }
}
