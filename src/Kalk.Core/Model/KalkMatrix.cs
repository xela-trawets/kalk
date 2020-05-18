﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using Scriban;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Kalk.Core
{
    public abstract class KalkMatrix : KalkValue
    {
        protected KalkMatrix(int row, int column)
        {
            if (row <= 0) throw new ArgumentOutOfRangeException(nameof(row));
            if (column <= 0) throw new ArgumentOutOfRangeException(nameof(column));
            RowCount = row;
            ColumnCount = column;
        }

        public int RowCount { get; }

        public int ColumnCount { get; }

        protected abstract KalkMatrix Transpose();

        protected abstract KalkMatrix Identity();

        protected abstract KalkVector Diagonal();

        protected abstract object GenericDeterminant();

        protected abstract KalkMatrix GenericInverse();

        public abstract Array Values { get; }

        public bool IsSquare => RowCount == ColumnCount;

        protected abstract KalkVector GenericMultiplyLeft(KalkVector x);

        protected abstract KalkVector GenericMultiplyRight(KalkVector y);

        protected abstract KalkMatrix GenericMultiply(KalkMatrix y);
        

        protected static void AssertSquareMatrix(KalkMatrix m)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));
            if (!m.IsSquare) throw new ArgumentException($"Matrix must be square nxn instead of {m.TypeName}", nameof(m));
        }

        [KalkDoc("transpose")]
        public static KalkMatrix Transpose(KalkMatrix m)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));
            return m.Transpose();
        }

        [KalkDoc("identity")]
        public static KalkMatrix Identity(KalkMatrix m)
        {
            AssertSquareMatrix(m);
            return m.Identity();
        }

        public static KalkVector Diagonal(KalkMatrix x)
        {
            AssertSquareMatrix(x);
            return x.Diagonal();
        }

        [KalkDoc("determinant")]
        public static object Determinant(KalkMatrix m)
        {
            AssertSquareMatrix(m);
            return m.GenericDeterminant();
        }

        [KalkDoc("inverse")]
        public static KalkMatrix Inverse(KalkMatrix m)
        {
            AssertSquareMatrix(m);
            return m.GenericInverse();
        }

        public static object Multiply(KalkVector x, KalkMatrix y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            CheckElementType(x, y);
            if (x.Length != y.RowCount)
                throw new ArgumentException($"Invalid size between the vector type length {x.Length} and the matrix row count {y.RowCount}. They Must be equal.", nameof(x));
            return y.GenericMultiplyLeft(x);
        }

        public static object Multiply(KalkMatrix x, KalkVector y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            CheckElementType(x, y);
            if (x.ColumnCount != y.Length)
                throw new ArgumentException($"Invalid size between the vector type length {y.Length} and the matrix column count {x.ColumnCount}. They Must be equal.", nameof(x));
            return x.GenericMultiplyRight(y);
        }

        public static object Multiply(KalkMatrix x, KalkMatrix y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            CheckElementType(x, y);
            if (x.ColumnCount != y.RowCount)
                throw new ArgumentException($"Invalid size not between the matrix x {x.TypeName} with a column count {x.ColumnCount} and the matrix y {y.TypeName} with a row count of {y.RowCount}. They Must be equal.", nameof(x));
            return x.GenericMultiply(y);
        }

        private static void CheckElementType(KalkValue x, KalkValue y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (x.ElementType != y.ElementType)
            {
                throw new ArgumentException($"Unsupported type for matrix multiplication. The combination of {x.GetType().ScriptPrettyName()} * {y.GetType().ScriptPrettyName()} is not supported.", nameof(x));
            }
        }
    }

    public class KalkMatrix<T> : KalkMatrix, IFormattable, IList, IScriptCustomType
    {
        private readonly T[] _values;

        public KalkMatrix(int row, int column) : base(row, column)
        {
            _values = new T[row * column];
            VerifyElementType();
        }

        private KalkMatrix(int row, int column, T[] values) : base(row, column)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (values.Length != row * column) throw new ArgumentException($"Matrix values must have {row*column} elements instead of {values.Length}");
            _values = values;
            VerifyElementType();
        }

        public KalkMatrix(KalkMatrix<T> values) : base(values.RowCount, values.ColumnCount)
        {
            _values = (T[])values._values.Clone();
            VerifyElementType();
        }

        private static void VerifyElementType()
        {
            if (typeof(T) == typeof(bool) || typeof(T) == typeof(int) || typeof(T) == typeof(float) || typeof(T) == typeof(double))
            {
                return;
            }
            throw new InvalidOperationException($"The type {typeof(T)} is not supported for matrices. Only bool, int, float and double are supported.");
        }

        public override Type ElementType => typeof(T);

        public override Array Values => _values;

        public T this[int index]
        {
            get => _values[index];
            set => _values[index] = value;
        }


        public KalkVector<T> GetRow(int index)
        {
            if ((uint)index >= (uint)RowCount) throw new ArgumentOutOfRangeException(nameof(index));

            var row = new KalkVector<T>(ColumnCount);
            for (int i = 0; i < ColumnCount; i++)
            {
                row[i] = _values[ColumnCount * index + i];
            }

            return row;
        }

        public void SetRow(int index, KalkVector<T> value)
        {
            if ((uint)index >= (uint)RowCount) throw new ArgumentOutOfRangeException(nameof(index));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.Length != ColumnCount) throw new ArgumentOutOfRangeException(nameof(value), $"Invalid vector size. The vector has a length of {value.Length} while the row of this matrix is expecting {ColumnCount} elements.");

            for (int i = 0; i < ColumnCount; i++)
            {
                _values[ColumnCount * index + i] = value[i];
            }
        }

        public KalkVector<T> GetColumn(int index)
        {
            if ((uint)index >= (uint)RowCount) throw new ArgumentOutOfRangeException(nameof(index));

            var column = new KalkVector<T>(RowCount);
            for (int i = 0; i < RowCount; i++)
            {
                column[i] = _values[ColumnCount * i + index];
            }

            return column;
        }

        public void SetColumn(int index, KalkVector<T> value)
        {
            if ((uint)index >= (uint)RowCount) throw new ArgumentOutOfRangeException(nameof(index));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.Length != RowCount) throw new ArgumentOutOfRangeException(nameof(value), $"Invalid vector size. The vector has a length of {value.Length} while the column of this matrix is expecting {RowCount} elements.");

            for (int i = 0; i < RowCount; i++)
            {
                _values[ColumnCount * i + index] = value[i];
            }
        }

        public KalkMatrix<T> Clone()
        {
            return new KalkMatrix<T>(this);
        }

        public override IScriptObject Clone(bool deep)
        {
            return Clone();
        }

        public override string TypeName => $"{ElementTypeName}{RowCount.ToString(CultureInfo.InvariantCulture)}x{ColumnCount.ToString(CultureInfo.InvariantCulture)}";

        private string ElementTypeName => typeof(T).ScriptPrettyName();


        public object Transform(Func<T, T> apply)
        {
            var newValue = Clone();
            for (int i = 0; i < _values.Length; i++)
            {
                var result = apply(_values[i]);
                newValue._values[i] = (T)result;
            }
            return newValue;
        }

        public override bool CanTransform(Type transformType)
        {
            return typeof(T) == typeof(int) && (typeof(long) == transformType || typeof(int) == transformType) ||
                   (typeof(T) == typeof(float) && (typeof(double) == transformType || typeof(float) == transformType) ||
                    typeof(T) == typeof(double) && typeof(double) == transformType);
        }

        public override object Transform(TemplateContext context, SourceSpan span, Func<object, object> apply)
        {
            return this.Transform(value =>
            {
                var result = apply(value);
                return context.ToObject<T>(span, result);
            });
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            var context = formatProvider as TemplateContext;
            var builder = new StringBuilder();
            builder.Append(typeof(T).ScriptPrettyName()).Append(RowCount.ToString(CultureInfo.InvariantCulture))
                .Append('x').Append(ColumnCount.ToString(CultureInfo.InvariantCulture));
            builder.Append('(');
            for(int i = 0; i < _values.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                var valueToFormat = _values[i];
                builder.Append(context != null ? context.ObjectToString(valueToFormat) : valueToFormat is IFormattable formattable ? formattable.ToString(null, formatProvider) : valueToFormat.ToString());
            }
            builder.Append(')');
            return builder.ToString();
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection) _values).CopyTo(array, index);
        }

        public override int Count => RowCount;

        public bool IsSynchronized => ((ICollection) _values).IsSynchronized;

        public object SyncRoot => ((ICollection) _values).SyncRoot;

        public override IEnumerable<string> GetMembers()
        {
            if (_values == null) yield break;

            for(int i = 0; i < _values.Length; i++)
            {
                switch (i)
                {
                    case 0: yield return "x";
                        break;
                    case 1: yield return "y";
                        break;
                    case 2: yield return "z";
                        break;
                    case 3: yield return "w";
                        break;
                    default:
                        yield return $"{"xyzw"[i % 4]}{(i / 4 + 1).ToString(CultureInfo.InvariantCulture)}";
                        break;
                }
            }
        }

        public override bool Contains(string member)
        {
            if (member.Length < 1) return false;

            var emptySpan = new SourceSpan();
            foreach (var index in ForEachMemberPart(emptySpan, member, false))
            {
                // We say that 0 or 1 are members as well
                if (index < 0) return false;
            }

            return true;
        }

        public int Add(object value) => throw new NotSupportedException("This type does not support this operation");

        public void Clear()
        {
            Array.Clear(_values, 0, _values.Length);
        }

        public bool Contains(object value) => throw new NotSupportedException("This type does not support this operation");

        public int IndexOf(object value) => throw new NotSupportedException("This type does not support this operation");

        public void Insert(int index, object value) => throw new NotSupportedException("This type does not support this operation");

        public void Remove(object value) => throw new NotSupportedException("This type does not support this operation");

        public void RemoveAt(int index) => throw new NotSupportedException("This type does not support this operation");
        
        public bool IsFixedSize => ((IList) _values).IsFixedSize;

        public override bool IsReadOnly
        {
            get => false;
            set => throw new InvalidOperationException("Cannot set this instance readonly");
        }

        object IList.this[int index]
        {
            get => GetRow(index);
            set => SetRow(index, (KalkVector<T>)value);
        }

        public override bool TryGetValue(TemplateContext context, SourceSpan span, string member, out object result)
        {
            result = null;
            if (_values == null) return false;

            if (member.Length < 1) return false;
            List<T> list = null;
            foreach (var index in ForEachMemberPart(span, member, true))
            {
                var value = index < 0 ? context.ToObject<T>(span, index): _values[index];

                if (result == null)
                {
                    result = value;
                }
                else
                {
                    if (list == null)
                    {
                        list = new List<T>() { (T)result };
                    }
                    list.Add(value);
                }
            }

            if (list != null)
            {
                //result = new KalkMatrix<T>(list);
            }

            return true;
        }

        private IEnumerable<int> ForEachMemberPart(SourceSpan span, string member, bool throwIfInvalid)
        {
            for (int i = 0; i < member.Length; i++)
            {
                var c = member[i];
                int index;
                switch (c)
                {
                    case 'x':
                        index = 0;
                        break;
                    case 'y':
                        index = 1;
                        break;
                    case 'z':
                        index = 2;
                        break;
                    case 'w':
                        index = 3;
                        break;
                    default:
                        if (throwIfInvalid)
                        {
                            throw new ScriptRuntimeException(span, $"Invalid swizzle {c}. Expecting only x,y,z,w.");
                        }
                        else
                        {
                            index = -1;
                        }
                        break;
                }

                if (index < 0)
                {
                    yield return index;
                    break;
                }

                if (index < _values.Length)
                {
                    yield return index;
                }
                else
                {
                    if (throwIfInvalid)
                    {
                        throw new ScriptRuntimeException(span, $"Swizzle swizzle {c} is out of range ({index}) for this vector length ({RowCount})");
                    }
                    else
                    {
                        yield return -1;
                        break;
                    }
                }
            }
        }

        public override bool CanWrite(string member)
        {
            return true;
        }



        protected override KalkVector GenericMultiplyLeft(KalkVector x)
        {
            return MultiplyLeft((KalkVector<T>)x);
        }

        protected override KalkVector GenericMultiplyRight(KalkVector y)
        {
            return MultiplyRight((KalkVector<T>)y);
        }

        protected override KalkMatrix GenericMultiply(KalkMatrix y)
        {
            return Multiply((KalkMatrix<T>)y);
        }

        protected override object GenericDeterminant()
        {
            return Determinant();
        }

        protected override KalkMatrix GenericInverse()
        {
            return Inverse();
        }

        protected KalkVector<T> MultiplyLeft(KalkVector<T> x)
        {
            if (typeof(T) == typeof(float))
            {
                var matrix = Matrix<float>.Build.Dense(RowCount, ColumnCount, (float[])(object)_values);
                var vector = (Vector<float>)x.AsMathNetVector();
                var result = new KalkVector<float>(matrix.LeftMultiply(vector));
                return Unsafe.As<KalkVector<float>, KalkVector<T>>(ref result);
            }

            if (typeof(T) == typeof(double))
            {
                var matrix = Matrix<double>.Build.Dense(RowCount, ColumnCount, (double[])(object)_values);
                var vector = (Vector<double>)x.AsMathNetVector();
                var result = new KalkVector<double>(matrix.LeftMultiply(vector));
                return Unsafe.As<KalkVector<double>, KalkVector<T>>(ref result);
            }

            throw new ArgumentException($"The type {ElementType.ScriptPrettyName()} is not supported for this mul operation", nameof(x));
        }

        protected KalkVector<T> MultiplyRight(KalkVector<T> y)
        {
            if (typeof(T) == typeof(float))
            {
                var matrix = Matrix<float>.Build.Dense(RowCount, ColumnCount, (float[])(object)_values);
                var vector = (Vector<float>)y.AsMathNetVector();
                var result = new KalkVector<float>(matrix.Multiply(vector));
                return Unsafe.As<KalkVector<float>, KalkVector<T>>(ref result);
            }

            if (typeof(T) == typeof(double))
            {
                var matrix = Matrix<double>.Build.Dense(RowCount, ColumnCount, (double[])(object)_values);
                var vector = (Vector<double>)y.AsMathNetVector();
                var result = new KalkVector<double>(matrix.Multiply(vector));
                return Unsafe.As<KalkVector<double>, KalkVector<T>>(ref result);
            }

            throw new ArgumentException($"The type {ElementType.ScriptPrettyName()} is not supported for this mul operation", nameof(y));
        }

        protected KalkMatrix Multiply(KalkMatrix<T> y)
        {
            if (typeof(T) == typeof(float))
            {
                var mx = Matrix<float>.Build.Dense(RowCount, ColumnCount, (float[])(object)_values);
                var my = Matrix<float>.Build.Dense(RowCount, ColumnCount, (float[])(object)y._values);
                var result = mx.Multiply(my);
                var kalkResult = new KalkMatrix<float>(result.RowCount, result.ColumnCount, result.Storage.AsColumnMajorArray());
                return kalkResult;
            }

            if (typeof(T) == typeof(double))
            {
                var mx = Matrix<double>.Build.Dense(RowCount, ColumnCount, (double[])(object)_values);
                var my = Matrix<double>.Build.Dense(RowCount, ColumnCount, (double[])(object)y._values);
                var result = mx.Multiply(my);
                var kalkResult = new KalkMatrix<double>(result.RowCount, result.ColumnCount, result.Storage.AsColumnMajorArray());
                return kalkResult;
            }

            throw new ArgumentException($"The type {ElementType.ScriptPrettyName()} is not supported for this mul operation", nameof(y));
        }

        protected T Determinant()
        {
            if (typeof(T) == typeof(float))
            {
                var matrix = Matrix<float>.Build.Dense(RowCount, ColumnCount, (float[])(object)_values);
                var value = matrix.Determinant();
                return Unsafe.As<float, T>(ref value);
            }
            else if (typeof(T) == typeof(double))
            {
                var matrix = Matrix<double>.Build.Dense(RowCount, ColumnCount, (double[])(object)_values);
                var value = matrix.Determinant();
                return Unsafe.As<double, T>(ref value);
            }
            else
            {
                throw new InvalidOperationException($"Determinant can only be calculated for float or double matrices but not for {TypeName}.");
            }
        }

        protected KalkMatrix<T> Inverse()
        {
            if (typeof(T) == typeof(float))
            {
                var matrix = Matrix<float>.Build.Dense(RowCount, ColumnCount, (float[])(object)_values);
                matrix = matrix.Inverse();
                var result = new KalkMatrix<float>(matrix.RowCount, matrix.ColumnCount, matrix.Storage.AsColumnMajorArray());
                return Unsafe.As<KalkMatrix<float>, KalkMatrix<T>>(ref result);
            }
            else if (typeof(T) == typeof(double))
            {
                var matrix = Matrix<double>.Build.Dense(RowCount, ColumnCount, (double[])(object)_values);
                matrix = matrix.Inverse();
                var result = new KalkMatrix<double>(matrix.RowCount, matrix.ColumnCount, matrix.Storage.AsColumnMajorArray());
                return Unsafe.As<KalkMatrix<double>, KalkMatrix<T>>(ref result);
            }
            else
            {
                throw new InvalidOperationException("Determinant can only be calculated for float or double matrices.");
            }
        }

        protected override KalkMatrix Transpose()
        {
            var transpose = new KalkMatrix<T>(ColumnCount, RowCount);
            for(int y =0; y < RowCount; y++)
            {
                for(int x = 0; x < ColumnCount; x++)
                {
                    transpose[RowCount * x + y] = this[ColumnCount * y + x];
                }
            }
            return transpose;
        }
        
        protected override KalkMatrix Identity()
        {
            if (RowCount != ColumnCount) throw new InvalidOperationException($"Matrix must be square nxn instead of {TypeName}");

            var transpose = new KalkMatrix<T>(ColumnCount, RowCount);
            if (typeof(T) == typeof(bool))
            {
                var m = (KalkMatrix<bool>) (KalkMatrix) transpose;
                int count = RowCount;
                for (int i = 0; i < count; i++)
                    m[ColumnCount * i + i] = true;
            }
            else if (typeof(T) == typeof(int))
            {
                var m = (KalkMatrix<int>)(KalkMatrix)transpose;
                int count = RowCount;
                for (int i = 0; i < count; i++)
                    m[ColumnCount * i + i] = 1;
            }
            else if (typeof(T) == typeof(float))
            {
                var m = (KalkMatrix<float>)(KalkMatrix)transpose;
                int count = RowCount;
                for (int i = 0; i < count; i++)
                    m[ColumnCount * i + i] = 1.0f;
            }
            else if (typeof(T) == typeof(double))
            {
                var m = (KalkMatrix<double>)(KalkMatrix)transpose;
                int count = RowCount;
                for (int i = 0; i < count; i++)
                    m[ColumnCount * i + i] = 1.0;
            }
            return transpose;
        }

        protected override KalkVector Diagonal()
        {
            if (RowCount != ColumnCount) throw new InvalidOperationException($"Matrix must be square nxn instead of {TypeName}");

            if (typeof(T) == typeof(bool))
            {
                var m = (KalkMatrix<bool>)(KalkMatrix)this;
                int count = RowCount;
                var result = new KalkVector<bool>(RowCount);
                for (int i = 0; i < count; i++)
                    result[i] = m[ColumnCount * i + i];
                return result;
            }
            else if (typeof(T) == typeof(int))
            {
                var m = (KalkMatrix<int>)(KalkMatrix)this;
                int count = RowCount;
                var result = new KalkVector<int>(RowCount);
                for (int i = 0; i < count; i++)
                    result[i] = m[ColumnCount * i + i];
                return result;
            }
            else if (typeof(T) == typeof(float))
            {
                var m = (KalkMatrix<float>)(KalkMatrix)this;
                int count = RowCount;
                var result = new KalkVector<float>(RowCount);
                for (int i = 0; i < count; i++)
                    result[i] = m[ColumnCount * i + i];
                return result;

            }
            else if (typeof(T) == typeof(double))
            {
                var m = (KalkMatrix<double>)(KalkMatrix)this;
                int count = RowCount;
                var result = new KalkVector<double>(RowCount);
                for (int i = 0; i < count; i++)
                    result[i] = m[ColumnCount * i + i];
                return result;
            }

            throw new InvalidOperationException($"Type {ElementType} is not supported for this operation");
        }

        public override bool TrySetValue(TemplateContext context, SourceSpan span, string member, object value, bool readOnly)
        {
            var tValue = context.ToObject<T>(span, value);
            foreach (var index in ForEachMemberPart(span, member, true))
            {
                if (index < 0) throw new ScriptRuntimeException(span, $"Swizzle with 0 or 1 are not supporting when setting values");
                _values[index] = tValue;
            }

            return true;
        }

        public override bool Remove(string member)
        {
            throw new InvalidOperationException($"Cannot remove member {member} for {this.GetType()}");
        }

        public override void SetReadOnly(string member, bool readOnly)
        {
            throw new InvalidOperationException($"Cannot set readonly member {member} for {this.GetType()}");
        }

        public bool TryEvaluate(TemplateContext context, SourceSpan span, ScriptBinaryOperator op, SourceSpan leftSpan, object leftValue, SourceSpan rightSpan, object rightValue, out object result)
        {
            if (leftValue is KalkExpression leftExpression)
            {
                return leftExpression.TryEvaluate(context, span, op, leftSpan, leftValue, rightSpan, rightValue, out result);
            }

            if (rightValue is KalkExpression rightExpression)
            {
                return rightExpression.TryEvaluate(context, span, op, leftSpan, leftValue, rightSpan, rightValue, out result);
            }

            result = null;
            var leftMatrix = leftValue as KalkMatrix<T>;
            var rightMatrix = rightValue as KalkMatrix<T>;
            if (leftMatrix == null && rightMatrix == null) return false;
            if (leftMatrix != null && rightMatrix != null && (leftMatrix.RowCount != rightMatrix.RowCount || leftMatrix.ColumnCount != rightMatrix.ColumnCount))
            {
                return false;
            }

            if (leftMatrix == null)
            {
                leftMatrix = new KalkMatrix<T>(rightMatrix.RowCount, rightMatrix.ColumnCount);
                var leftComponentValue = context.ToObject<T>(span, leftValue);
                for (int i = 0; i < leftMatrix._values.Length; i++)
                {
                    leftMatrix[i] = leftComponentValue;
                }
            }

            if (rightMatrix == null)
            {
                rightMatrix = new KalkMatrix<T>(leftMatrix.RowCount, leftMatrix.ColumnCount);
                var rightComponentValue = context.ToObject<T>(span, rightValue);
                for (int i = 0; i < rightMatrix._values.Length; i++)
                {
                    rightMatrix[i] = rightComponentValue;
                }
            }

            switch (op)
            {
                case ScriptBinaryOperator.CompareEqual:
                case ScriptBinaryOperator.CompareNotEqual:
                case ScriptBinaryOperator.CompareLessOrEqual:
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                case ScriptBinaryOperator.CompareLess:
                case ScriptBinaryOperator.CompareGreater:
                    var vbool = new KalkMatrix<bool>(leftMatrix.RowCount, leftMatrix.ColumnCount);
                    for (int i = 0; i < vbool._values.Length; i++)
                    {
                        vbool[i] = (bool)ScriptBinaryExpression.Evaluate(context, span, op, leftMatrix._values[i], rightMatrix._values[i]);
                    }
                    result = vbool;
                    return true;

                case ScriptBinaryOperator.Add:
                case ScriptBinaryOperator.Substract:
                case ScriptBinaryOperator.Multiply:
                case ScriptBinaryOperator.Divide:
                case ScriptBinaryOperator.DivideRound:
                case ScriptBinaryOperator.Modulus:
                case ScriptBinaryOperator.ShiftLeft:
                case ScriptBinaryOperator.ShiftRight:
                case ScriptBinaryOperator.Power:
                {
                    var opResult = new KalkMatrix<T>(leftMatrix.RowCount, leftMatrix.ColumnCount);
                    for (int i = 0; i < opResult._values.Length; i++)
                    {
                        opResult[i] = context.ToObject<T>(span, ScriptBinaryExpression.Evaluate(context, span, op, leftMatrix._values[i], rightMatrix._values[i]));
                    }

                    result = opResult;
                    return true;
                }
            }

            return false;
        }

        public bool TryConvertTo(TemplateContext context, SourceSpan span, Type type, out object value)
        {
            value = null;
            return false;
        }

        public bool Equals(KalkMatrix<T> other)
        {
            if (other == null) return false;
            if (RowCount != other.RowCount || ColumnCount != other.ColumnCount) return false;
            var values = _values;
            var otherValues = other._values;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].Equals(otherValues[i])) return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((KalkMatrix<T>) obj);
        }

        public override int GetHashCode()
        {
            var values = _values;
            int hashCode = (RowCount * 397) ^ ColumnCount;
            for (int i = 0; i < values.Length; i++)
            {
                hashCode = (hashCode * 397) ^ values[i].GetHashCode();
            }
            return hashCode;
        }

        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < RowCount; i++)
            {
                yield return GetRow(i);
            }
        }

        public bool TryEvaluate(TemplateContext context, SourceSpan span, ScriptUnaryOperator op, object rightValue, out object result)
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(KalkMatrix<T> left, KalkMatrix<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(KalkMatrix<T> left, KalkMatrix<T> right)
        {
            return !Equals(left, right);
        }
    }
}