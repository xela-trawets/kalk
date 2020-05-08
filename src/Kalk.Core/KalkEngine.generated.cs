//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Date: 07 May 2020
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Kalk.Core
{
    public partial class KalkEngine
    {
        partial void RegisterDocumentation()
        {
            {
                var descriptor = Descriptors["abs"];
                descriptor.Description = @"Returns the absolute value of the specified value.";
                descriptor.IsCommand = false;
                descriptor.Params.Add(new KalkParamDescriptor("x", @"The specified value.")  { IsOptional = false });
                descriptor.Returns = @"The absolute value of the x parameter.";
            }
            {
                var descriptor = Descriptors["cls"];
                descriptor.Description = @"Clears the screen (by default) or the history (e.g clear history).";
                descriptor.IsCommand = true;
                descriptor.Params.Add(new KalkParamDescriptor("what", @"An optional argument specifying what to clear. Can be of the following value:
    * screen: to clear the screen (default if not passed)
    * history: to clear the history")  { IsOptional = true });
            }
            {
                var descriptor = Descriptors["e"];
                descriptor.Description = @"Defines the natural logarithmic base. e = 2.71828182845905";
                descriptor.IsCommand = false;
            }
            {
                var descriptor = Descriptors["inf"];
                descriptor.Description = @"Defines the infinity constant for a double.";
                descriptor.IsCommand = false;
            }
            {
                var descriptor = Descriptors["nan"];
                descriptor.Description = @"Defines the ""Not a Number"" constant for a double.";
                descriptor.IsCommand = false;
            }
            {
                var descriptor = Descriptors["pi"];
                descriptor.Description = @"Defines the PI constant. pi = 3.14159265358979";
                descriptor.IsCommand = false;
            }
            {
                var descriptor = Descriptors["sign"];
                descriptor.Description = @"Returns an integer that indicates the sign of a number.";
                descriptor.IsCommand = false;
                descriptor.Params.Add(new KalkParamDescriptor("x", @"A signed number.")  { IsOptional = false });
                descriptor.Returns = @"A number that indicates the sign of value:
     - -1 if x is less than zero
     - 0 if x is equal to zero
     - 1 if x is greater than zero.";
            }
        }        
    }
}
