using System;
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
            try
            {
                // Example FSM content
                var exampleFSM = @"
                    STATE initial _ ""Start"" : INITIAL;
                    STATE powered _ ""Main state"" : COMPOUND;
                    STATE on powered ""Lamp on"" : SIMPLE;
                    STATE off powered ""Lamp off"" : SIMPLE;
                    STATE final _ ""Done"" : FINAL;
                    TRIGGER switch ""Press the switch"";
                    ACTION turn_on ""Turn lamp on"" : ENTRY_ACTION;
                    ACTION turn_off ""Turn lamp off"" : EXIT_ACTION;
                    TRANSITION t1 initial -> off switch """";
                    TRANSITION t2 off -> on switch """";
                    TRANSITION t3 on -> off switch """";
                    TRANSITION t4 powered -> final switch """";
                ";

                // Build FSM
                var factory = new StateFactory();
                var builder = new FSMBuilder(factory);
                var parser = new FSMParser(builder);
                var model = parser.ParseFromString(exampleFSM);

                // Validate FSM
                var validator = new FSMValidator();
                var errors = validator.Validate(model);

                if (errors.Any())
                {
                    Console.WriteLine("=== VALIDATION ERRORS ===");
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"ERROR: {error}");
                    }
                    Console.WriteLine();
                }

                // Render FSM
                var renderer = new TextRenderer();
                Console.WriteLine(renderer.Render(model));

                // User options
                Console.WriteLine("Options:");
                Console.WriteLine("1. Run simulator");
                Console.WriteLine("2. Load from file");
                Console.WriteLine("3. Exit");
                Console.Write("Choose option (1-3): ");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        var simulator = new FSMSimulator(model, renderer);
                        simulator.Run();
                        break;
                    case "2":
                        Console.Write("Enter file path: ");
                        var filePath = Console.ReadLine();
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            var fileModel = parser.ParseFromFile(filePath);
                            Console.WriteLine(renderer.Render(fileModel));
                        }
                        break;
                    case "3":
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}