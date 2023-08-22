using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

namespace RoslynCompilerApi;

public class SolutionAnalysis
{

    public class ProjectInfo
    {
        public Project project;
        public Compilation compilation;
        public List<Document> documentList = null;
    }
    
    private Dictionary<string, ProjectInfo> _projectInfoMap = new Dictionary<string, ProjectInfo>();
    
    public void InitSolution(string slnPath)
    {
        // set up visual studio instance
        VisualStudioInstance instance = MSBuildLocator.QueryVisualStudioInstances().First();
        MSBuildLocator.RegisterInstance(instance);
        
        var workspace = MSBuildWorkspace.Create();
        var solution = workspace.OpenSolutionAsync(slnPath).Result;
        
        foreach (var diagnostic in workspace.Diagnostics)
            Console.WriteLine(diagnostic.ToString());
        
        // generate project infos
        foreach (var project in solution.Projects)
        {
            var compilation = project.GetCompilationAsync().Result;
            if (compilation == null)
                throw new Exception("[Roslyn] Project Assembly-CSharp Compilation Failed");
            
            ProjectInfo info = new ProjectInfo
            {
                project = project,
                compilation = compilation,
                documentList = project.Documents.ToList()
            };

            _projectInfoMap[project.Name] = info;
        }
    }
    
    public List<INamedTypeSymbol> GetDerivedClasses(string baseClassName, string classFromWhichProject)
    {
        if (!_projectInfoMap.TryGetValue(classFromWhichProject, out var proj))
            return new List<INamedTypeSymbol>();
        
        INamedTypeSymbol? baseClassSymbol = null;
        foreach (var document in proj.documentList)
        {
            GetSemanticModelAndSyntaxNode(document, out var semanticModel, out var syntaxRoot);
            foreach (var classDeclaration in GetSyntaxClassDeclaration(syntaxRoot))
            {
                var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
                if (classSymbol.Name == baseClassName)
                {
                    baseClassSymbol = classSymbol as INamedTypeSymbol;
                    break;
                }
            }
        }

        if (baseClassSymbol == null)
            return new List<INamedTypeSymbol>();
        
        var derivedTypes = SymbolFinder.FindDerivedClassesAsync(baseClassSymbol, proj.project.Solution).Result.ToList();
        return derivedTypes;
    }

    public bool GetSemanticModelAndSyntaxNode(Document doc, out SemanticModel? semanticModel, out SyntaxNode? syntaxRoot)
    {
        semanticModel = doc.GetSemanticModelAsync().Result;
        syntaxRoot = semanticModel?.SyntaxTree.GetRoot();

        return semanticModel != null && syntaxRoot != null;
    }
    
    public IEnumerable<ClassDeclarationSyntax> GetSyntaxClassDeclaration(SyntaxNode syntaxRoot)
    {
        return GetSyntaxDeclaration<ClassDeclarationSyntax>(syntaxRoot);
    }
    
    public IEnumerable<NamespaceDeclarationSyntax> GetSyntaxNamespaceDeclaration(SyntaxNode syntaxRoot)
    {
        return GetSyntaxDeclaration<NamespaceDeclarationSyntax>(syntaxRoot);
    }
    
    public IEnumerable<InterfaceDeclarationSyntax> GetSyntaxInterfaceDeclaration(SyntaxNode syntaxRoot)
    {
        return GetSyntaxDeclaration<InterfaceDeclarationSyntax>(syntaxRoot);
    }

    public IEnumerable<T> GetSyntaxDeclaration<T>(SyntaxNode syntaxRoot) where T : SyntaxNode
    {
        return syntaxRoot.DescendantNodes().OfType<T>();
    }

    public T? GetSymbolFromSyntax<T>(SemanticModel semanticModel, SyntaxNode declaration) where T : class, ISymbol
    {
        ISymbol? symbol = semanticModel.GetDeclaredSymbol(declaration);
        if (symbol == null)
            return null;
        
        return symbol as T;
    }
    
    public IEnumerable<IFieldSymbol> GetAllFieldMember(INamedTypeSymbol symbol)
    {
        return symbol.GetMembers()
            .Where(m => m.Kind == SymbolKind.Field)
            .Cast<IFieldSymbol>();
    }

    public IEnumerable<IMethodSymbol> GetAllMethodMember(INamedTypeSymbol symbol)
    {
        return symbol.GetMembers()
            .Where(m => m.Kind == SymbolKind.Method)
            .Cast<IMethodSymbol>();
    }
    
    public IEnumerable<IPropertySymbol> GetAllPropertyMember(INamedTypeSymbol symbol)
    {
        return symbol.GetMembers()
            .Where(m => m.Kind == SymbolKind.Property)
            .Cast<IPropertySymbol>();
    }

    public BlockSyntax? GetMethodBody(IMethodSymbol methodSymbol)
    {
        foreach (var location in methodSymbol.Locations)
        {
            if (location.SourceTree == null)
                continue;
            
            var syntaxRoot = location.SourceTree.GetRoot();
            var position = location.SourceSpan.Start;
            var methodToken = syntaxRoot.FindToken(position);
            if (methodToken.Parent == null)
                continue;
            
            var methodDeclaration = methodToken.Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (methodDeclaration == null)
                continue;
            
            return methodDeclaration.Body;
        }

        return null;
    }
    
    public static void Run()
    {
        VisualStudioInstance instance = MSBuildLocator.QueryVisualStudioInstances().First();
        MSBuildLocator.RegisterInstance(instance);
        
        string slnPath = "../../../../../ToBeAnalyzedSolution/ToBeAnalyzedSolution.sln";
        string fullPath = Path.GetFullPath(slnPath);
        
        using var workspace = MSBuildWorkspace.Create();
        var solution = workspace.OpenSolutionAsync(fullPath).Result;
        
        foreach (var project in solution.Projects)
        {
            Console.WriteLine($"Project Name: {project.Name}, Doc Nums: {project.Documents.Count()}");
            
            foreach (var document in project.Documents)
            {
                Console.WriteLine($"Document Name: {document.Name}");
                
                var model = document.GetSemanticModelAsync().Result;
                var root = model.SyntaxTree.GetRoot();
                
                var methodDeclarations = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

                foreach (var methodDeclaration in methodDeclarations)
                {
                    // ...
                }
            }
        }
    }
}