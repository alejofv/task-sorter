using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TaskSorter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var fileContents = ReadFile(args);

            var taskSet = await MyTaskSet.Build(fileContents);
            var sortedTasks = taskSet.Sort();

            PrintTasks(sortedTasks);
        }

        /// <summary>
        /// Reads the file contents.
        /// Use an IAsyncEnumerable so we can read line by line without loading the full file in memory.
        /// </summary>
        private static async IAsyncEnumerable<string> ReadFile(string[] args)
        {
            if (args?.Length != 1)
                throw new ArgumentException("Please specify an input file name");
            
            using (var reader = File.OpenText(args[0]))
            {
                while (!reader.EndOfStream)
                    yield return await reader.ReadLineAsync();
            }
        }

        /// <summary>
        /// Prints the prioritized tasks.
        /// For tasks that can be done at the same time, list them on the same line separated by commas
        /// </summary>
        private static void PrintTasks(List<(int Priority, List<MyTask> Tasks)> prioritizedTasks)
        {
            foreach (var tasks in prioritizedTasks.Select(x => x.Tasks))
                Console.WriteLine(string.Join(", ", tasks.Select(t => t.Name)));
        }
    }
}
