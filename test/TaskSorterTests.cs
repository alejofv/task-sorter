using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskSorter;
using TaskSorter.Domain;

namespace test
{
    [TestClass]
    public class TaskSorterTests
    {
        [TestMethod]
        public void TaskPriority_GivenNoDependencies_EqualsToOne()
        {
            var t1 = new MyTask("t1");
            Assert.AreEqual(1, t1.Priority);
        }

        [TestMethod]
        public void TaskPriority_GivenSingleDependency_EqualsToDependentPlusOne()
        {
            var t1 = new MyTask("t1");
            var t2 = new MyTask("t2");

            t2.DependsOn(t1);

            Assert.AreEqual(2, t2.Priority);
        }

        [TestMethod]
        public void TaskPriority_GivenMultipleDependencies_EqualsToMaxDependentPlusOne()
        {
            var t1 = new MyTask("t1");
            var t2 = new MyTask("t2");
            var t3 = new MyTask("t3");
            var tfinal = new MyTask("tfinal");

            // t1 -> t2 -> tfinal
            // t3 -> tfinal
            tfinal
                .DependsOn(
                    t2.DependsOn(t1))
                .DependsOn(t3);

            Assert.AreEqual(1, t1.Priority);
            Assert.AreEqual(1, t3.Priority);
            Assert.AreEqual(2, t2.Priority);
            Assert.AreEqual(3, tfinal.Priority);
        }

        [TestMethod]
        public async Task BuildTaskSet_GivenTasks_ReturnsOk()
        {
            var lines = Generator("t1->t2", "t2->t3", "t3->t4");
            var taskSet = await TaskSet.Build(lines);

            Assert.AreEqual(4, taskSet.Count);
        }

        [TestMethod]
        public async Task SortTaskSet_GivenTasks_ReturnsOk()
        {
            var lines = Generator("t1->t2", "t2->t3");
            var taskSet = await TaskSet.Build(lines);
            var sorted = taskSet.Sort();

            Assert.AreEqual(3, sorted.Count);

            Assert.AreEqual(1, sorted[0].Priority);
            Assert.AreEqual(2, sorted[1].Priority);
            Assert.AreEqual(3, sorted[2].Priority);

            Assert.AreEqual(1, sorted[0].Tasks.Count);
            Assert.AreEqual("t1", sorted[0].Tasks[0].Name);

            Assert.AreEqual(1, sorted[1].Tasks.Count);
            Assert.AreEqual("t2", sorted[1].Tasks[0].Name);

            Assert.AreEqual(1, sorted[2].Tasks.Count);
            Assert.AreEqual("t3", sorted[2].Tasks[0].Name);
        }

        [TestMethod]
        public async Task SortTaskSet_GivenInvertedTasks_ReturnsOk()
        {
            var lines = Generator("t2->t3", "t1->t2");
            var taskSet = await TaskSet.Build(lines);
            var sorted = taskSet.Sort();

            Assert.AreEqual(3, sorted.Count);

            Assert.AreEqual("t1", sorted[0].Tasks[0].Name);
            Assert.AreEqual("t2", sorted[1].Tasks[0].Name);
            Assert.AreEqual("t3", sorted[2].Tasks[0].Name);
        }

        [TestMethod]
        public async Task SortTaskSet_GivenSampleData_ReturnsOk()
        {
            var lines = Generator(
                "Design floor plans->Review and edit plans",
                "Review and edit plans->Prepare the ground",
                "Frame the structure->Siding",
                "Frame the structure->Landscaping",
                "Prepare the ground->Pour the foundation",
                "Pour the foundation->Frame the structure",
                "Frame the structure->Plumbing",
                "Electrical->Drywall",
                "Frame the structure->Roofing",
                "HVAC->Drywall",
                "Drywall->Paint",
                "Frame the structure->Electrical",
                "Frame the structure->HVAC",
                "Plumbing->Drywall",
                "Paint->Carpet",
                "Frame the structure->Windows",
                "Roofing->Shingles",
                "Frame the structure->Driveway"
            );

            var taskSet = await TaskSet.Build(lines);
            var sorted = taskSet.Sort();

            Assert.AreEqual(9, sorted.Count);

            Assert.AreEqual("Design floor plans", sorted[0].Tasks[0].Name);
            Assert.AreEqual("Review and edit plans", sorted[1].Tasks[0].Name);

            Assert.AreEqual(8, sorted[5].Tasks.Count);
        }

        // Helper method to generate an IAsyncEnumerable
        private async IAsyncEnumerable<TaskNames> Generator(params string[] lines)
        {
            await Task.Yield();
            
            foreach (var line in lines)
                yield return new TaskNames(line.Split("->")[0], line.Split("->")[1]);
        }
    }
}
