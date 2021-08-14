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

        public MyTask DependsOn(MyTask task)
        {
            this._dependencies.Add(task);
            // allow chaining for fluent-like calls in UnitTests :)
            return this;
        }
    }

    public class TaskSet
    {
        private List<MyTask> _tasks;

        // Used by UnitTests to verify the set
        public int Count => _tasks.Count;

        protected TaskSet(List<MyTask> tasks)
        {
            _tasks = tasks;
        }

        /// <summary>
        /// Builds a task set from a text line collection, adding dependencies between tasks
        /// </summary>
        public static async Task<TaskSet> Build(IAsyncEnumerable<string> lines)
        {
            // Use a dictionary to lookup already added tasks
            var dict = new Dictionary<string, MyTask>();

            await foreach (var line in lines)
            {
                // Add task dependency
                var (task, dependentTask) = GetTasks(line, dict);
                dependentTask.DependsOn(task);
            }

            return new TaskSet(dict.Values.ToList());
        }

        /// <summary>
        /// Returns the task and the task it depends on, parsing a text line
        /// Each line consists of a dependency and the task that it is 
        /// dependent on it. The dependency and task will be separated by an arrow (denoted by "->").
        /// </summary>
        private static (MyTask Task, MyTask DependentTask) GetTasks(string line, Dictionary<string, MyTask> taskDictionary)
        {
            var names = line.Split("->");
            if (names.Length != 2)
                throw new ArgumentException("Invalid text line: " + line);
                
            if (!taskDictionary.TryGetValue(names[0], out MyTask task))
            {
                task = new MyTask(names[0]);
                taskDictionary.Add(task.Name, task);
            }

            if (!taskDictionary.TryGetValue(names[1], out MyTask dependentTask))
            {
                dependentTask = new MyTask(names[1]);
                taskDictionary.Add(dependentTask.Name, dependentTask);
            }

            return (task, dependentTask);
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