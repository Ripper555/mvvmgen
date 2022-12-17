// ***********************************************************************
// ⚡ MvvmGen => https://github.com/thomasclaudiushuber/mvvmgen
// Copyright © by Thomas Claudius Huber
// Licensed under the MIT license => See LICENSE file in repository root
// ***********************************************************************

using System;

namespace MvvmGen;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class SafeCommandAttribute : CommandAttribute
{
}
