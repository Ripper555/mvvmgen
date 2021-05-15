﻿// ***********************************************************************
// ⚡ MvvmGen => https://github.com/thomasclaudiushuber/mvvmgen
// Copyright © by Thomas Claudius Huber
// Licensed under the MIT license => See LICENSE file in repository root
// ***********************************************************************

using Xunit;

namespace MvvmGen.SourceGenerators
{
    public class PropertyInvalidateAttributeTests : ViewModelGeneratorTestsBase
    {
        [InlineData(true, false, "[PropertyInvalidate(nameof(FirstName))]\n[PropertyInvalidate(nameof(FirstName))]")]
        [InlineData(true, true, "[PropertyInvalidate(nameof(FirstName), nameof(LastName))]")]
        [InlineData(true, true, "[PropertyInvalidate(\"FirstName\", \"LastName\")]")]
        [InlineData(true, true, "[PropertyInvalidate(nameof(FirstName))]\n[PropertyInvalidate(nameof(LastName))]")]
        [InlineData(true, true, "[PropertyInvalidate(\"FirstName\")]\n[PropertyInvalidate(\"LastName\")]")]
        [InlineData(true, false, "[PropertyInvalidate(\"FirstName\")]")]
        [InlineData(true, false, "[PropertyInvalidate(nameof(FirstName))]")]
        [InlineData(false, false, "")]
        [Theory]
        public void CallOnPropertyChangedInSettersOfOtherProperty(bool expectedInFirstName, bool expectedInLastName, string propertyInvalidateAttributes)
        {
            var onPropertyChangedCall = @"                    OnPropertyChanged(""FullName"");
";
            // Note: Do line-breaks in the string above with @"". 
            // If you use \r\n, the test will work on Windows, but fail on GitHub.
            // If you use \n, the test will fail on Windows, but work on GitHub. :-)

            ShouldGenerateExpectedCode(
      $@"using MvvmGen;

namespace MyCode
{{
  [ViewModel]
  public partial class EmployeeViewModel
  {{
    [Property] string _firstName;
    [Property] string _lastName;
    
    {propertyInvalidateAttributes}
    public string FullName => $""{{FirstName}} {{LastName}}"";
  }}
}}",
      $@"{AutoGeneratedComment}
{AutoGeneratedUsings}

namespace MyCode
{{
    partial class EmployeeViewModel : ViewModelBase
    {{
        public EmployeeViewModel()
        {{
            this.OnInitialize();
        }}

        partial void OnInitialize();

        public string FirstName
        {{
            get => _firstName;
            set
            {{
                if (_firstName != value)
                {{
                    _firstName = value;
                    OnPropertyChanged(""FirstName"");
{(expectedInFirstName ? onPropertyChangedCall : "")}                }}
            }}
        }}

        public string LastName
        {{
            get => _lastName;
            set
            {{
                if (_lastName != value)
                {{
                    _lastName = value;
                    OnPropertyChanged(""LastName"");
{(expectedInLastName ? onPropertyChangedCall : "")}                }}
            }}
        }}
    }}
}}
");
        }
    }
}
