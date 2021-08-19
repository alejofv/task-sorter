using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TaskSorter.Domain;

namespace TaskSorter
{
    public interface IOutputAdapter
    {
        /// <summary>
        /// Outputs a sorted task set to external medium
        /// </summary>
        void WriteOutput(IList<SortedTasks> sortedTasks);
    }

    public record SortedTasks (int Priority, List<MyTask> Tasks);

    // Default implementation - Console 

    public class ConsoleOutputAdapter : IOutputAdapter
    {
        /// <inheritdoc />
        public void WriteOutput(IList<SortedTasks> sortedTasks)
        {
            // For tasks that can be done at the same time, list them on the same line separated by commas
            foreach (var tasks in sortedTasks.Select(x => x.Tasks))
                Console.WriteLine(string.Join(", ", tasks.Select(t => t.Name)));
        }
    }
}