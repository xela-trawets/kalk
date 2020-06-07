//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Date: 07 Jun 2020
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using NUnit.Framework;

namespace Kalk.Tests
{
    using Kalk.Core.Modules;
    using Kalk.Core;

    public partial class CsvModuleTests : KalkTestBase
    {
    }

    public partial class CurrencyModuleTests : KalkTestBase
    {
    }

    public partial class FileModuleTests : KalkTestBase
    {
    }

    public partial class KalkEngineTests : KalkTestBase
    {
    }

    public partial class MathModuleTests : KalkTestBase
    {
        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Fib(Kalk.Core.KalkIntValue)"/> or `fib`.
        /// </summary>
        [TestCase(@"fib 50", @"# fib(50)
out = 12586269025", Category = "Math Functions")]
        public static void Test_fib(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.ComplexNumber"/> or `i`.
        /// </summary>
        [TestCase(@"1 + 2i", @"# 1 + 2 * i
out = 1 + 2i", Category = "Math Functions")]
        public static void Test_i(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.All(System.Object)"/> or `all`.
        /// </summary>
        [TestCase(@"all(bool4(true, false, true, false))
all(bool4(true, true, true, true))
all([0,1,0,2])
all([1,1,1,1])", @"# all(bool4(true, false, true, false))
out = false
# all(bool4(true, true, true, true))
out = true
# all([0,1,0,2])
out = false
# all([1,1,1,1])
out = true", Category = "Math Functions")]
        public static void Test_all(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Any(System.Object)"/> or `any`.
        /// </summary>
        [TestCase(@"any(bool4(true, false, true, false))
any(bool4(false, false, false, false))
any([0,1,0,2])
any([0,0,0,0])", @"# any(bool4(true, false, true, false))
out = true
# any(bool4(false, false, false, false))
out = false
# any([0,1,0,2])
out = true
# any([0,0,0,0])
out = false", Category = "Math Functions")]
        public static void Test_any(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Abs(Kalk.Core.KalkCompositeValue)"/> or `abs`.
        /// </summary>
        [TestCase(@"abs(-1)
abs(float4(-1, 1, -2, -3))", @"# abs(-1)
out = 1
# abs(float4(-1, 1, -2, -3))
out = float4(1, 1, 2, 3)", Category = "Math Functions")]
        public static void Test_abs(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Rnd(Kalk.Core.KalkCompositeValue)"/> or `rnd`.
        /// </summary>
        [TestCase(@"seed(0); rnd
rnd
rnd(float4)", @"# seed(0); rnd
out = 0.7262432699679598
# rnd
out = 0.8173253595909687
# rnd(float4)
out = float4(0.7680227, 0.5581612, 0.20603316, 0.5588848)", Category = "Math Functions")]
        public static void Test_rnd(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Seed(System.Nullable{System.Int32})"/> or `seed`.
        /// </summary>
        [TestCase(@"seed(0); rnd
seed(1); rnd", @"# seed(0); rnd
out = 0.7262432699679598
# seed(1); rnd
out = 0.24866858415709278", Category = "Math Functions")]
        public static void Test_seed(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Modf(Kalk.Core.KalkCompositeValue)"/> or `modf`.
        /// </summary>
        [TestCase(@"modf(1.5)
modf(float2(-1.2, 3.4))", @"# modf(1.5)
out = [1, 0.5]
# modf(float2(-1.2, 3.4))
out = [float2(-1, 3), float2(-0.20000005, 0.4000001)]", Category = "Math Functions")]
        public static void Test_modf(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Radians(Kalk.Core.KalkCompositeValue)"/> or `radians`.
        /// </summary>
        [TestCase(@"radians(90)
radians(180)", @"# radians(90)
out = 1.5707963267948966
# radians(180)
out = 3.141592653589793", Category = "Math Functions")]
        public static void Test_radians(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Degrees(Kalk.Core.KalkCompositeValue)"/> or `degrees`.
        /// </summary>
        [TestCase(@"degrees(pi/2)
degrees(pi)", @"# degrees(pi / 2)
out = 90
# degrees(pi)
out = 180", Category = "Math Functions")]
        public static void Test_degrees(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Sign(Kalk.Core.KalkCompositeValue)"/> or `sign`.
        /// </summary>
        [TestCase(@"sign(-5); sign(0); sign(2.3)
sign float4(-1, 2, 0, 1.5)", @"# sign(-5); sign(0); sign(2.3)
out = -1
out = 0
out = 1
# sign(float4(-1, 2, 0, 1.5))
out = float4(-1, 1, 0, 1)", Category = "Math Functions")]
        public static void Test_sign(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Cos(Kalk.Core.KalkDoubleValue)"/> or `cos`.
        /// </summary>
        [TestCase(@"cos 0.5
cos float4(pi, pi/2, 0, 0.5)", @"# cos(0.5)
out = 0.8775825618903728
# cos(float4(pi, pi / 2, 0, 0.5))
out = float4(-1, -4.371139E-08, 1, 0.87758255)", Category = "Math Functions")]
        public static void Test_cos(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Acos(Kalk.Core.KalkDoubleValue)"/> or `acos`.
        /// </summary>
        [TestCase(@"acos(-1)
acos(0)
acos(1)
acos(float4(-1,0,1,0.5))", @"# acos(-1)
out = 3.141592653589793
# acos(0)
out = 1.5707963267948966
# acos(1)
out = 0
# acos(float4(-1, 0, 1, 0.5))
out = float4(3.1415927, 1.5707964, 0, 1.0471976)", Category = "Math Functions")]
        public static void Test_acos(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Cosh(Kalk.Core.KalkDoubleValue)"/> or `cosh`.
        /// </summary>
        [TestCase(@"cosh(-1)
cosh(1)
cosh(0)
cosh(float4(-1, 1, 0, 2))", @"# cosh(-1)
out = 1.5430806348152437
# cosh(1)
out = 1.5430806348152437
# cosh(0)
out = 1
# cosh(float4(-1, 1, 0, 2))
out = float4(1.5430807, 1.5430807, 1, 3.7621956)", Category = "Math Functions")]
        public static void Test_cosh(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Acosh(Kalk.Core.KalkDoubleValue)"/> or `acosh`.
        /// </summary>
        [TestCase(@"acosh(1)
acosh(10)
acosh(float4(1,2,4,10))", @"# acosh(1)
out = 0
# acosh(10)
out = 2.993222846126381
# acosh(float4(1, 2, 4, 10))
out = float4(0, 1.316958, 2.063437, 2.993223)", Category = "Math Functions")]
        public static void Test_acosh(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Sin(Kalk.Core.KalkDoubleValue)"/> or `sin`.
        /// </summary>
        [TestCase(@"sin 0.5
sin float4(pi, pi/2, 0, 0.5)", @"# sin(0.5)
out = 0.479425538604203
# sin(float4(pi, pi / 2, 0, 0.5))
out = float4(-8.742278E-08, 1, 0, 0.47942555)", Category = "Math Functions")]
        public static void Test_sin(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Asin(Kalk.Core.KalkDoubleValue)"/> or `asin`.
        /// </summary>
        [TestCase(@"asin 0.5
asin float4(-1, 0, 1, 0.5)", @"# asin(0.5)
out = 0.5235987755982989
# asin(float4(-1, 0, 1, 0.5))
out = float4(-1.5707964, 0, 1.5707964, 0.5235988)", Category = "Math Functions")]
        public static void Test_asin(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Sinh(Kalk.Core.KalkDoubleValue)"/> or `sinh`.
        /// </summary>
        [TestCase(@"sinh(-1)
sinh(0)
sinh(1)
sinh(float4(-1, 1, 0, 2))", @"# sinh(-1)
out = -1.1752011936438014
# sinh(0)
out = 0
# sinh(1)
out = 1.1752011936438014
# sinh(float4(-1, 1, 0, 2))
out = float4(-1.1752012, 1.1752012, 0, 3.6268604)", Category = "Math Functions")]
        public static void Test_sinh(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Asinh(Kalk.Core.KalkDoubleValue)"/> or `asinh`.
        /// </summary>
        [TestCase(@"asinh(-1.1752011936438014)
asinh(0)
asinh(1.1752011936438014)
asinh(float4(-1.1752011936438014, 0, 1.1752011936438014, 2))", @"# asinh(-1.1752011936438014)
out = -1
# asinh(0)
out = 0
# asinh(1.1752011936438014)
out = 1
# asinh(float4(-1.1752011936438014, 0, 1.1752011936438014, 2))
out = float4(-1, 0, 1, 1.4436355)", Category = "Math Functions")]
        public static void Test_asinh(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Tan(Kalk.Core.KalkDoubleValue)"/> or `tan`.
        /// </summary>
        [TestCase(@"tan(0.5)
tan(1)
tan float4(1, 2, 3, 4)", @"# tan(0.5)
out = 0.5463024898437905
# tan(1)
out = 1.5574077246549023
# tan(float4(1, 2, 3, 4))
out = float4(1.5574077, -2.1850398, -0.14254655, 1.1578213)", Category = "Math Functions")]
        public static void Test_tan(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Atan(Kalk.Core.KalkDoubleValue)"/> or `atan`.
        /// </summary>
        [TestCase(@"atan(0.5)
atan(1)
atan(0)
atan(float4(0,1,2,3))", @"# atan(0.5)
out = 0.4636476090008061
# atan(1)
out = 0.7853981633974483
# atan(0)
out = 0
# atan(float4(0, 1, 2, 3))
out = float4(0, 0.7853982, 1.1071488, 1.2490457)", Category = "Math Functions")]
        public static void Test_atan(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Tanh(Kalk.Core.KalkDoubleValue)"/> or `tanh`.
        /// </summary>
        [TestCase(@"tanh(0)
tanh(1)
tanh(2)
tanh(float4(0, 1, 2, 3))", @"# tanh(0)
out = 0
# tanh(1)
out = 0.7615941559557649
# tanh(2)
out = 0.9640275800758169
# tanh(float4(0, 1, 2, 3))
out = float4(0, 0.7615942, 0.9640276, 0.9950548)", Category = "Math Functions")]
        public static void Test_tanh(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Atanh(Kalk.Core.KalkDoubleValue)"/> or `atanh`.
        /// </summary>
        [TestCase(@"atanh(0)
atanh(0.5)
atanh(float4(-0.5, 0, 0.5, 0.8))", @"# atanh(0)
out = 0
# atanh(0.5)
out = 0.5493061443340549
# atanh(float4(-0.5, 0, 0.5, 0.8))
out = float4(-0.54930615, 0, 0.54930615, 1.0986123)", Category = "Math Functions")]
        public static void Test_atanh(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Atan2(Kalk.Core.KalkDoubleValue,Kalk.Core.KalkDoubleValue)"/> or `atan2`.
        /// </summary>
        [TestCase(@"atan2(1,1)
atan2(1,0)
atan2(0,1)
atan2(float4(1), float4(0,1,-1,2))", @"# atan2(1, 1)
out = 0.7853981633974483
# atan2(1, 0)
out = 1.5707963267948966
# atan2(0, 1)
out = 0
# atan2(float4(1), float4(0, 1, -1, 2))
out = float4(1.5707964, 0.7853982, 2.3561945, 0.4636476)", Category = "Math Functions")]
        public static void Test_atan2(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Fmod(Kalk.Core.KalkDoubleValue,Kalk.Core.KalkDoubleValue)"/> or `fmod`.
        /// </summary>
        [TestCase(@"fmod(2.5, 2)
fmod(2.5, 3)
fmod(-1.5, 1)
fmod(float4(1.5, 1.2, -2.3, -4.6), 0.2)", @"# fmod(2.5, 2)
out = 0.5
# fmod(2.5, 3)
out = 2.5
# fmod(-1.5, 1)
out = -0.5
# fmod(float4(1.5, 1.2, -2.3, -4.6), 0.2)
out = float4(0.09999998, 2.9802322E-08, -0.09999992, -0.19999984)", Category = "Math Functions")]
        public static void Test_fmod(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Frac(Kalk.Core.KalkDoubleValue)"/> or `frac`.
        /// </summary>
        [TestCase(@"frac(1.25)
frac(10.5)
frac(-1.75)
frac(-10.25)
frac(float4(1.25, 10.5, -1.75, -10.25))", @"# frac(1.25)
out = 0.25
# frac(10.5)
out = 0.5
# frac(-1.75)
out = 0.25
# frac(-10.25)
out = 0.75
# frac(float4(1.25, 10.5, -1.75, -10.25))
out = float4(0.25, 0.5, 0.25, 0.75)", Category = "Math Functions")]
        public static void Test_frac(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Rsqrt(Kalk.Core.KalkDoubleValue)"/> or `rsqrt`.
        /// </summary>
        [TestCase(@"rsqrt(1)
rsqrt(2)
rsqrt(float4(1,2,3,4))", @"# rsqrt(1)
out = 1
# rsqrt(2)
out = 0.7071067811865475
# rsqrt(float4(1, 2, 3, 4))
out = float4(1, 0.70710677, 0.57735026, 0.5)", Category = "Math Functions")]
        public static void Test_rsqrt(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Sqrt(Kalk.Core.KalkDoubleValue)"/> or `sqrt`.
        /// </summary>
        [TestCase(@"sqrt(1)
sqrt(2)
sqrt(float4(1,2,3,4))", @"# sqrt(1)
out = 1
# sqrt(2)
out = 1.4142135623730951
# sqrt(float4(1, 2, 3, 4))
out = float4(1, 1.4142135, 1.7320508, 2)", Category = "Math Functions")]
        public static void Test_sqrt(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Log(Kalk.Core.KalkDoubleValue)"/> or `log`.
        /// </summary>
        [TestCase(@"log 1
log 2
log 0
log float4(0,1,2,3)", @"# log(1)
out = 0
# log(2)
out = 0.6931471805599453
# log(0)
out = -inf
# log(float4(0, 1, 2, 3))
out = float4(-inf, 0, 0.6931472, 1.0986123)", Category = "Math Functions")]
        public static void Test_log(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Log2(Kalk.Core.KalkDoubleValue)"/> or `log2`.
        /// </summary>
        [TestCase(@"log2 0
log2 8
log2 129
log2 float4(0, 2, 16, 257)", @"# log2(0)
out = -inf
# log2(8)
out = 3
# log2(129)
out = 7.011227255423254
# log2(float4(0, 2, 16, 257))
out = float4(-inf, 1, 4, 8.005625)", Category = "Math Functions")]
        public static void Test_log2(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Log10(Kalk.Core.KalkDoubleValue)"/> or `log10`.
        /// </summary>
        [TestCase(@"log10 0
log10 10
log10 100
log10 1001
log10(float4(0,10,100,1001))", @"# log10(0)
out = -inf
# log10(10)
out = 1
# log10(100)
out = 2
# log10(1001)
out = 3.000434077479319
# log10(float4(0, 10, 100, 1001))
out = float4(-inf, 1, 2, 3.0004342)", Category = "Math Functions")]
        public static void Test_log10(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Exp(Kalk.Core.KalkDoubleValue)"/> or `exp`.
        /// </summary>
        [TestCase(@"exp(0)
exp(1)
exp(float4(0,1,2,3))", @"# exp(0)
out = 1
# exp(1)
out = 2.718281828459045
# exp(float4(0, 1, 2, 3))
out = float4(1, 2.7182817, 7.389056, 20.085537)", Category = "Math Functions")]
        public static void Test_exp(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Exp2(Kalk.Core.KalkDoubleValue)"/> or `exp2`.
        /// </summary>
        [TestCase(@"exp2(0)
exp2(1)
exp2(4)
exp2(float4(0,1,2,3))", @"# exp2(0)
out = 1
# exp2(1)
out = 2
# exp2(4)
out = 16
# exp2(float4(0, 1, 2, 3))
out = float4(1, 2, 4, 8)", Category = "Math Functions")]
        public static void Test_exp2(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Pow(Kalk.Core.KalkDoubleValue,Kalk.Core.KalkDoubleValue)"/> or `pow`.
        /// </summary>
        [TestCase(@"pow(1.5, 3.5)
pow(2, 4)
pow(float4(1,2,3,4), 4)
pow(float4(1..4), float4(5..8))", @"# pow(1.5, 3.5)
out = 4.133513940946613
# pow(2, 4)
out = 16
# pow(float4(1, 2, 3, 4), 4)
out = float4(1, 16, 81, 256)
# pow(float4(1..4), float4(5..8))
out = float4(1, 64, 2187, 65536)", Category = "Math Functions")]
        public static void Test_pow(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Round(Kalk.Core.KalkDoubleValue)"/> or `round`.
        /// </summary>
        [TestCase(@"round(0.2); round(1.5); round(10.7)
round(-0.2); round(-1.5); round(-10.7)", @"# round(0.2); round(1.5); round(10.7)
out = 0
out = 2
out = 11
# round(-0.2); round(-1.5); round(-10.7)
out = -0
out = -2
out = -11", Category = "Math Functions")]
        public static void Test_round(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Floor(Kalk.Core.KalkDoubleValue)"/> or `floor`.
        /// </summary>
        [TestCase(@"floor(0.2); floor(1.5); floor(10.7)
floor(-0.2); floor(-1.5); floor(-10.7)", @"# floor(0.2); floor(1.5); floor(10.7)
out = 0
out = 1
out = 10
# floor(-0.2); floor(-1.5); floor(-10.7)
out = -1
out = -2
out = -11", Category = "Math Functions")]
        public static void Test_floor(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Ceiling(Kalk.Core.KalkDoubleValue)"/> or `ceil`.
        /// </summary>
        [TestCase(@"ceil(0.2); ceil(1.5); ceil(10.7)
ceil(-0.2); ceil(-1.5); ceil(-10.7)", @"# ceil(0.2); ceil(1.5); ceil(10.7)
out = 1
out = 2
out = 11
# ceil(-0.2); ceil(-1.5); ceil(-10.7)
out = -0
out = -1
out = -10", Category = "Math Functions")]
        public static void Test_ceil(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Trunc(Kalk.Core.KalkDoubleValue)"/> or `trunc`.
        /// </summary>
        [TestCase(@"trunc(0.2); trunc(1.5); trunc(10.7)
trunc(-0.2); trunc(-1.5); trunc(-10.7)", @"# trunc(0.2); trunc(1.5); trunc(10.7)
out = 0
out = 1
out = 10
# trunc(-0.2); trunc(-1.5); trunc(-10.7)
out = -0
out = -1
out = -10", Category = "Math Functions")]
        public static void Test_trunc(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Saturate(Kalk.Core.KalkDoubleValue)"/> or `saturate`.
        /// </summary>
        [TestCase(@"saturate(10)
saturate(-10)
saturate(float4(-1, 0.5, 1, 2))", @"# saturate(10)
out = 1
# saturate(-10)
out = 0
# saturate(float4(-1, 0.5, 1, 2))
out = float4(0, 0.5, 1, 1)", Category = "Math Functions")]
        public static void Test_saturate(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Min(Kalk.Core.KalkDoubleValue,Kalk.Core.KalkDoubleValue)"/> or `min`.
        /// </summary>
        [TestCase(@"min(-5, 6)
min(1, 0)
min(float4(0, 1, 2, 3), float4(1, 0, 3, 2))", @"# min(-5, 6)
out = -5
# min(1, 0)
out = 0
# min(float4(0, 1, 2, 3), float4(1, 0, 3, 2))
out = float4(0, 0, 2, 2)", Category = "Math Functions")]
        public static void Test_min(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Max(Kalk.Core.KalkDoubleValue,Kalk.Core.KalkDoubleValue)"/> or `max`.
        /// </summary>
        [TestCase(@"max(-5, 6)
max(1, 0)
max(float4(0, 1, 2, 3), float4(1, 0, 3, 2))", @"# max(-5, 6)
out = 6
# max(1, 0)
out = 1
# max(float4(0, 1, 2, 3), float4(1, 0, 3, 2))
out = float4(1, 1, 3, 3)", Category = "Math Functions")]
        public static void Test_max(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Step(Kalk.Core.KalkDoubleValue,Kalk.Core.KalkDoubleValue)"/> or `step`.
        /// </summary>
        [TestCase(@"step(1, 5)
step(5, 1)
step(float4(0, 1, 2, 3), float4(1, 0, 3, 2))
step(-10, 5)
step(5.5, -10.5)", @"# step(1, 5)
out = 1
# step(5, 1)
out = 0
# step(float4(0, 1, 2, 3), float4(1, 0, 3, 2))
out = float4(1, 0, 1, 0)
# step(-10, 5)
out = 1
# step(5.5, -10.5)
out = 0", Category = "Math Functions")]
        public static void Test_step(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Smoothstep(Kalk.Core.KalkDoubleValue,Kalk.Core.KalkDoubleValue,Kalk.Core.KalkDoubleValue)"/> or `smoothstep`.
        /// </summary>
        [TestCase(@"smoothstep(float4(0), float4(1), float4(-0.5))
smoothstep(float4(0), float4(1), float4(1.5))
smoothstep(float4(0), float4(1), float4(0.5))
smoothstep(float4(0), float4(1), float4(0.9))", @"# smoothstep(float4(0), float4(1), float4(-0.5))
out = float4(0, 0, 0, 0)
# smoothstep(float4(0), float4(1), float4(1.5))
out = float4(1, 1, 1, 1)
# smoothstep(float4(0), float4(1), float4(0.5))
out = float4(0.5, 0.5, 0.5, 0.5)
# smoothstep(float4(0), float4(1), float4(0.9))
out = float4(0.972, 0.972, 0.972, 0.972)", Category = "Math Functions")]
        public static void Test_smoothstep(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Lerp(Kalk.Core.KalkDoubleValue,Kalk.Core.KalkDoubleValue,Kalk.Core.KalkDoubleValue)"/> or `lerp`.
        /// </summary>
        [TestCase(@"lerp(0, 10, 0.5)
lerp(rgb(""AliceBlue"").xyz, rgb(""Green"").xyz, 0.5)", @"# lerp(0, 10, 0.5)
out = 5
# lerp(rgb(""AliceBlue"").xyz, rgb(""Green"").xyz, 0.5)
out = float3(0.47058824, 0.7372549, 0.5)", Category = "Math Functions")]
        public static void Test_lerp(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Clamp(Kalk.Core.KalkDoubleValue,Kalk.Core.KalkDoubleValue,Kalk.Core.KalkDoubleValue)"/> or `clamp`.
        /// </summary>
        [TestCase(@"clamp(-1, 0, 1)
clamp(2, 0, 1)
clamp(0.5, 0, 1)
clamp(float4(0, 1, -2, 3), float4(0, -1, 3, 4), float4(1, 2, 5, 6))", @"# clamp(-1, 0, 1)
out = 0
# clamp(2, 0, 1)
out = 1
# clamp(0.5, 0, 1)
out = 0.5
# clamp(float4(0, 1, -2, 3), float4(0, -1, 3, 4), float4(1, 2, 5, 6))
out = float4(0, 1, 3, 4)", Category = "Math Functions")]
        public static void Test_clamp(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Real(Kalk.Core.KalkComplex)"/> or `real`.
        /// </summary>
        [TestCase(@"real(1.5 + 2.5i)", @"# real(1.5 + 2.5 * i)
out = 1.5", Category = "Math Functions")]
        public static void Test_real(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Imag(Kalk.Core.KalkComplex)"/> or `imag`.
        /// </summary>
        [TestCase(@"imag(1.5 + 2.5i)", @"# imag(1.5 + 2.5 * i)
out = 2.5", Category = "Math Functions")]
        public static void Test_imag(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Phase(Kalk.Core.KalkComplex)"/> or `phase`.
        /// </summary>
        [TestCase(@"phase(1.5 + 2.5i)", @"# phase(1.5 + 2.5 * i)
out = 1.0303768265243125", Category = "Math Functions")]
        public static void Test_phase(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.IsFinite(Kalk.Core.KalkCompositeValue)"/> or `isfinite`.
        /// </summary>
        [TestCase(@"isfinite(1)
isfinite(nan)
isfinite(inf)
isfinite(float4(1, -10.5, inf, nan))", @"# isfinite(1)
out = true
# isfinite(nan)
out = false
# isfinite(inf)
out = false
# isfinite(float4(1, -10.5, inf, nan))
out = bool4(true, true, false, false)", Category = "Math Functions")]
        public static void Test_isfinite(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.IsInf(Kalk.Core.KalkCompositeValue)"/> or `isinf`.
        /// </summary>
        [TestCase(@"isinf(1)
isinf(inf)
isinf(float4(1, -10.5, inf, nan))", @"# isinf(1)
out = false
# isinf(inf)
out = true
# isinf(float4(1, -10.5, inf, nan))
out = bool4(false, false, true, false)", Category = "Math Functions")]
        public static void Test_isinf(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.IsNan(Kalk.Core.KalkCompositeValue)"/> or `isnan`.
        /// </summary>
        [TestCase(@"isnan(1)
isnan(inf)
isnan(nan)
isnan(float4(1, -10.5, inf, nan))", @"# isnan(1)
out = false
# isnan(inf)
out = false
# isnan(nan)
out = true
# isnan(float4(1, -10.5, inf, nan))
out = bool4(false, false, false, true)", Category = "Math Functions")]
        public static void Test_isnan(string input, string output) => AssertScript(input, output);

        /// <summary>
        /// Test for <see cref="M:Kalk.Core.MathModule.Sum(System.Object,System.Object[])"/> or `sum`.
        /// </summary>
        [TestCase(@"sum(1,2,3,4)
sum(float4(1..4))
sum(float4(1..4), float4(5..8))
sum(""a"", ""b"", ""c"")
sum([""a"", ""b"", ""c""])", @"# sum(1, 2, 3, 4)
out = 10
# sum(float4(1..4))
out = 10
# sum(float4(1..4), float4(5..8))
out = float4(15, 16, 17, 18)
# sum(""a"", ""b"", ""c"")
out = ""abc""
# sum([""a"", ""b"", ""c""])
out = ""abc""", Category = "Math Functions")]
        public static void Test_sum(string input, string output) => AssertScript(input, output);

    }

    public partial class MemoryModuleTests : KalkTestBase
    {
    }

    public partial class StringModuleTests : KalkTestBase
    {
    }

    public partial class VectorModuleTests : KalkTestBase
    {
    }

    public partial class WebModuleTests : KalkTestBase
    {
    }
}
