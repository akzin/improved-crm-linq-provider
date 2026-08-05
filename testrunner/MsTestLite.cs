using System;
using System.Linq;
using System.Reflection;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: mstest-lite <test-assembly>");
            return 2;
        }

        var assembly = Assembly.LoadFrom(args[0]);
        var passed = 0;
        var failed = 0;

        foreach (var type in assembly.GetTypes().Where(HasTestClass))
        {
            foreach (var method in type.GetMethods().Where(HasTestMethod))
            {
                var expectedType = GetExpectedExceptionType(method);
                Exception actual = null;

                try
                {
                    method.Invoke(Activator.CreateInstance(type), null);
                }
                catch (TargetInvocationException exception)
                {
                    actual = exception.InnerException;
                }

                if ((expectedType == null && actual == null) ||
                    (expectedType != null && actual != null && expectedType.IsAssignableFrom(actual.GetType())))
                {
                    passed++;
                }
                else
                {
                    failed++;
                    Console.WriteLine(
                        "FAIL {0}.{1}: expected={2}, actual={3}",
                        type.FullName,
                        method.Name,
                        expectedType == null ? "none" : expectedType.FullName,
                        actual == null ? "none" : actual.ToString());
                }
            }
        }

        Console.WriteLine("Passed: {0}; Failed: {1}", passed, failed);
        return failed == 0 ? 0 : 1;
    }

    private static bool HasTestClass(Type type)
    {
        return HasAttribute(type.GetCustomAttributes(false), "TestClassAttribute");
    }

    private static bool HasTestMethod(MethodInfo method)
    {
        return HasAttribute(method.GetCustomAttributes(false), "TestMethodAttribute");
    }

    private static bool HasAttribute(object[] attributes, string name)
    {
        return attributes.Any(attribute => attribute.GetType().Name == name);
    }

    private static Type GetExpectedExceptionType(MethodInfo method)
    {
        var attribute = method.GetCustomAttributes(false)
            .FirstOrDefault(item => item.GetType().Name == "ExpectedExceptionAttribute");
        if (attribute == null)
            return null;

        return (Type)attribute.GetType().GetProperty("ExceptionType").GetValue(attribute, null);
    }
}
