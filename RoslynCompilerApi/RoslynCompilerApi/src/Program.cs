

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoslynCompilerApi;

class Program
{
    static void Main(string[] args)
    {
        DllAnalysis.Run();
        SolutionAnalysis.Run();
    }
    
    
}