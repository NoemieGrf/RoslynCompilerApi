
namespace Test1
{
    public static class StaticClassTest
    {
        public static int[] testStaticArray = new[] { 1, 2, 3 };

        public static int[] TestStaticMethod()
        {
            return new[] { 4, 5, 6 };
        }
    }

    public class ClassTest
    {
        public int Add(int a, int b)
        {
            return a + b;
        }

        public double Multiply(double a, double b)
        {
            return a * b;
        }
    }
}

namespace Test2
{
    namespace Test2_2
    {
        public class ClassTest
        {
            public void DoLog()
            {
                Console.WriteLine("test test test");
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {

    }
}
