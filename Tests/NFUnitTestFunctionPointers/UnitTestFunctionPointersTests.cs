// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using nanoFramework.TestFramework;

namespace NFUnitTestFunctionPointers
{
    [TestClass]
    public unsafe class UnitTestFunctionPointersTests
    {
        // targets
        private static int Add(int a, int b) => a + b;
        private static int Mul(int a, int b) => a * b;
        private static void Increment(ref int value) => value++;
        private static string Describe(string prefix, object value) => prefix + (value == null ? "null" : value.ToString());
        private static Point Swap(Point p) => new Point(p.Y, p.X);
        private static long AddLong(long a, long b) => a + b;
        private static double Half(double value) => value / 2;
        private static int Thrower(int value) => throw new InvalidOperationException("boom");
        private static int Factorial(int n) => n <= 1 ? 1 : n * s_factorial(n - 1);

        private static int Sum16(int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8, int a9, int a10, int a11, int a12, int a13, int a14, int a15, int a16)
            => a1 + a2 + a3 + a4 + a5 + a6 + a7 + a8 + a9 + a10 + a11 + a12 + a13 + a14 + a15 + a16;
        private static string Mixed(byte a, sbyte b, short c, ushort d, char e, bool f, uint g, ulong h, float i)
            => a.ToString() + "|" + b.ToString() + "|" + c.ToString() + "|" + d.ToString() + "|" + e.ToString() + "|" + f.ToString() + "|" + g.ToString() + "|" + h.ToString() + "|" + i.ToString();
        private static void Split(int value, out int doubled, out string text)
        {
            doubled = value * 2;
            text = "v" + value.ToString();
        }
        private static ref int ElementAt(int[] values, int index) => ref values[index];
        private static Large Echo(Large value)
        {
            value.Name = value.Name.ToUpper();
            value.P = new Point(value.P.Y, value.P.X);
            return value;
        }
        private static delegate*<int, int, int> Twice(delegate*<int, int, int> operation) => operation;

        private static int s_counter;
        private static void Bump() => s_counter++;

        // storage
        private static delegate*<int, int> s_factorial = &Factorial;
        private static delegate*<int, int, int> s_operation = &Add;
        private static delegate*<int, int, int> s_null;
        private delegate*<int, int, int> _operation;

        private delegate int BinaryOperation(int a, int b);

        private static int Apply(delegate*<int, int, int> operation, int a, int b) => operation(a, b);

        private static delegate*<int, int, int> Pick(bool add) => add ? &Add : &Mul;

        private int InstanceApply(int value)
        {
            _operation = &Mul;
            return _operation(value, value);
        }

        [TestMethod]
        public void FunctionPointer_CallThroughLocal()
        {
            delegate*<int, int, int> operation = &Add;
            Assert.AreEqual(5, operation(2, 3));

            operation = &Mul;
            Assert.AreEqual(6, operation(2, 3));
        }

        [TestMethod]
        public void FunctionPointer_StoredInStaticField()
        {
            s_operation = &Add;
            Assert.AreEqual(7, s_operation(3, 4));

            s_operation = &Mul;
            Assert.AreEqual(12, s_operation(3, 4));
        }

        [TestMethod]
        public void FunctionPointer_StoredInInstanceField()
        {
            Assert.AreEqual(49, new UnitTestFunctionPointersTests().InstanceApply(7));
        }

        [TestMethod]
        public void FunctionPointer_PassedAsParameterAndReturned()
        {
            Assert.AreEqual(9, Apply(&Add, 4, 5));
            Assert.AreEqual(20, Apply(&Mul, 4, 5));
            Assert.AreEqual(11, Pick(true)(5, 6));
            Assert.AreEqual(30, Pick(false)(5, 6));
        }

        [TestMethod]
        public void FunctionPointer_ByRefParameter()
        {
            delegate*<ref int, void> increment = &Increment;
            int value = 41;

            increment(ref value);

            Assert.AreEqual(42, value);
        }

        [TestMethod]
        public void FunctionPointer_ReferenceTypeParameters()
        {
            delegate*<string, object, string> describe = &Describe;

            Assert.AreEqual("v=7", describe("v=", 7));
            Assert.AreEqual("v=null", describe("v=", null));
        }

        [TestMethod]
        public void FunctionPointer_ValueTypeParameters()
        {
            delegate*<Point, Point> swap = &Swap;
            Point swapped = swap(new Point(1, 2));
            Assert.AreEqual(2, swapped.X);
            Assert.AreEqual(1, swapped.Y);

            delegate*<long, long, long> addLong = &AddLong;
            Assert.AreEqual(0x100000001L, addLong(0x100000000L, 1L));

            delegate*<double, double> half = &Half;
            Assert.AreEqual(1.25, half(2.5));
        }

        [TestMethod]
        public void FunctionPointer_VoidWithoutParameters()
        {
            delegate*<void> bump = &Bump;
            s_counter = 0;

            bump();
            bump();

            Assert.AreEqual(2, s_counter);
        }

        [TestMethod]
        public void FunctionPointer_Recursion()
        {
            Assert.AreEqual(120, s_factorial(5));
        }

        [TestMethod]
        public void FunctionPointer_TargetInAnotherClass()
        {
            delegate*<int, int> triple = &OtherClass.Triple;
            Assert.AreEqual(21, triple(7));
        }

        [TestMethod]
        public void FunctionPointer_TargetInCoreLibrary()
        {
            delegate*<string, string, string> concat = &string.Concat;
            Assert.AreEqual("nanoFramework", concat("nano", "Framework"));

            delegate*<string, bool> isNullOrEmpty = &string.IsNullOrEmpty;
            Assert.IsTrue(isNullOrEmpty(""));
            Assert.IsFalse(isNullOrEmpty("x"));
        }

        [TestMethod]
        public void FunctionPointer_Array()
        {
            delegate*<int, int, int>[] operations = new delegate*<int, int, int>[2];
            operations[0] = &Add;
            operations[1] = &Mul;

            Assert.AreEqual(7, operations[0](3, 4));
            Assert.AreEqual(12, operations[1](3, 4));
        }

        [TestMethod]
        public void FunctionPointer_Comparisons()
        {
            delegate*<int, int, int> add1 = &Add;
            delegate*<int, int, int> add2 = &Add;
            delegate*<int, int, int> mul = &Mul;

            Assert.IsTrue(add1 == add2);
            Assert.IsFalse(add1 == mul);
            Assert.IsTrue(add1 != mul);
            Assert.IsTrue(add1 != null);
            Assert.IsTrue(s_null == null);
        }

        [TestMethod]
        public void FunctionPointer_NullCallThrowsNullReferenceException()
        {
            Assert.ThrowsException(typeof(NullReferenceException), CallNull);
        }

        [TestMethod]
        public void FunctionPointer_ExceptionPropagatesFromTarget()
        {
            Assert.ThrowsException(typeof(InvalidOperationException), CallThrower);
        }

        [TestMethod]
        public void FunctionPointer_InvalidPointerIsRejected()
        {
            bool caught = false;

            try
            {
                delegate*<int, int, int> forged = (delegate*<int, int, int>)0x7FFF7FFF;
                forged(1, 2);
            }
            catch (Exception)
            {
                caught = true;
            }

            Assert.IsTrue(caught);
        }

        [TestMethod]
        public void FunctionPointer_IncompatibleSignatureIsRejected()
        {
            delegate*<string, string, string> concat = &string.Concat;
            delegate*<int, int, string> wrongParameters = (delegate*<int, int, string>)concat;
            delegate*<string, string, int> wrongReturn = (delegate*<string, string, int>)concat;
            bool caughtParameters = false;
            bool caughtReturn = false;

            try
            {
                wrongParameters(1, 2);
            }
            catch (Exception)
            {
                caughtParameters = true;
            }

            try
            {
                wrongReturn("a", "b");
            }
            catch (Exception)
            {
                caughtReturn = true;
            }

            Assert.IsTrue(caughtParameters);
            Assert.IsTrue(caughtReturn);
            Assert.AreEqual("ab", concat("a", "b"));
        }

        [TestMethod]
        public void FunctionPointer_SameCallSiteDifferentTargets()
        {
            // the runtime caches validated call site / target pairs: every new target still has to be validated
            delegate*<string, string, string> concat = &string.Concat;
            delegate*<int, int, int> incompatible = (delegate*<int, int, int>)concat;
            int results = 0;
            int rejected = 0;

            for (int i = 0; i < 3; i++)
            {
                results += CallWithOneAndTwo(&Add);

                try
                {
                    CallWithOneAndTwo(incompatible);
                }
                catch (Exception)
                {
                    rejected++;
                }

                results += CallWithOneAndTwo(&Mul);
            }

            Assert.AreEqual(15, results);
            Assert.AreEqual(3, rejected);
        }

        private static int CallWithOneAndTwo(delegate*<int, int, int> function) => function(1, 2);

        [TestMethod]
        public void Delegate_StillWorksForSameTarget()
        {
            // delegates use the same ldftn instruction as function pointers
            BinaryOperation add = Add;
            BinaryOperation mul = new BinaryOperation(Mul);
            BinaryOperation add2 = Add;

            Assert.AreEqual(5, add(2, 3));
            Assert.AreEqual(6, mul(2, 3));
            Assert.IsTrue(add.Equals(add2));
            Assert.IsFalse(add.Equals(mul));

            delegate*<int, int, int> pointer = &Add;
            Assert.AreEqual(add(10, 20), pointer(10, 20));
        }

        [TestMethod]
        public void FunctionPointer_ManyParameters()
        {
            delegate*<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int> sum = &Sum16;

            Assert.AreEqual(136, sum(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16));
        }

        [TestMethod]
        public void FunctionPointer_FortyMixedParameters()
        {
            delegate*<int, long, double, string, Point, int, long, double, string, Point, int, long, double, string, Point, int, long, double, string, Point, int, long, double, string, Point, int, long, double, string, Point, int, long, double, string, Point, int, long, double, string, Point, string> many = &Many40;
            var p = new Point(1, 2);

            string expected = Many40(1, 2, 0.5, "a", p, 1, 2, 0.5, "b", p, 1, 2, 0.5, "c", p, 1, 2, 0.5, "d", p, 1, 2, 0.5, "e", p, 1, 2, 0.5, "f", p, 1, 2, 0.5, "g", p, 1, 2, 0.5, "h", p);

            Assert.AreEqual("52|abcdefgh", expected);
            Assert.AreEqual(expected, many(1, 2, 0.5, "a", p, 1, 2, 0.5, "b", p, 1, 2, 0.5, "c", p, 1, 2, 0.5, "d", p, 1, 2, 0.5, "e", p, 1, 2, 0.5, "f", p, 1, 2, 0.5, "g", p, 1, 2, 0.5, "h", p));
        }

        private static string Many40(
            int a0, long a1, double a2, string a3, Point a4, int b0, long b1, double b2, string b3, Point b4,
            int c0, long c1, double c2, string c3, Point c4, int d0, long d1, double d2, string d3, Point d4,
            int e0, long e1, double e2, string e3, Point e4, int f0, long f1, double f2, string f3, Point f4,
            int g0, long g1, double g2, string g3, Point g4, int h0, long h1, double h2, string h3, Point h4)
        {
            double sum = a0 + a1 + a2 + a4.X + a4.Y + b0 + b1 + b2 + b4.X + b4.Y
                + c0 + c1 + c2 + c4.X + c4.Y + d0 + d1 + d2 + d4.X + d4.Y
                + e0 + e1 + e2 + e4.X + e4.Y + f0 + f1 + f2 + f4.X + f4.Y
                + g0 + g1 + g2 + g4.X + g4.Y + h0 + h1 + h2 + h4.X + h4.Y;

            return ((int)sum).ToString() + "|" + a3 + b3 + c3 + d3 + e3 + f3 + g3 + h3;
        }

        private static delegate*<int, int, int> s_threadTarget;
        private static int s_threadResult;

        private static void ThreadWorker() => s_threadResult = s_threadTarget(20, 22);

        [TestMethod]
        public void FunctionPointer_ReferenceTypeVariance()
        {
            // as in .NET, method groups and function pointers convert with reference type variance:
            // parameters are contravariant, the return type is covariant
            delegate*<string, object> objectToString = &ObjectToString;
            Assert.AreEqual("s:x", (string)objectToString("x"));

            delegate*<FpDog, FpAnimal> makeDog = &MakeDog;
            Assert.AreEqual("dog", makeDog(new FpDog()).Name);

            delegate*<FpThing, int> thingId = &ThingId;
            Assert.AreEqual(7, thingId(new FpThing()));

            delegate*<IFpThing> makeThing = &MakeThing;
            Assert.AreEqual(7, makeThing().Id);

            delegate*<string[], int> objectArrayLength = &ObjectArrayLength;
            Assert.AreEqual(2, objectArrayLength(new string[2]));

            delegate*<int[], int> arrayLength = &ArrayLength;
            Assert.AreEqual(3, arrayLength(new int[3]));

            // implicit conversion between function pointer types
            delegate*<object, string> exact = &ObjectToString;
            delegate*<string, object> converted = exact;
            Assert.AreEqual("s:y", (string)converted("y"));
        }

        [TestMethod]
        public void FunctionPointer_IncompatibleVarianceIsRejected()
        {
            // conversions that C# doesn't allow (forged with casts) are rejected
            delegate*<int, object> boxing = (delegate*<int, object>)(delegate*<object, string>)&ObjectToString;
            delegate*<FpStructThing, int> structToInterface = (delegate*<FpStructThing, int>)(delegate*<IFpThing, int>)&ThingId;
            delegate*<ref string, void> byRef = (delegate*<ref string, void>)(delegate*<ref object, void>)&SetObject;
            delegate*<FpDog> returnType = (delegate*<FpDog>)(delegate*<FpAnimal>)&MakeAnimal;
            delegate*<int[], int> valueTypeArray = (delegate*<int[], int>)(delegate*<object[], int>)&ObjectArrayLength;
            delegate*<FpAnimal, string> downcast = (delegate*<FpAnimal, string>)(delegate*<FpDog, string>)&DogName;
            string value = "x";
            int rejected = 0;

            try
            {
                boxing(1);
            }
            catch (Exception)
            {
                rejected++;
            }

            try
            {
                structToInterface(new FpStructThing());
            }
            catch (Exception)
            {
                rejected++;
            }

            try
            {
                byRef(ref value);
            }
            catch (Exception)
            {
                rejected++;
            }

            try
            {
                returnType();
            }
            catch (Exception)
            {
                rejected++;
            }

            try
            {
                valueTypeArray(new int[1]);
            }
            catch (Exception)
            {
                rejected++;
            }

            try
            {
                downcast(new FpAnimal());
            }
            catch (Exception)
            {
                rejected++;
            }

            Assert.AreEqual(6, rejected);
            Assert.AreEqual("x", value);
        }

        private static string ObjectToString(object value) => "s:" + value.ToString();
        private static FpDog MakeDog(FpAnimal animal) => new FpDog();
        private static FpAnimal MakeAnimal() => new FpAnimal();
        private static string DogName(FpDog dog) => dog.Name;
        private static int ThingId(IFpThing thing) => thing.Id;
        private static FpThing MakeThing() => new FpThing();
        private static int ObjectArrayLength(object[] array) => array.Length;
        private static int ArrayLength(Array array) => array.Length;
        private static void SetObject(ref object value) => value = 5;

        [TestMethod]
        public void FunctionPointer_CalledFromAnotherThread()
        {
            s_threadTarget = &Add;
            s_threadResult = 0;

            var thread = new System.Threading.Thread(ThreadWorker);
            thread.Start();
            thread.Join();

            Assert.AreEqual(42, s_threadResult);
        }

        [TestMethod]
        public void FunctionPointer_MixedParameterTypes()
        {
            delegate*<byte, sbyte, short, ushort, char, bool, uint, ulong, float, string> mixed = &Mixed;

            Assert.AreEqual("1|-2|-3|4|z|True|5|6|1.5", mixed(1, -2, -3, 4, 'z', true, 5, 6, 1.5f));
        }

        [TestMethod]
        public void FunctionPointer_OutParameters()
        {
            delegate*<int, out int, out string, void> split = &Split;

            split(21, out int doubled, out string text);

            Assert.AreEqual(42, doubled);
            Assert.AreEqual("v21", text);
        }

        [TestMethod]
        public void FunctionPointer_RefReturn()
        {
            delegate*<int[], int, ref int> elementAt = &ElementAt;
            int[] values = new int[] { 1, 2, 3 };

            elementAt(values, 1) = 20;

            Assert.AreEqual(20, values[1]);
        }

        [TestMethod]
        public void FunctionPointer_LargeStruct()
        {
            delegate*<Large, Large> echo = &Echo;
            Large large = new Large { A = 1, B = 2, C = 3, D = 4, Name = "large", P = new Point(5, 6) };

            Large result = echo(large);

            Assert.AreEqual(10L, result.A + result.B + result.C + result.D);
            Assert.AreEqual("LARGE", result.Name);
            Assert.AreEqual(6, result.P.X);
        }

        [TestMethod]
        public void FunctionPointer_FunctionPointerParametersAndReturn()
        {
            delegate*<delegate*<int, int, int>, delegate*<int, int, int>> twice = &Twice;
            delegate*<delegate*<int, int, int>, int, int, int> apply = &Apply;

            Assert.AreEqual(5, apply(twice(&Add), 2, 3));
            Assert.AreEqual(6, apply(twice(&Mul), 2, 3));
        }

        [TestMethod]
        public void FunctionPointer_StoredInStruct()
        {
            Holder holder = new Holder { Operation = &Mul };

            Assert.AreEqual(12, holder.Operation(3, 4));
        }

        [TestMethod]
        public void FunctionPointer_NestedLocalFunctionTarget()
        {
            delegate*<int, int> square = &Square;

            Assert.AreEqual(49, square(7));

            static int Square(int value) => value * value;
        }

        private static void CallNull()
        {
            s_null(1, 2);
        }

        private static void CallThrower()
        {
            delegate*<int, int> thrower = &Thrower;
            thrower(1);
        }

        [TestMethod]
        public void FunctionPointer_VirtualOverloadsDifferingByFunctionPointerType()
        {
            // a function pointer parameter is stored as I4, its signature is kept so it doesn't match V(int)
            FunctionPointerBase instance = new FunctionPointerDerived();

            Assert.AreEqual(1011, instance.V(&AddOne), "override of V(delegate*<int, int>)");
            Assert.AreEqual(-5, instance.V(5), "V(int) must not be overridden by V(delegate*<int, int>)");
            Assert.AreEqual(-5, instance.W(5), "W(int) must not be hijacked by W(delegate*<int, int>)");
            Assert.AreEqual(2, instance.X(&ReturnZeroLong), "override of X(delegate*<long>)");
            Assert.AreEqual(-1, instance.X(&ReturnZero), "X(delegate*<int>) must not be overridden by X(delegate*<long>)");
        }

        [TestMethod]
        public void FunctionPointer_InterfaceOverloadsDifferingByFunctionPointerType()
        {
            IFunctionPointerConsumer consumer = new FunctionPointerConsumer();

            Assert.AreEqual(503, consumer.Consume(&AddOne));
            Assert.AreEqual(50, consumer.Consume(5));
            Assert.AreEqual(10, consumer.Consume(&ReturnZero));
            Assert.AreEqual(20, consumer.Consume(&ReturnZeroLong));
        }

        [TestMethod]
        public void FunctionPointer_ArrayAndByRefOverloads()
        {
            IFunctionPointerConsumer consumer = new FunctionPointerConsumer();
            delegate*<int, int>[] array = new delegate*<int, int>[] { &AddOne, &OtherClass.Triple };
            delegate*<int, int> f = &AddOne;
            int i = 0;

            Assert.AreEqual(4 + 9, consumer.Sum(array, 3));
            Assert.AreEqual(-1, consumer.Sum(new int[2], 3));

            consumer.Set(ref f, &OtherClass.Triple);
            consumer.Set(ref i, 4);

            Assert.AreEqual(9, f(3));
            Assert.AreEqual(-4, i);
        }

        [TestMethod]
        public void FunctionPointer_SizeOf()
        {
            // function pointers are stored as 32 bits values
            Assert.AreEqual(4, sizeof(delegate*<int, int>));
            Assert.AreEqual(4, sizeof(delegate*<string, long, void>));
        }

        private static int AddOne(int value) => value + 1;

        private static int ReturnZero() => 0;

        private static long ReturnZeroLong() => 0;

        [TestMethod]
        public void FunctionPointer_GenericInstantiationIdentity()
        {
            delegate*<int, int> direct = &GenericHolder<int>.Identity;

            // the same instantiation reached from different generic contexts is the same function pointer
            Assert.IsTrue(GenericHolder<int>.Self() == direct);
            Assert.IsTrue(GenericHolder<int>.FromMethod<string>() == direct);
            Assert.IsTrue(GetIdentity<int>() == direct);

            // different instantiations are different function pointers
            Assert.IsTrue((nint)GetIdentity<long>() != (nint)GetIdentity<int>());
            Assert.IsTrue((nint)GetArrayIdentity<int>() != (nint)GetIdentity<int>());

            delegate*<int[], int[]> directArray = &GenericHolder<int[]>.Identity;
            Assert.IsTrue(GetArrayIdentity<int>() == directArray);

            Assert.AreEqual(7, GenericHolder<int>.FromMethod<string>()(7));
            Assert.AreEqual(3, GetArrayIdentity<int>()(new int[3]).Length);
        }

        private static delegate*<T, T> GetIdentity<T>() => &GenericHolder<T>.Identity;

        private static delegate*<T[], T[]> GetArrayIdentity<T>() => &GenericHolder<T[]>.Identity;
    }


    public unsafe class FunctionPointerBase
    {
        public virtual int V(delegate*<int, int> f) => f(1);

        public virtual int V(int value) => -value;

        public virtual int W(int value) => -value;

        public virtual int X(delegate*<int> f) => -1;

        public virtual int X(delegate*<long> f) => -2;
    }

    public unsafe class FunctionPointerDerived : FunctionPointerBase
    {
        public override int V(delegate*<int, int> f) => f(10) + 1000;

        // new overload, not an override of W(int)
        public virtual int W(delegate*<int, int> f) => 7777;

        public override int X(delegate*<long> f) => 2;
    }

    public unsafe interface IFunctionPointerConsumer
    {
        int Consume(delegate*<int, int> f);

        int Consume(int value);

        int Consume(delegate*<int> f);

        int Consume(delegate*<long> f);

        int Sum(delegate*<int, int>[] functions, int value);

        int Sum(int[] values, int value);

        void Set(ref delegate*<int, int> f, delegate*<int, int> value);

        void Set(ref int f, int value);
    }

    public unsafe class FunctionPointerConsumer : IFunctionPointerConsumer
    {
        public int Consume(delegate*<int, int> f) => f(2) + 500;

        public int Consume(int value) => value * 10;

        public int Consume(delegate*<int> f) => 10 + f();

        public int Consume(delegate*<long> f) => 20 + (int)f();

        public int Sum(delegate*<int, int>[] functions, int value)
        {
            int sum = 0;

            for (int i = 0; i < functions.Length; i++)
            {
                sum += functions[i](value);
            }

            return sum;
        }

        public int Sum(int[] values, int value) => -1;

        public void Set(ref delegate*<int, int> f, delegate*<int, int> value) => f = value;

        public void Set(ref int f, int value) => f = -value;
    }

    public static unsafe class GenericHolder<T>
    {
        public static T Identity(T value) => value;

        public static delegate*<T, T> Self() => &Identity;

        public static delegate*<T, T> FromMethod<U>() => &GenericHolder<T>.Identity;
    }

    public static class OtherClass
    {
        public static int Triple(int value) => value * 3;
    }

    public struct Large
    {
        public long A;
        public long B;
        public long C;
        public long D;
        public string Name;
        public Point P;
    }

    public unsafe struct Holder
    {
        public delegate*<int, int, int> Operation;
    }

    public struct Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class FpAnimal
    {
        public virtual string Name => "animal";
    }

    public class FpDog : FpAnimal
    {
        public override string Name => "dog";
    }

    public interface IFpThing
    {
        int Id { get; }
    }

    public class FpThing : IFpThing
    {
        public int Id => 7;
    }

    public struct FpStructThing : IFpThing
    {
        public int Id => 9;
    }
}
