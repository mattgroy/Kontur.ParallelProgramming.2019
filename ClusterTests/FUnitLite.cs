using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using NUnit.Framework;

namespace ClusterTests
{
    public class FUnitLite
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FUnitLite));

        private readonly List<ITestFixture> testFixtures = new List<ITestFixture>();

        public void AddTestFixture<T>(T testFixture)
        {
            this.testFixtures.Add(new TestFixture<T>(testFixture));
        }

        public void RunAndReport()
        {
            var results = new List<(string, bool)>();
            foreach (var testFixture in this.testFixtures)
                results.AddRange(testFixture.Run().Select(p => ($"{testFixture.Name}.{p.Item1}", p.Item2)));

            var oldColor = Console.ForegroundColor;
            foreach (var (name, isSuccess) in results)
            {
                Console.ForegroundColor = isSuccess ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($"[{name}] {(isSuccess ? "Passed" : "Faulted")}");
            }

            Console.ForegroundColor = oldColor;
        }

        private interface ITestFixture
        {
            string Name { get; }
            IEnumerable<(string, bool)> Run();
        }

        private class TestFixture<T> : ITestFixture
        {
            private readonly Method setUp;
            private readonly Method tearDown;
            private readonly List<Method> tests = new List<Method>();

            public TestFixture(T instance)
            {
                this.Name = typeof(T).Name;

                this.setUp = GetMethodsWithAttribute<SetUpAttribute>(instance).SingleOrDefault();
                this.tearDown = GetMethodsWithAttribute<TearDownAttribute>(instance).SingleOrDefault();
                var tests = GetMethodsWithAttribute<TestAttribute>(instance);

                foreach (var method in tests)
                    this.tests.Add(method);
            }

            public IEnumerable<(string, bool)> Run()
            {
                foreach (var test in this.tests)
                {
                    if (this.setUp.Action != null && !this.setUp.Action.Try(out var setUpEx))
                        throw new InvalidOperationException($"[{this.Name}.{test.Name}] SetUp faulted", setUpEx);

                    if (test.Action.Try(out var ex))
                    {
                        yield return (test.Name, true);
                    }
                    else
                    {
                        Log.Error($"[{this.Name}.{test.Name}]", ex);
                        yield return (test.Name, false);
                    }

                    if (this.tearDown.Action != null && !this.tearDown.Action.Try(out var tearDownEx))
                        throw new InvalidOperationException($"[{this.Name}.{test.Name}] TearDown faulted", tearDownEx);
                }
            }

            public string Name { get; }

            private static IEnumerable<Method> GetMethodsWithAttribute<TAttribute>(T testFixture)
                where TAttribute : Attribute
            {
                return typeof(T).GetMethods()
                    .Where(m => m.GetCustomAttributes<TAttribute>().Any())
                    .Where(m => m.GetParameters().Length == 0)
                    .Select(m => new Method(m, testFixture));
            }

            private class Method
            {
                public readonly Action Action;
                public readonly string Name;

                public Method(MethodInfo method, object instance)
                    : this(method.Name, () => method.Invoke(instance, new object[0]))
                {
                }

                public Method(string name, Action action)
                {
                    this.Name = name;
                    this.Action = action;
                }
            }
        }
    }
}