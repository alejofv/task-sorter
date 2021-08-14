using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskSorter
{
    public class MyTask
    {
        private List<MyTask> _dependencies;
        private int? _priority;

        public string Name { get; set; }

        /// <summary>
        /// The priority of this task, calculated as the maximum priority of the dependencies, plus one
        /// </summary>
        public int Priority
        {
            get
            {
                // Calculate priority recursively, but store it in a local variable so we don't have to calculate multiple times
                if (_priority == null)
                    _priority = (_dependencies.Any() ? _dependencies.Select(d => d.Priority).Max() : 0) + 1;

                 return _priority.Value;
            }
        }

        public MyTask(string name)
        {
            this.Name = name;
            this._dependencies = new List<MyTask>();
        }

        public void DependsOn(MyTask task)
        {
            this._dependencies.Add(task);
        }
    }

    public class MyTaskSet
    {
        private List<MyTask> _tasks;

        protected MyTaskSet(List<MyTask> tasks)
        {
            _tasks = tasks;
        }

        /// <summary>
        /// Builds a task set from a line collection.
        /// Each line consists of a dependency and the task that it is 
        /// dependent on it. The dependency and task will be separated by an arrow (denoted by "->").
        /// </summary>
        public static async Task<MyTaskSet> Build(IAsyncEnumerable<string> lines)
        {
            // Use a dictionary to lookup already added tasks
            var dict = new Dictionary<string, MyTask>();

            await foreach (var line in lines)
            {
                var parts = line.Split("->");
                if (parts.Length != 2)
                    throw new Exception("Invalid task line: " + line);
                
                if (!dict.TryGetValue(parts[0], out MyTask task))
                {
                    task = new MyTask(parts[0]);
                    dict.Add(task.Name, task);
                }

                if (!dict.TryGetValue(parts[1], out MyTask dependentTask))
                {
                    dependentTask = new MyTask(parts[1]);
                    dict.Add(dependentTask.Name, dependentTask);
                }

                dependentTask.DependsOn(task);
            }

            return new MyTaskSet(dict.Values.ToList());
        }

        /// <summary>
        /// Calculates the priority for each task in the set and returns a list of tasks sorted by priority.
        /// For tasks that can be done at the same time, sort them in alphabetical order
        /// </summary>
        public List<(int Priority, List<MyTask> Tasks)> Sort()
            => _tasks
                .GroupBy(x => x.Priority)
                .Select(g => (Priority: g.Key, g.OrderBy(t => t.Name).ToList()))
                .OrderBy(t => t.Priority)
                .ToList();
    }
}