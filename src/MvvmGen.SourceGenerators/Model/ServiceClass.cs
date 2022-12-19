using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace MvvmGen.SourceGenerators.Model;

public class ServiceClass
{
    public ServiceClass(INamedTypeSymbol service, ImmutableArray<INamedTypeSymbol> interfaces)
    {
        Interfaces = interfaces;
        Service = service;
    }

    public INamedTypeSymbol Service { get; }
    public ImmutableArray<INamedTypeSymbol> Interfaces { get; }
}
