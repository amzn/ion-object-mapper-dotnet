/*
 * Copyright (c) Amazon.com, Inc. or its affiliates. All Rights Reserved.
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
    using System;
    using System.Collections.Generic;

    public class PerformanceSuite
    {
        private readonly long baselineKey;
        private readonly int errorMargin;
        private readonly IDictionary<long, Metric> metrics;

        public PerformanceSuite(long baselineKey, int errorMargin)
        {
            this.baselineKey = baselineKey;
            this.errorMargin = errorMargin;
            this.metrics = new Dictionary<long, Metric>();
        }

        public void AddMetric(long key, long runtime, long memoryUsed)
        {
            this.metrics.Add(key, new Metric { RuntimeTicks = runtime, MemoryUsedBytes = memoryUsed });
        }

        public bool IsLinearRuntime()
        {
            long baseRuntime = this.metrics[this.baselineKey].RuntimeTicks / this.baselineKey;
            Console.WriteLine($"Runtime baseline : {baseRuntime} ticks per entry");

            long threshold = Convert.ToInt64(baseRuntime * (1 + (this.errorMargin / 100m)));
            foreach (var kvp in this.metrics)
            {
                long entryCount = kvp.Key;
                if (kvp.Value.RuntimeTicks / entryCount > threshold)
                {
                    Console.WriteLine($"Acceptable runtime threshold of {threshold} ticks exceeded for {entryCount} entries : {kvp.Value.RuntimeTicks / entryCount} ticks used per entry");
                    return false;
                }
            }

            return true;
        }

        public bool IsLinearMemoryUsed()
        {
            long baseMemoryUsed = this.metrics[this.baselineKey].MemoryUsedBytes / this.baselineKey;
            Console.WriteLine($"Memory baseline : {baseMemoryUsed} bytes per entry");

            long threshold = Convert.ToInt64(baseMemoryUsed * (1 + (this.errorMargin / 100m)));
            foreach (var kvp in this.metrics)
            {
                long entryCount = kvp.Key;
                if (kvp.Value.MemoryUsedBytes / entryCount > threshold)
                {
                    Console.WriteLine($"Acceptable memory threshold of {threshold} bytes exceeded for {entryCount} entries : {kvp.Value.MemoryUsedBytes / entryCount} bytes used per entry");
                    return false;
                }
            }

            return true;
        }
    }

    public struct Metric
    {
        public long RuntimeTicks { get; init; }
        public long MemoryUsedBytes { get; init; }
    }

    public class PerformanceTestObject
    {
        public string Name { get; set; }
        public long Value { get; set; }
    }
}
