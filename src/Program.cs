using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TaskSorter.Domain;

namespace TaskSorter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args?.Length != 1)
                throw new ArgumentException("Please specify an input file name");

            var (input, output) = CreateAdapters(args);

            var fileContents = input.ReadInput();
            
            var taskSet = await TaskSet.Build(fileContents);
            var sortedTasks = taskSet.Sort();

            output.WriteOutput(sortedTasks);
        }

        private static (IInputAdapter, IOutputAdapter) CreateAdapters(string[] args)
            => (new FileInputAdapter(args[0]), new ConsoleOutputAdapter());
    }
}
