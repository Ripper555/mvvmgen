﻿// ***********************************************************************
// ⚡ MvvmGen => https://github.com/thomasclaudiushuber/mvvmgen
// Copyright © by Thomas Claudius Huber
// Licensed under the MIT license => See LICENSE file in repository root
// ***********************************************************************

namespace MvvmGen.Model;

internal class InjectionToGenerate
{
    public InjectionToGenerate(string type, string propertyName)
    {
        Type = type;
        PropertyName = propertyName;
    }
    public string Type { get; }

    public string PropertyName { get; internal set; }

    public string PropertyAccessModifier { get; set; } = "protected";

    public string SetterAccessModifier => PropertyAccessModifier == "private" ? "" : "private";
}