/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */

namespace Amazon.IonObjectMapper.PerformanceTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    [TestClass]
    public class PerformanceTest
    {
        /// <summary>
        /// Size of dictionary to use as the baseline for determining how much runtime and
        /// memory is required for one entry.
        ///
        /// Setting this to be too low will cause overhead runtime and memory to dominate
        /// the per-entry baseline.
        /// </summary>
        private const long BaseCount = 100000;

        /// <summary>
        /// The orders of magnitude to test over the BaseCount amount.
        ///
        /// For example, a BaseCount of 100,000 with a Magnitude of 2 will test
        /// 100,000, 1,000,000 and 10,000,000 entries.
        /// </summary>
        private const int Magnitude = 2;

        /// <summary>
        /// Percentage margin of error to allow over the per-entry baseline.
        /// </summary>
        private const int ErrorMargin = 15;

        private static readonly PerformanceSuite suite = new PerformanceSuite(BaseCount, ErrorMargin);

        [TestMethod]
        public void AssertLinearRuntime()
        {
            Assert.IsTrue(suite.IsLinearRuntime());
        }

        [TestMethod]
        public void AssertLinearMemoryUsed()
        {
            Assert.IsTrue(suite.IsLinearMemoryUsed());
        }

        [ClassInitialize]
        public static void RunPerformanceTest(TestContext context)
        {
            IList<IDictionary<string, PerformanceTestObject>> dictionaries = new List<IDictionary<string, PerformanceTestObject>>();
            for (int i = 0; i <= Magnitude; i++)
            {
                long dictionarySize = Convert.ToInt64(BaseCount * Math.Pow(10, i));
                IDictionary<string, PerformanceTestObject> dictionary = new Dictionary<string, PerformanceTestObject>();

                for (long j = 0; j < dictionarySize; j++)
                {
                    dictionary.Add(j.ToString(), new PerformanceTestObject { Name = j.ToString(), Value = j });
                }

                dictionaries.Add(dictionary);
            }

            foreach (var dictionary in dictionaries)
            {
                RecordMetrics(dictionary);
            }
        }

        private static void RecordMetrics(IDictionary<string, PerformanceTestObject> dictionary)
        {
            IonSerializer serializer = new IonSerializer();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            long baseMemoryUsed = GC.GetTotalMemory(true);

            Stopwatch timer = new Stopwatch();

            timer.Start();
            Stream serialized = serializer.Serialize(dictionary);
            dictionary = serializer.Deserialize<Dictionary<string, PerformanceTestObject>>(serialized);
            timer.Stop();

            long memoryUsed = GC.GetTotalMemory(true) - baseMemoryUsed;

            suite.AddMetric(dictionary.Count, timer.ElapsedTicks, memoryUsed);
        }
    }
}
