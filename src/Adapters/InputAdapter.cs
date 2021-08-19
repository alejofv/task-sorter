using System;
using System.Collections.Generic;
using System.IO;
using TaskSorter.Domain;

namespace TaskSorter
{
    public interface IInputAdapter
    {
        /// <summary>
        /// Reads a collection of task pairs (task and dependency) from the input
        /// </summary>
        IAsyncEnumerable<TaskNames> ReadInput();
    }

    // Default implementation - Read from file
    
    public class FileInputAdapter : IInputAdapter
    {
        private readonly string _fileName;

        public FileInputAdapter(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException($"'{nameof(fileName)}' cannot be null or whitespace.", nameof(fileName));

            _fileName = fileName;
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<TaskNames> ReadInput()
        {
            // Use an IAsyncEnumerable so we can read line by line without loading the full file in memory.
            using (var reader = File.OpenText(_fileName))
            {
                while (!reader.EndOfStream)
                {
                    // Each line consists of a dependency and the task that it is 
                    // dependent on it. The dependency and task will be separated by an arrow (denoted by "->").
                    
                    var line = await reader.ReadLineAsync();
                    
                    var names = line.Split("->");
                    if (names.Length != 2)
                        throw new ArgumentException("Invalid text line: " + line);

                    yield return new TaskNames(names[0], names[1]);
                }
            }
        }
    }
}