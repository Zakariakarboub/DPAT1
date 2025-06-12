using System;
using System.IO;
using System.Linq;
using FSMViewer.Model;
using FSMViewer.Factory;
using FSMViewer.Builder;
using FSMViewer.Parser;
using FSMViewer.Validation;
using FSMViewer.Renderer;
using FSMViewer.Simulation;

namespace FSMViewer
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new StateFactory();
            var validator = new FSMValidator();
            var renderer = new TextRenderer();

            FSMModel? loadedModel = null;

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("1. Run simulator");
                Console.WriteLine("2. Load from file");
                Console.WriteLine("3. Exit");
                Console.Write("Choose option (1-3): ");
                var choice = Console.ReadLine()?.Trim();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        if (loadedModel == null)
                        {
                            Console.WriteLine("Geen FSM geladen! Kies eerst optie 2 om in te laden.");
                            break;
                        }
                        var simulator = new FSMSimulator(loadedModel, renderer);
                        simulator.Run();
                        break;

                    case "2":
                        Console.Write("Enter file name: ");
                        var input = Console.ReadLine()?.Trim('"').Trim();
                        Console.WriteLine();

                        if (string.IsNullOrEmpty(input))
                        {
                            Console.WriteLine("Geen bestandsnaam opgegeven.");
                            break;
                        }

                        // Zoek in heel project
                        var matches = Directory.GetFiles(Directory.GetCurrentDirectory(), input, SearchOption.AllDirectories);
                        if (matches.Length == 0)
                        {
                            Console.WriteLine($"Error: bestand niet gevonden: {input}");
                            break;
                        }
                        var path = matches[0];
                        Console.WriteLine($"Attempting to load FSM from: {path}");

                        try
                        {
                            var builder = new FSMBuilder(factory);
                            var parser = new FSMParser(builder);
                            var model = parser.ParseFromFile(path);

                            var errors = validator.Validate(model);
                            if (errors.Any())
                            {
                                Console.WriteLine("=== VALIDATION ERRORS ===");
                                errors.ForEach(err => Console.WriteLine($"ERROR: {err}"));
                                Console.WriteLine();
                            }

                            Console.WriteLine("=== FSM MODEL ===");
                            Console.WriteLine(renderer.Render(model));
                            Console.WriteLine();

                            loadedModel = model;
                            Console.WriteLine(">> FSM succesvol geladen. Kies nu optie 1 om te simuleren.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error tijdens laden/parsen: {ex.Message}");
                        }
                        break;

                    case "3":
                        return;

                    default:
                        Console.WriteLine("Ongeldige keuze, probeer opnieuw.");
                        break;
                }
            }
        }
    }
}
