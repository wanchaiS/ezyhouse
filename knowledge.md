# C# and .NET Knowledge Base

## Contents

1. [Purpose and how to use this file](#1-purpose-and-how-to-use-this-file)
2. [.NET platform fundamentals](#2-net-platform-fundamentals)
3. [C# language basics](#3-c-language-basics)
4. [Object-oriented programming](#4-object-oriented-programming)
5. [Memory and resources](#5-memory-and-resources)
6. [Error handling](#6-error-handling)
7. [File I/O](#7-file-io)
8. [Generics and collections](#8-generics-and-collections)
9. [Delegates, lambdas and LINQ](#9-delegates-lambdas-and-linq)
10. [GUI with Windows Forms](#10-gui-with-windows-forms)
11. [Entity Framework Core](#11-entity-framework-core)
12. [Unit testing with NUnit](#12-unit-testing-with-nunit)
13. [Mapping to Assignment 2 requirements](#13-mapping-to-assignment-2-requirements)
14. [Corrections to the lecture slides](#14-corrections-to-the-lecture-slides)

---

## 1. Purpose and how to use this file

This is a topic-by-topic summary of the 32998/31927 *.NET Application Development* lectures (Weeks 1-9), written as the reference for building our Assignment 2 application. Each section notes the week(s) it comes from.

- The lecture slides' code examples were images, so the snippets here are written fresh to illustrate each concept. Where a slide example was wrong, the snippet shows the corrected form, and [Section 14](#14-corrections-to-the-lecture-slides) lists every correction.
- [Section 13](#13-mapping-to-assignment-2-requirements) maps each Assignment 2 marking item to the section that covers it. Start there when planning features.

---

## 2. .NET platform fundamentals

*Source: Week 1*

### What .NET is

- .NET is a free (since November 2014), open-source, cross-platform development platform for web, mobile, desktop, gaming and IoT apps.
- It supports several languages (C#, F#, Visual Basic) that share one consistent API and set of libraries.
- C# descends from C (1970s), C++ (1980s) and Java (mid-1990s). Microsoft announced it in the 2000s specifically for .NET. .NET itself followed earlier Windows inter-process technologies: DDE, then OLE, then COM/COM+/DCOM.

### The three historical flavours, now unified

- **.NET Framework**: the original, Windows-only platform. It ships with Windows and provides memory management, type safety, security, networking and deployment. Use it only when you need technology that never moved to .NET Core, such as ASP.NET Web Forms.
- **.NET Core**: the open-source (MIT), cross-platform rewrite for Windows, macOS and Linux. It is made up of the runtime, the framework libraries, the SDK and compilers, and the `dotnet` app host. It suits cross-platform apps, microservices, Docker, high-performance systems, side-by-side versions and command-line control.
- **Xamarin/Mono**: .NET for iOS and Android. Xamarin was discontinued in May 2024 and replaced by **.NET MAUI** (Multi-platform App UI).
- Since 2020 these have merged into a single **.NET**:
  - .NET 5 (2020) unified the platforms.
  - .NET 6 (2021) added Arm64 and Apple Silicon support.
  - .NET 7 (2022) added OpenAPI support.
  - .NET 8 (2023) added built-in third-party tooling.
  - .NET 9 (2024) added cloud-native support.
  - .NET 10 (2025) ships with C# 14.
- **Assignment 2 requires Visual Studio 2022 with .NET 9.0 or higher.**

### Core runtime components

- **CLR (Common Language Runtime)**: the execution engine. It manages code execution, memory (garbage collection) and type safety, and it makes .NET code language- and platform-independent.
- **FCL (Framework Class Library)**: the large library of tested, reusable classes, interfaces and value types. It covers data types, data structures, data access, networking, GUI and more.
- **Managed code**: code whose execution the CLR manages (C#, F#, VB). **Unmanaged code** (for example C/C++) leaves memory and safety entirely to the programmer.
- **JIT (Just-In-Time) compilation**: C# compiles to platform-independent **CIL (Common Intermediate Language)**. At run time, the CLR's JIT compiler turns CIL into native machine code for the current machine.

### Visual Studio

Visual Studio is Microsoft's IDE. It includes the code editor with IntelliSense, an integrated debugger, a form designer, a class designer and a database schema designer. The Community Edition 2022 is free.

### First program

```csharp
using System;               // import a namespace

namespace HelloWorld        // declare our own namespace (a scope for classes)
{
    class Program
    {
        // Entry point. It is static so it can run without creating an object.
        static void Main(string[] args)   // args holds command-line arguments
        {
            Console.WriteLine("Hello World!");
        }
    }
}
```

- A **namespace** organises classes and controls their scope in large projects. The **`using` directive** imports namespaces so their types can be used without full qualification.
- **`Main`** is the first method invoked. It must be `static` because no object exists yet when the program starts.

---

## 3. C# language basics

*Source: Weeks 2-3*

C# is a modern, general-purpose, object-oriented, type-safe language in the curly-brace family (C, C++, Java). It adds its own mechanisms such as delegates, attributes and LINQ.

### Comments

```csharp
// Single-line comment

/* Multi-line comment
   Author: ...  Date: ... */
```

### Built-in data types

| Keyword | Meaning | Example values |
|---|---|---|
| `byte` | 8-bit unsigned integer | 0 to 255 |
| `int` | 32-bit signed integer | -12, 0, 3467 |
| `uint` | 32-bit unsigned integer | 0 to 4,294,967,295 |
| `long` | 64-bit signed integer | |
| `float` | single-precision floating point | `3.1234f` |
| `double` | double-precision floating point | `78.096` |
| `decimal` | high-precision decimal, best for money | `19.99m` |
| `bool` | Boolean, default `false` | `true`, `false` |
| `char` | 16-bit Unicode character, default `'\0'` | `'A'`, `'#'` |
| `string` | sequence of characters (a reference type) | `"Hello"` |

### Variables and constants

```csharp
int a;                 // declaration: <datatype> <identifier>;
int b, c, d;           // several at once
double rate = 2.25;    // declaration with initialisation

const double Pi = 3.14159;   // const: the value can never change
```

- Character literals use single quotes (`'a'`). String literals use double quotes (`"Hello"`).
- Common escape sequences: `\n` newline, `\t` tab, `\\` backslash, `\"` double quote, `\'` single quote, `\r` carriage return.

### Value types and reference types

- **Value types** hold their own copy of the data (copy semantics) and are usually stored on the **stack**. They include the simple types (`bool`, `byte`, `int`, `long`, `char`, `decimal`, `float`, `double`), `struct` and `enum`.
- **Reference types** hold a reference to data stored on the **heap** (reference semantics). They include `class`, `interface`, arrays, `delegate`, `string`, `object` and `dynamic`.
- **Boxing** converts a value type into a reference type (`object`). **Unboxing** converts it back.

```csharp
int number = 42;
object boxed = number;       // boxing
int unboxed = (int)boxed;    // unboxing (needs an explicit cast)
```

### Console input and output

- `Console.ReadLine()` reads a whole line and returns a `string`.
- `Console.Read()` reads the next character and returns its code as an `int`.
- `Console.ReadKey()` waits for the next key press.
- `Console.WriteLine()` prints followed by a newline. `Console.Write()` prints without one.
- All console input arrives as a string, so numbers must be converted with the `Convert` class (or `int.Parse` / `int.TryParse`).

```csharp
Console.Write("Enter an integer: ");
string input = Console.ReadLine();
int value = Convert.ToInt32(input);
double d = Convert.ToDouble(input);

Console.WriteLine("You entered {0}", value);   // {0} is a placeholder
Console.WriteLine($"You entered {value}");      // string interpolation, same result
```

### Operators

- **Arithmetic:** `+  -  *  /  %  ++  --`. Integer division truncates, so `1 / 2` is `0`. Cast first to get a fractional result: `(float)x / y`.
- **Relational:** `==  !=  >  >=  <  <=`
- **Logical:** `&&` (and), `||` (or), `!` (not)
- **Assignment:** `=  +=  -=  *=  /=  %=` (for example, `x += 10` means `x = x + 10`)
- **Other:** `.` member access, `[]` indexing, `()` cast, `?:` ternary, `sizeof(int)` (gives 4), `typeof(StreamReader)`

```csharp
int max = (5 > 6) ? 5 : 6;   // ternary: condition ? valueIfTrue : valueIfFalse
```

**Precedence** (highest first): parentheses, then `* / %`, then binary `+ -`, then relational operators, then logical operators. Operators of equal precedence evaluate left to right.

### Conditional statements

```csharp
if (score >= 85)
{
    grade = "HD";
}
else if (score >= 75)
{
    grade = "D";
}
else
{
    grade = "Other";
}

switch (day)
{
    case 1:
        Console.WriteLine("Monday");
        break;
    case 2:
        Console.WriteLine("Tuesday");
        break;
    default:
        Console.WriteLine("Other day");
        break;
}
```

`if` statements can also be nested inside one another.

### Loops

- **Entry-controlled** loops (`for`, `while`) check the condition first, so they may run zero times.
- **Exit-controlled** loops (`do ... while`) check at the end, so they always run at least once.
- Loops can be nested inside one another.

```csharp
for (int i = 0; i < 5; i++) { Console.WriteLine(i); }

int n = 0;
while (n < 5) { n++; }

do { n--; } while (n > 0);

foreach (string name in names) { Console.WriteLine(name); }   // see Section 8
```

**Loop control:** `break` exits the loop immediately. `continue` skips the rest of the current iteration and moves to the next one.

### Strings

- `string` is a reference type that behaves somewhat like a value type, because strings are immutable.
- Useful members:
  - `Length` property
  - `String.Compare(s1, s2)`
  - `String.Concat(s1, s2)`
  - `s1.Contains(s2)`
  - `ToUpper()` and `ToLower()`

```csharp
string first = "Hello";
string last = "World";
int len = first.Length;                         // 5
bool has = "Application Dev".Contains("Application");
string upper = first.ToUpper();                 // "HELLO"
string full = String.Concat(first, " ", last);  // "Hello World"
```

### Enums

An **enum** is a value type made up of named integer constants.

```csharp
enum Days { Sun, Mon, Tue, Wed, Thu, Fri, Sat }   // Sun = 0, Mon = 1, ...

int weekStart = (int)Days.Mon;   // 1
Days today = Days.Fri;
```

### Arrays

- An array is a **fixed-size** collection of elements of the same type, stored contiguously and accessed by a zero-based index.
- Arrays are reference types.

```csharp
double[] marks = new double[1000];                    // allocate
marks[0] = 98.0;                                      // assign by index

double[] m1 = { 98.0, 79.0, 65.0 };                   // equivalent initialisers
double[] m2 = new double[] { 98.0, 79.0, 65.0 };
double[] m3 = new double[3] { 98.0, 79.0, 65.0 };

int size = m1.Length;        // 3
double third = m1[2];        // 65.0

// Two-dimensional (rectangular) array: rows x columns
int[,] studentInfo = new int[4, 2]
{
    { 1001, 99 },
    { 1002, 88 },
    { 1003, 76 },
    { 1004, 69 },
};
int mark = studentInfo[1, 1];   // 88
```

### Methods

A method is a named group of statements that performs a task. Every program has at least one class and one method (`Main`).

```text
<access_specifier> <return_type> <MethodName>(<parameter list>)
{
    // method body
}
```

Use `void` when a method returns nothing. Method names are case-sensitive.

**Three ways to pass parameters:**

```csharp
// 1. By value (the default): the method gets a copy, so the caller's variable is unchanged.
static bool IsEven(int numberToCheck) => numberToCheck % 2 == 0;

// 2. By reference (ref): the method works on the caller's variable itself.
static void Swap(ref int x, ref int y)
{
    int temp = x;
    x = y;
    y = temp;
}

// 3. Output (out): passes data OUT of the method. The caller doesn't need to
//    initialise it, but the method must assign it before returning.
static void GetMinMax(int[] values, out int min, out int max)
{
    min = values.Min();
    max = values.Max();
}

int a = 1, b = 2;
Swap(ref a, ref b);                                  // a = 2, b = 1
GetMinMax(new[] { 3, 9, 1 }, out int lo, out int hi); // lo = 1, hi = 9
```

---

## 4. Object-oriented programming

*Source: Weeks 3-5*

### Structured programming compared with OOP

- **Structured (procedural) programming** is process-centric. Programs are split into functions, data is secondary, and there is no data hiding. Reuse is limited and functions depend heavily on one another.
- **Object-oriented programming** is data-centric. Programs are split into **objects** that bundle **state** (attributes/properties) with **behaviour** (methods). It supports data hiding, reuse and complex problems.
- For example, a `Car` has properties (colour, weight, speed, seat capacity) and methods (`GoForward`, `TurnLeft`, `ApplyBrakes`).
- **Advantages:**
  - **Modularity:** each object can be written, maintained and reused independently.
  - **Information hiding:** callers ignore implementation details, and each object controls its own internal state.

### The pillars of OOP

- **Class and object:** a class is a type, or template. An object is an instance of a class holding its own values for the class's attributes.
- **Abstraction:** expose the essential features (*what* an object does) and hide *how* it does it. This solves the problem at the design level.
- **Encapsulation:** bundle related data and behaviour, and expose only what is needed. This is the basis of class design and solves the problem at the implementation level.
- **Inheritance:** a class acquires the members of another class. It models an **IS-A** relationship.
- **Polymorphism:** one interface with many forms.
  - **Static (compile time):** method overloading and operator overloading.
  - **Dynamic (run time):** virtual methods, overriding and abstract classes.

### Classes and access modifiers

```text
<access_modifier> class <ClassName>
{
    <access_modifier> <type> field;                                // data members
    <access_modifier> <return_type> Method(<params>) { ... }       // methods
}
```

- `public`: no restrictions.
- `private`: accessible only inside the containing class. This is the default for class members.
- `protected`: accessible inside the containing class and derived classes.
- `internal`: accessible within the same assembly (project).
- `static`: belongs to the type, not an instance, so you access it through the type name.
- `abstract`: the implementation is incomplete or missing.
- `sealed`: the class cannot be inherited.

Objects are created with **`new`**, which allocates memory and calls a constructor. Even `int n = new int();` is valid, and gives `0`.

### Properties (`get`, `set`, `value`)

Properties are special methods called **accessors** that read, write or compute the value of a private field.

```csharp
public class Student
{
    private string name;                 // private backing field

    public string Name                   // full property with validation
    {
        get { return name; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))   // value = the value being assigned
                throw new ArgumentException("Name is required.");
            name = value;
        }
    }

    public int RollNo { get; set; }      // auto-implemented property
    public double Gpa { get; private set; }   // read-only from outside the class
}
```

### Constructors and `this`

- A **constructor** has the same name as the class and no return type. It runs whenever an object is created.
- If you write no constructor, C# provides a parameterless **default constructor** that sets fields to their default values.
- A class can have several constructors. This is called **constructor overloading**.
- **`this`** refers to the current instance. Use it to tell fields apart from parameters, or to chain constructors.

```csharp
public class Box
{
    private double length, width, height;

    public Box() : this(1, 1, 1) { }                 // chains to the constructor below

    public Box(double length, double width, double height)
    {
        this.length = length;                        // this.field = parameter
        this.width = width;
        this.height = height;
    }

    // Copy constructor. C# has no built-in one, so we write it ourselves.
    public Box(Box other) : this(other.length, other.width, other.height) { }

    public double Volume() => length * width * height;
}
```

### Static polymorphism: overloading

**Method overloading** means several methods share a name but differ in the **number or types of parameters**. You cannot overload by return type alone.

```csharp
public class StudentDatabase
{
    public bool Search(int studentId) { /* ... */ return true; }
    public bool Search(string studentName) { /* ... */ return true; }
    public bool Search(string studentName, int studentId) { /* ... */ return true; }
}

var db = new StudentDatabase();
db.Search(5);
db.Search("George");
db.Search("George", 5);
```

**Operator overloading** redefines a built-in operator for a user-defined type. It is declared as a `public static` method named `operator` followed by the symbol.

```csharp
public class Money
{
    public decimal Amount { get; }
    public Money(decimal amount) => Amount = amount;

    public static Money operator +(Money a, Money b) => new Money(a.Amount + b.Amount);
}
```

Which operators can be overloaded:

- **Can be overloaded:**
  - unary `+ - ! ~ ++ --` (one operand)
  - binary `+ - * / %` (two operands)
  - comparison `== != < > <= >=`, which must be overloaded in pairs
- **Cannot be overloaded directly:**
  - `&&` and `||`
  - compound assignment such as `+=`, which is derived automatically from `+`
- **Cannot be overloaded at all:** `= . ?: -> new is sizeof typeof`

### Inheritance

- Inheritance reuses and extends an existing class.
- C# allows **single inheritance only**: one base class per class. Every class ultimately derives from `System.Object`.
- Inheritance is transitive. If C derives from B and B derives from A, then C gets A's members too.
- **Not inherited:** static constructors, instance constructors (each class defines its own) and finalizers. Everything else is inherited, subject to its access modifier.

```csharp
public class Person
{
    public string Name { get; set; }
    public Person(string name) => Name = name;
}

public class Customer : Person                  // Customer IS-A Person
{
    public int LoyaltyPoints { get; set; }
    public Customer(string name) : base(name) { }   // call the base constructor
}
```

### Dynamic polymorphism: overriding and hiding

- **Overriding:** mark the base method `virtual` and the derived method `override`. Methods are non-virtual by default, so they cannot be overridden unless marked. The **run-time type of the object** decides which version runs.
- **Hiding:** declare a method with the same signature using `new`. The **declared type of the variable** decides which version runs, so there is no polymorphism.

```csharp
public class Shape
{
    public virtual double Area() => 0;
    public void Describe() => Console.WriteLine("A shape");
}

public class Circle : Shape
{
    public double Radius { get; set; }
    public override double Area() => Math.PI * Radius * Radius;     // overriding
    public new void Describe() => Console.WriteLine("A circle");    // hiding
}

Shape s = new Circle { Radius = 2 };
s.Area();       // calls Circle.Area: the object's real type wins
s.Describe();   // prints "A shape": the variable's type wins
```

### Abstract classes

- An abstract class **cannot be instantiated**.
- It may contain **abstract methods**, which have no body and are implicitly virtual. Abstract methods are only allowed inside abstract classes.
- A non-abstract derived class **must override every inherited abstract member**.

```csharp
public abstract class Person
{
    public string Name { get; set; }
    public abstract string TransformName(string name);   // no implementation
}

public class Customer : Person
{
    public override string TransformName(string name) => name.ToUpper();
}
```

### Sealed classes and methods

- `sealed` stops a class from being inherited. It is mostly used in libraries or commercial code.
- `sealed override` stops further overriding of a single method.

```csharp
public sealed class Configuration { /* ... */ }   // no class can derive from this

public class Employee
{
    public virtual decimal CalculatePay() => 3000m;
}

public class Manager : Employee
{
    public sealed override decimal CalculatePay() => 5000m;
}
```

### Interfaces

- An interface declares **signatures only**: methods, properties, events and indexers, with no implementation.
- Members are implicitly `public`, and you don't write an access modifier on them.
- A class that implements an interface must implement every member.
- A class can implement **many interfaces** but inherit only one class.

```csharp
public interface IPayable
{
    decimal AmountDue { get; }
    void Pay(decimal amount);
}

public interface IExportable
{
    string ToCsv();
}

public class Bill : IPayable, IExportable   // one class, two interfaces
{
    public decimal AmountDue { get; private set; } = 100m;
    public void Pay(decimal amount) => AmountDue -= amount;
    public string ToCsv() => $"Bill,{AmountDue}";
}
```

### Structs

- A struct is a **value type** suited to small, lightweight objects such as `Point`, `Rectangle` or `Colour`.
- Structs do not support inheritance, although they can implement interfaces.
- Structs cannot declare a finalizer.
- You can create one without `new`, but then you must assign every field before using it.
- The lecture notes say a struct cannot have an explicit parameterless constructor or field initialisers. That was true before C# 10. Modern C# allows both, but the simple, safe pattern is still a parameterised constructor.

```csharp
public struct Point
{
    public int X;
    public int Y;
    public Point(int x, int y) { X = x; Y = y; }
}

Point p1 = new Point(3, 4);
Point p2 = p1;     // a full copy: changing p2 does not affect p1
```

---

## 5. Memory and resources

*Source: Weeks 3-4*

### Garbage collection (GC)

The CLR's garbage collector is an automatic memory manager. It:

- frees you from releasing memory manually
- allocates objects efficiently on the **managed heap**
- reclaims objects that are no longer used, so the memory can be reused
- gives new objects clean (zeroed) memory
- provides memory safety, because one object cannot use another object's memory

A collection is triggered when:

- the system is low on physical memory
- memory used on the managed heap passes a threshold that adjusts as the program runs
- `GC.Collect()` is called. You should rarely do this; it is meant for testing and unusual cases.

A collection has three phases:

1. **Marking:** find all live objects, starting from the roots (stack and global/static references).
2. **Relocating:** update references to objects that are about to move.
3. **Compacting:** reclaim dead objects' space and move survivors together.

The GC uses **generations**, which allow cheap **partial collections** of young objects instead of an expensive **full collection** that pauses the program and scans everything.

### Cleaning up unmanaged resources

The GC handles memory but not **unmanaged resources** such as database connections, open files and network sockets. To release those:

1. Implement **`IDisposable`** and its `Dispose()` method.
2. Wrap usage in a **`using`** block or declaration, which calls `Dispose()` automatically, even if an exception is thrown.
3. Optionally add a **finalizer (destructor)** as a backup in case `Dispose()` is never called.

```csharp
using (var writer = new StreamWriter("log.txt"))   // Dispose() runs at the closing brace
{
    writer.WriteLine("Saved");
}

using var reader = new StreamReader("log.txt");     // C# 8+: disposed at the end of the scope
```

### Finalizers (destructors)

- A finalizer runs when the GC removes the object from memory. It **cannot be called explicitly**.
- Each class can have only one finalizer. It cannot be inherited or overloaded, and it is not available on structs.
- Its syntax is `~ClassName() { ... }`.

```csharp
public class Logger
{
    ~Logger()
    {
        // last-chance cleanup; prefer IDisposable
    }
}
```

---

## 6. Error handling

*Source: Week 4*

- An **exception** is a run-time error or unexpected condition. All exceptions derive from `System.Exception`.
- `try` holds code that might fail.
- `catch` handles a failure where it is sensible to do so. You can have several `catch` blocks; put the **most specific first**.
- `finally` is optional. It **always runs** and is used for cleanup such as closing files, streams or database connections.
- `throw` raises an exception. Exceptions can come from the CLR, the .NET libraries, third-party libraries or your own code.

```csharp
try
{
    int value = Convert.ToInt32(textBoxAmount.Text);
    int result = 100 / value;
}
catch (FormatException)
{
    MessageBox.Show("Please enter a whole number.");
}
catch (DivideByZeroException ex)
{
    MessageBox.Show($"Cannot divide by zero: {ex.Message}");
}
catch (Exception ex)                       // general catch-all goes last
{
    MessageBox.Show($"Unexpected error: {ex.Message}");
}
finally
{
    // cleanup that must always happen
}
```

### Throwing exceptions

```csharp
public void Withdraw(decimal amount)
{
    if (amount <= 0)
        throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
    if (amount > Balance)
        throw new InvalidOperationException("Insufficient funds.");
    Balance -= amount;
}
```

You can also write a custom exception class by deriving from `Exception`, for example `class ValidationException : Exception`.

### Common exception types

| Exception | Typical cause |
|---|---|
| `System.IO.IOException` | General input/output failure |
| `FileNotFoundException` | The file does not exist |
| `DivideByZeroException` | Integer division by zero |
| `IndexOutOfRangeException` | Bad array index, e.g. `arr[arr.Length]` |
| `NullReferenceException` | Using a member of a `null` reference |
| `ArgumentNullException` | A `null` argument was not allowed, e.g. `"text".IndexOf(null)` |
| `ArgumentOutOfRangeException` | An argument was outside the allowed range, e.g. `s.Substring(s.Length + 1)` |
| `FormatException` | Parsing failed, e.g. `Convert.ToInt32("abc")` |

---

## 7. File I/O

*Source: Weeks 3-4*

- A **file** is named data stored on disk at a directory path. An open file is a **stream**: a sequence of bytes flowing between the program and the device.
- An **input stream** reads from a file. An **output stream** writes to one.
- The file classes live in the **`System.IO`** namespace.

### `FileStream`

Use `FileStream` for low-level reading and writing:

```csharp
using System.IO;

using var fs = new FileStream("sample.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
```

- **`FileMode`:**
  - `Append`: open and move to the end, creating the file if missing.
  - `Create`: create a new file, overwriting any existing one.
  - `Open`: open an existing file.
  - `OpenOrCreate`: open the file, or create it if missing.
  - `Truncate`: open and empty the file.
- **`FileAccess`:** `Read`, `Write`, `ReadWrite`.
- **`FileShare`:** `None`, `Read`, `Write`, `ReadWrite`. This controls what other processes may do with the file while it is open.

### `File` static helpers

These are the simplest way to work with a whole file:

```csharp
string path = "boxes.txt";

File.WriteAllText(path, "Box 1\n");          // create or overwrite, then close
File.AppendAllText(path, "Box 2\n");         // append, creating the file if missing
string all = File.ReadAllText(path);         // whole file as one string
string[] lines = File.ReadAllLines(path);    // one string per line
bool exists = File.Exists(path);
```

### `StreamReader` and `StreamWriter`

Use these to read or write line by line:

```csharp
using (var writer = new StreamWriter("data.csv", append: true))
{
    writer.WriteLine("1,Alice,25.50");
}

using (var reader = new StreamReader("data.csv"))
{
    string? line;
    while ((line = reader.ReadLine()) != null)
    {
        string[] parts = line.Split(',');
    }
}
```

Relative paths resolve against the program's output folder (`bin/Debug/net9.0-windows/...`).

---

## 8. Generics and collections

*Source: Weeks 4 and 6*

### Generics

Generics let you write classes and methods with a **type parameter** (`<T>`), so one piece of code works for any type while staying **strongly typed**.

There are three ways to write an "are these two values equal?" check, and generics are the best:

1. **One method per type** (for example `AreEqual(int, int)`). This works for only one type.
2. **Use `object` parameters.** This works for any type, but it boxes and unboxes value types (slow) and loses type safety.
3. **Use a generic method.** This works for any type, needs no boxing and stays type-safe.

```csharp
public static class Calculator
{
    public static bool AreEqual<T>(T value1, T value2) => Equals(value1, value2);
}

bool b1 = Calculator.AreEqual<int>(10, 10);
bool b2 = Calculator.AreEqual("A", "B");   // T is inferred as string

// A generic class with a constraint
public class Repository<T> where T : class
{
    private readonly List<T> items = new List<T>();
    public void Add(T item) => items.Add(item);
    public IEnumerable<T> GetAll() => items;
}
```

### Collections overview

- Arrays have a fixed size. **Collections** grow and shrink dynamically, and they provide lists, stacks, queues and hash tables.
- **Non-generic collections** (`System.Collections`) store `object`. They are **not type-safe** and they box value types. Examples: `ArrayList`, `Hashtable`, `Queue`, `Stack`, `SortedList`.
- **Generic collections** (`System.Collections.Generic`) are **preferred**. They are type-safe and faster. Examples: `List<T>`, `Dictionary<TKey,TValue>`, `Queue<T>`, `Stack<T>`, `SortedList<TKey,TValue>`, `LinkedList<T>`.
- Every collection lets you add, remove and find items. Collections that implement `ICollection` or `ICollection<T>` can also be enumerated, copied to an array with `CopyTo`, report a `Count`, and have a consistent lower bound (their first index).

### Collection interfaces

- `IEnumerable` / `IEnumerable<T>`: has `GetEnumerator()`, which is what makes `foreach` work.
- `ICollection` / `ICollection<T>`: adds `Count` and `CopyTo`. It extends `IEnumerable`.
- `IList` / `IList<T>`: adds an indexer plus `Add`, `Remove`, `Contains` and `IndexOf`. It extends `ICollection`.
- `IDictionary` / `IDictionary<TKey,TValue>`: key/value pairs, with an indexer, `Add`, `Remove`, `Keys` and `Values`.
- `IComparer<T>` and `IEqualityComparer<T>`: custom ordering and custom equality.

### Which collection to use

- **Look up by key:** use `Dictionary<TKey,TValue>` (non-generic: `Hashtable`).
- **Access by index:** use `List<T>` (non-generic: array or `ArrayList`).
- **Last-in, first-out (LIFO):** use `Stack<T>` (non-generic: `Stack`).
- **First-in, first-out (FIFO):** use `Queue<T>` (non-generic: `Queue`).
- **Sequential access with cheap inserts and removals:** use `LinkedList<T>`.
- **Sorted key/value pairs:** use `SortedList<TKey,TValue>` (non-generic: `SortedList`).

### `ArrayList` (non-generic)

- `ArrayList` grows automatically, needs no size, supports inserting and removing at a position, and can hold **mixed types**.
- **Properties:** `Capacity`, `Count`, `IsFixedSize`, `IsReadOnly`, and the indexer `[i]`.
- **Methods:** `Add`, `Insert`, `Remove`, `RemoveAt`, `Sort`, `Contains`, `Clear`, `Clone` (a shallow copy), `BinarySearch` (sort first), `Reverse`, `IndexOf`.

```csharp
using System.Collections;

ArrayList students = new ArrayList();
students.Add("Alice");
students.Add(42);              // mixed types are allowed, but this is not type-safe
students.Insert(0, "Bob");
students.Remove("Alice");
int count = students.Count;
```

### `List<T>` (generic)

`List<T>` is the generic, type-safe replacement for `ArrayList`:

```csharp
List<string> names = new List<string> { "Alice", "Bob" };
names.Add("Cara");
names.Remove("Bob");
names.Sort();
bool hasAlice = names.Contains("Alice");
string first = names[0];
```

### `Dictionary<TKey, TValue>`

- **Properties:** `Count`, the indexer `[key]`, `Keys`, `Values`.
- **Methods:** `Add(key, value)`, `Remove(key)`, `Clear()`, `ContainsKey(key)`, `ContainsValue(value)`, `TryGetValue(key, out value)`.

```csharp
Dictionary<int, string> weekdays = new Dictionary<int, string>();
// Or through the interface: IDictionary<int, string> weekdays = new Dictionary<int, string>();

weekdays.Add(1, "Monday");
weekdays[2] = "Tuesday";                    // add or replace

if (weekdays.ContainsKey(1))
    Console.WriteLine(weekdays[1]);

if (weekdays.TryGetValue(3, out string? day))   // safe lookup, no exception if missing
    Console.WriteLine(day);

foreach (KeyValuePair<int, string> pair in weekdays)
    Console.WriteLine($"{pair.Key}: {pair.Value}");
```

### `foreach`

- `foreach` iterates any `IEnumerable` or `IEnumerable<T>` (arrays, lists, dictionaries) without using indexes.
- For a 1-D array it visits index 0 through `Length - 1`.
- It works on multi-dimensional arrays, but nested `for` loops give more control there.
- `break` and `continue` work as in other loops.
- You cannot add or remove items from a collection while you are iterating it with `foreach`.

### Extension methods

Extension methods add methods to an **existing type** without deriving from it, recompiling it or modifying it. They are useful for `sealed` classes and built-in types.

- They must be declared in a **static, non-nested class** and must themselves be **static**.
- The first parameter is marked with **`this`**, which binds the method to the type it extends.
- They cannot override existing methods, and each method binds to only one type.
- Don't overuse them.

```csharp
public static class StringExtensions
{
    public static int WordCount(this string text) =>
        text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
}

int words = "Hello big world".WordCount();   // 3, called as if it were a string method
```

LINQ's `Where`, `Select` and `OrderBy` are themselves extension methods on `IEnumerable<T>` (see Section 9).

---

## 9. Delegates, lambdas and LINQ

*Source: Week 7*

### Delegates

- A **delegate** is a **type-safe pointer to a method**. It lets you treat methods as data: store them, pass them to other methods, and call them later.
- Delegates are the basis of **events and callbacks**, such as button click handlers.
- They are reference types and derive from `System.Delegate`.
- The delegate's signature must match the target method's signature, otherwise you get a compiler error. That is what makes delegates type-safe.

```csharp
public delegate void OperationDelegate(int num1, int num2);   // declare the delegate type

public static void Add(int a, int b) => Console.WriteLine(a + b);
public static void Multiply(int a, int b) => Console.WriteLine(a * b);

OperationDelegate op = new OperationDelegate(Add);   // or simply: OperationDelegate op = Add;
op(10, 20);                                          // invokes Add, printing 30
```

### Multicast delegates

- A multicast delegate holds an **invocation list** of several methods and calls them **in order**.
- Register methods with `+` or `+=`, and unregister them with `-` or `-=`.
- If the delegate returns a value, the caller gets the value from the **last** method in the list.

```csharp
OperationDelegate ops = Add;
ops += Multiply;
ops(3, 4);          // prints 7, then 12
ops -= Add;
```

### Anonymous methods

An anonymous method is a method with no name, defined inline with the `delegate` keyword and assigned to a delegate variable.

- It can use variables from the surrounding scope.
- It can be passed as a parameter or used as an event handler.

```csharp
int bonus = 5;
OperationDelegate addWithBonus = delegate (int a, int b)
{
    Console.WriteLine(a + b + bonus);    // captures the outer variable
};
addWithBonus(1, 2);                      // prints 8

button1.Click += delegate (object? sender, EventArgs e) { MessageBox.Show("Clicked"); };
```

### Generic delegates

Generic delegates save you from declaring a separate delegate type for every data type:

```csharp
public delegate void MyDelegate<T>(T value);

MyDelegate<string> print = s => Console.WriteLine(s.ToLower());
print += s => Console.WriteLine(s.ToUpper());
print("Hello");     // prints "hello", then "HELLO"
```

### Built-in delegates: `Action`, `Predicate`, `Func`

- **`Action<T>`** takes an argument and returns `void`. Use it to *do* something with a value.
- **`Predicate<T>`** takes an argument and returns `bool`. Use it to *test* a value.
- **`Func<..., TResult>`** takes up to 16 arguments and returns a `TResult`. The **last** type parameter is always the return type.

```csharp
Action<string> greet = name => Console.WriteLine($"Hi {name}");
Predicate<int> isEven = n => n % 2 == 0;
Func<int, int, int> add = (a, b) => a + b;
Func<double> getPi = () => Math.PI;

greet("Alice");
bool even = isEven(4);   // true
int sum = add(2, 3);     // 5
```

### Lambda expressions

- A lambda is a shorter way to write an anonymous method. It was introduced with LINQ in C# 3.0 (.NET 3.5).
- It uses the `=>` operator: `(parameters) => expression`, or `(parameters) => { statements; }`.
- Every lambda converts to a delegate type, such as your own delegate, `Func`, `Action` or `Predicate`.

```csharp
// The same filter written as an anonymous method and as a lambda:
Func<Student, bool> isTeen1 = delegate (Student s) { return s.Age > 12 && s.Age < 20; };
Func<Student, bool> isTeen2 = s => s.Age > 12 && s.Age < 20;
```

### LINQ (Language-Integrated Query)

- LINQ (introduced in .NET 3.5) is a uniform query syntax **built into C#**. It works against object collections, SQL databases, XML, `DataSet`s and Entity Framework.
- Without LINQ, each data source needs its own query language, or a hand-written `foreach` loop to find items.
- **Advantages:** a familiar language, less code, readable code, one standard way to query many data sources, data shaping, and fewer errors.
- **Flavours:** LINQ to Objects, LINQ to XML, LINQ to DataSet, LINQ to SQL, and LINQ to Entities (EF).
- **API:** the `System.Linq` namespace, whose main static classes `Enumerable` and `Queryable` hold the extension methods. New projects import it automatically.
- **A query has three parts:** (1) get the data source, (2) create the query, (3) execute it.
- **Deferred execution:** defining a query doesn't run it. It runs when you iterate it, or when you call `ToList()`, `Count()` and similar methods.
- A query returns an `IEnumerable<T>` (or `IQueryable<T>` for databases).

### Query syntax

- A query starts with **`from range in source`**, where the range variable receives each element.
- It continues with zero or more **`where`** filters. Each one must evaluate to a `bool`, and you can have several.
- It can sort with **`orderby x ascending|descending`**. You can sort on several keys, and the keys must be comparable.
- It ends with **`select`** (choose or shape the result) or **`group ... by`**.
- The full keyword list: `from in where select group by into orderby ascending descending join on equals let`.

```csharp
int[] numbers = { 5, 10, 8, 3, 6, 12 };

IEnumerable<int> query =
    from n in numbers
    where n > 0
    where n < 10
    orderby n descending
    select n;

foreach (int n in query) Console.Write(n + " ");   // executes here: 8 6 5 3
```

### LINQ with custom objects: `group` and `join`

```csharp
var students = new List<Student>
{
    new Student { Id = 1, Name = "Alice", Age = 19, CourseId = 10 },
    new Student { Id = 2, Name = "Bob",   Age = 22, CourseId = 20 },
    new Student { Id = 3, Name = "Cara",  Age = 17, CourseId = 10 },
};
var courses = new List<Course>
{
    new Course { Id = 10, Title = ".NET" },
    new Course { Id = 20, Title = "Java" },
};

// group: returns IEnumerable<IGrouping<TKey, TElement>>
var byCourse = from s in students
               group s by s.CourseId into g
               select new { CourseId = g.Key, Count = g.Count() };

// join: combine two sources on matching keys
var enrolments = from s in students
                 join c in courses on s.CourseId equals c.Id
                 select new { s.Name, c.Title };
```

### Method syntax (lambdas with extension methods)

The same queries can be written by chaining extension methods with lambdas. This style is the most common in practice, and it is the one the assignment's "anonymous method with LINQ using a lambda expression" requirement asks for.

```csharp
IList<string> stringList = new List<string>
{
    "C# Tutorials", "VB.NET Tutorials", "Learn C++", "MVC Tutorials", "Java"
};

var tutorials = stringList.Where(s => s.Contains("Tutorials"));

var teenNames = students
    .Where(s => s.Age > 12 && s.Age < 20)
    .OrderBy(s => s.Name)
    .Select(s => s.Name)
    .ToList();
```

### Standard query operators

- **Categories:** filtering, projection, sorting, grouping, join, set, partition, aggregation, quantifier, element, conversion, concatenation, generation and equality.
- **Filtering and shaping:** `Where`, `Select`, `OrderBy` / `OrderByDescending` / `ThenBy`, `GroupBy`, `Join`, `Distinct`, `Skip`, `Take`, `ToList`, `ToDictionary`.
- **Quantifiers:**
  - `All(condition)`: true if every element matches.
  - `Any(condition)`: true if at least one element matches.
  - `Contains(obj)`: true if the sequence contains the object.
- **Aggregates:** `Count()`, `Sum()`, `Average()`, `Min()`, `Max()`.
- **Elements:** `First()`, `Last()`, and `FirstOrDefault()`, which returns `null` or the default value instead of throwing when nothing matches.

```csharp
decimal total = expenses.Sum(e => e.Amount);
double avgAge = students.Average(s => s.Age);
bool anyMinor = students.Any(s => s.Age < 18);
Student? alice = students.FirstOrDefault(s => s.Name == "Alice");
```

---

## 10. GUI with Windows Forms

*Source: Week 5*

### Creating a Windows Forms app

- In Visual Studio, choose **Windows Forms App** (.NET). The project starts with a blank `Form1`, which works like a canvas.
- **Designer view:** drag controls from the **Toolbox** onto the form, then move and resize them. Use the **Properties** window (F4) to set `Name`, `Text`, `Anchor`, `Dock` and other properties, and use its lightning-bolt tab to wire up events.
- **Code view:** double-click the form or a control, or right-click and choose **View Code**. Double-clicking a control creates its default event handler, such as `button1_Click`.
- Each form is a `partial class` split across two files:
  - `Form1.cs` holds your code.
  - `Form1.Designer.cs` is generated by the designer. Don't hand-edit it.
- Every event handler has the signature `(object sender, EventArgs e)`. Event handlers are delegates (see Section 9).

### Common controls

- **`Label`:** read-only text. You can change `Text` in code.
- **`Button`:** the user clicks it to trigger an action through its `Click` event.
- **`TextBox`:** text the user can type. For a password box, set `UseSystemPasswordChar = true` or `PasswordChar = '*'`.
- **`CheckBox`:** an on/off option. Several can be checked at once. Read it with `checkBox1.Checked`.
- **`RadioButton`:** only one radio button per container can be checked. Read it with `radioButton1.Checked`.
- **`ListBox`:** a list of items the user can select. Use `Items.Add(...)`, `SelectedItem`, and `SelectedItems` for multiple selection.
- **`ComboBox`:** a drop-down list, optionally editable. Use `Items.Add(...)`, `Text`, `SelectedItem` and `SelectedIndex`.
- **`GroupBox`:** a container with a caption that groups controls so they can be moved or hidden together. It also separates groups of radio buttons. It has no scroll bars.
- **`MenuStrip` / `ToolStripMenuItem`:** a menu bar with groups of related commands. Each item has a `Click` handler.
- **Other useful controls:**
  - `DataGridView` for tables
  - `PictureBox` for images
  - `DateTimePicker` for dates
  - `NumericUpDown` for numbers
  - `TabControl` for tabs
  - `ContextMenuStrip` for right-click menus
  - `ProgressBar`
  - `Chart` for graphs

```csharp
private void buttonAdd_Click(object sender, EventArgs e)
{
    labelStatus.Text = $"Hello {textBoxName.Text}";

    if (checkBoxSubscribe.Checked) { /* ... */ }
    if (radioButtonMonthly.Checked) { /* ... */ }

    listBoxItems.Items.Add(textBoxName.Text);
    comboBoxCategory.Items.Add("Groceries");
}

private void buttonShowSelection_Click(object sender, EventArgs e)
{
    MessageBox.Show(listBoxItems.SelectedItem?.ToString() ?? "Nothing selected");

    foreach (object item in listBoxItems.SelectedItems)   // multiple selection
        MessageBox.Show(item.ToString());

    MessageBox.Show(comboBoxCategory.Text);
}

private void newToolStripMenuItem_Click(object sender, EventArgs e)
{
    MessageBox.Show("You selected menu item New");
}
```

### `MessageBox`

`MessageBox.Show` has over 20 overloads. You can specify the buttons and icon, and read the user's choice from the returned **`DialogResult`** enum.

- **`MessageBoxButtons`:** `OK`, `OKCancel`, `YesNo`, `YesNoCancel`, `RetryCancel`, `AbortRetryIgnore`.
- **`MessageBoxIcon`:** `Information`, `Warning`, `Error`, `Question`, and others.

```csharp
MessageBox.Show("Saved successfully.", "My App");   // (message, title)

DialogResult result = MessageBox.Show(
    "Delete this record?", "Confirm delete",
    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

if (result == DialogResult.Yes)
{
    // delete
}
```

### Dialog boxes

- **Modal** dialogs block the rest of the application until they are closed. Open one with `ShowDialog()`.
- **Modeless** dialogs let the user keep working while they stay open. Open one with `Show()`.
- **Common dialogs** are standard Windows dialogs:
  - `OpenFileDialog`: choose a file to open.
  - `SaveFileDialog`: choose where to save.
  - `FontDialog`: pick a font.
  - `ColorDialog`: pick a colour.
- To use a common dialog: (1) create an instance, (2) set its properties, (3) call `ShowDialog()` and check the `DialogResult`.

```csharp
using var openDialog = new OpenFileDialog
{
    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
    Title = "Import data"
};

if (openDialog.ShowDialog() == DialogResult.OK)
{
    string[] lines = File.ReadAllLines(openDialog.FileName);
}

using var colorDialog = new ColorDialog();
if (colorDialog.ShowDialog() == DialogResult.OK)
    panelPreview.BackColor = colorDialog.Color;
```

### Multiple forms and communication between them

- Most applications have a **main form** that loads at start-up, such as a login form, and open other forms from it, such as profile or data-entry forms.
- To pass data **into** a form, use its constructor or public properties.
- To get data **back**, read public properties after `ShowDialog()` returns, or raise an event or call a delegate (callback) from the child form.

```csharp
// Program.cs: the start-up form
Application.Run(new LoginForm());

// LoginForm: open the main form after a successful login
private void buttonLogin_Click(object sender, EventArgs e)
{
    var main = new MainForm(currentUser);        // pass data in through the constructor
    main.FormClosed += (s, args) => this.Close(); // close the app when the main form closes
    main.Show();                                 // modeless
    this.Hide();
}

// MainForm: open a child form modally and read its result back
private void buttonAddExpense_Click(object sender, EventArgs e)
{
    using var dialog = new ExpenseForm();
    if (dialog.ShowDialog(this) == DialogResult.OK)
    {
        expenses.Add(dialog.CreatedExpense);     // public property on ExpenseForm
        RefreshGrid();
    }
}

// ExpenseForm: return a result to the caller
public Expense? CreatedExpense { get; private set; }

private void buttonSave_Click(object sender, EventArgs e)
{
    CreatedExpense = new Expense(textBoxDescription.Text, numericAmount.Value);
    DialogResult = DialogResult.OK;              // closes the modal form
}
```

### Resizable and responsive forms

The assignment requires resizable and responsive screens. In Windows Forms:

- Set **`Anchor`** (for example Top, Left, Right) so a control stretches or stays pinned when the form resizes.
- Set **`Dock`** (`Fill`, `Top`, `Bottom`, `Left`, `Right`) so a control fills an edge or the whole container.
- Use **`TableLayoutPanel`** and **`FlowLayoutPanel`** for grid and flow layouts that reflow automatically.
- Use `SplitContainer` for user-resizable panes, and set `MinimumSize` on the form.

### WPF and MAUI (bonus marks)

The lectures name **WPF** (Windows Presentation Foundation) and **.NET MAUI** as alternatives to Windows Forms. Using Blazor, ASP.NET, WPF or another UI library instead of Windows Forms earns bonus marks in Assignment 2.

- Both define layouts in **XAML** markup with code-behind in C#.
- Both use layout containers such as `Grid` and `StackPanel`, which are naturally responsive.
- Both support data binding.
- MAUI is cross-platform (Windows, Android, iOS, macOS). WPF is Windows-only.

---

## 11. Entity Framework Core

*Source: Week 8*

### What it is

- **Entity Framework (EF)** is an open-source **ORM (Object-Relational Mapper)** for .NET. It lets you work with database rows as ordinary .NET objects.
- It sits between the business layer and the database: **user interface, then business layer, then Entity Framework, then database**.
- **Advantages:**
  - simpler database access with little or no hand-written SQL
  - higher productivity
  - many database providers
  - **strongly typed queries** through LINQ to Entities
  - **automatic change tracking**
- **EF Core** is the lightweight, cross-platform rewrite for .NET Core and .NET 5+. It is the recommended choice for new projects, so use it with .NET 9.

### Three approaches

- **Database First:** reverse-engineer an existing database. EF generates the domain classes.
- **Code First:** write the C# domain classes. EF generates and updates the database tables through **migrations**. **This is the approach taught in the lecture.**
- **Model First:** draw a UML-style model in a visual designer, then generate the database from it.

### Providers (NuGet packages)

- SQL Server: `Microsoft.EntityFrameworkCore.SqlServer`
- SQLite: `Microsoft.EntityFrameworkCore.Sqlite`. It needs no database server, so it is a good fit for a lab demo.
- PostgreSQL: `Npgsql.EntityFrameworkCore.PostgreSQL`
- MySQL: `MySql.EntityFrameworkCore` (the slide lists the older package name `MySql.Data.EntityFrameworkCore`)
- In-memory, useful for testing: `Microsoft.EntityFrameworkCore.InMemory`
- Migrations from the Package Manager Console also need `Microsoft.EntityFrameworkCore.Tools`.

### `DbContext` and `DbSet<TEntity>`

- **`DbContext`** handles all communication with the database. It:
  - manages the connection
  - configures the model and relationships
  - runs queries
  - saves data
  - tracks changes
  - caches entities
  - manages transactions
- **`DbSet<TEntity>`** represents one table. You query it and save instances of one entity type through it, giving full **CRUD** (Create, Read, Update, Delete). It supports the LINQ extension methods.

### Code First in five steps

1. Create the domain class(es).
2. Create a class that derives from `DbContext`.
3. Expose each entity as a `DbSet<TEntity>` property.
4. Configure the database connection in `OnConfiguring()`.
5. Create the database schema from the classes, using migrations or `EnsureCreated()`.

```csharp
using Microsoft.EntityFrameworkCore;

// Step 1: domain classes. By convention, a property named Id becomes the primary key.
public class Household
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<Expense> Expenses { get; set; } = new();   // one-to-many navigation
}

public class Expense
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int HouseholdId { get; set; }                   // foreign key
    public Household? Household { get; set; }
}

// Steps 2-4: the context
public class AppDbContext : DbContext
{
    public DbSet<Household> Households => Set<Household>();
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=app.db");
}
```

**Step 5: create the schema.** In the Visual Studio Package Manager Console:

```text
Add-Migration InitialCreate
Update-Database
```

Or, for a quick start without migrations, call this at start-up:

```csharp
using var db = new AppDbContext();
db.Database.EnsureCreated();
```

### CRUD with LINQ to Entities

Changes are tracked automatically. Nothing is written to the database until you call **`SaveChanges()`**.

```csharp
using var db = new AppDbContext();

// Create
db.Expenses.Add(new Expense { Description = "Milk", Amount = 4.50m, Date = DateTime.Today, HouseholdId = 1 });
db.SaveChanges();

// Read
List<Expense> recent = db.Expenses
    .Where(e => e.Date >= DateTime.Today.AddDays(-30))
    .OrderByDescending(e => e.Date)
    .ToList();

Household? home = db.Households
    .Include(h => h.Expenses)          // eager-load related rows
    .FirstOrDefault(h => h.Id == 1);

// Update: change the tracked object, then save
Expense? milk = db.Expenses.Find(1);
if (milk != null)
{
    milk.Amount = 5.00m;
    db.SaveChanges();
}

// Delete
if (milk != null)
{
    db.Expenses.Remove(milk);
    db.SaveChanges();
}
```

---

## 12. Unit testing with NUnit

*Source: Week 9*

### Unit testing

- Unit testing checks individual units (methods or classes) in isolation. It is loosely a form of white-box testing.
- **Benefits:** problems are found early, tests are reusable and reliable, the code is easier to maintain, and debugging is simpler.
- **.NET test frameworks:** **MSTest** (built into Visual Studio), **xUnit** (works across .NET languages) and **NUnit** (C#, VB.NET, F#).
- **NUnit** is open source, was first released in 2000, and is currently at version 4.

### Setting up

1. Add a separate test project to the solution. Use the **NUnit Test Project** template, or a class library.
2. Install these NuGet packages: **`NUnit`**, **`NUnit3TestAdapter`** and **`Microsoft.NET.Test.Sdk`**.
3. Add a project reference from the test project to the application project.
4. Run the tests from **Test Explorer** (Test, then Test Explorer). It shows pass and fail results per test.

### The building blocks

- **`[TestFixture]`** goes on a **class**. It groups related tests that share setup or state.
- **`[Test]`** marks a test method.
- **`[SetUp]`** runs **before each** test. Use it for common arrangement. You can have more than one.
- **`[TearDown]`** runs **after each** test. Use it to clean up resources and reset state. You can have more than one.
- **`[TestCase(...)]`** runs the same test with several inputs.
- Test, setup and teardown methods are `public void` and take no parameters. The exception is `[TestCase]` methods, which take the case values as parameters.
- **Execution order for each test:** SetUp, then the test body (Arrange, Act, Assert), then TearDown.

### The Arrange-Act-Assert (AAA) pattern

1. **Arrange:** create objects and set up inputs. This is often done in `[SetUp]`.
2. **Act:** call the method under test.
3. **Assert:** check that the outcome matches the expected result.

```csharp
using NUnit.Framework;

[TestFixture]
public class CalculatorTests
{
    private Calculator calculator = null!;

    [SetUp]
    public void SetUp()
    {
        calculator = new Calculator();               // Arrange (shared by every test)
    }

    [Test]
    public void Add_TwoNumbers_ReturnsSum()
    {
        int result = calculator.Add(2, 3);           // Act
        Assert.That(result, Is.EqualTo(5));          // Assert
    }

    [Test]
    public void Divide_ByZero_Throws()
    {
        Assert.That(() => calculator.Divide(10, 0), Throws.TypeOf<DivideByZeroException>());
    }

    [TestCase(2, true)]
    [TestCase(3, false)]
    public void IsEven_ReturnsExpected(int number, bool expected)
    {
        Assert.That(calculator.IsEven(number), Is.EqualTo(expected));
    }

    [TearDown]
    public void TearDown()
    {
        // release resources, reset state
    }
}
```

### Assertions

- **Constraint model (current, preferred):** `Assert.That(actual, constraint)`, where the constraint is built from `Is`, `Has`, `Does`, `Contains` or `Throws`.

```csharp
Assert.That(total, Is.EqualTo(10.50m));
Assert.That(flag, Is.True);
Assert.That(user, Is.Not.Null);
Assert.That(list, Has.Count.EqualTo(3));
Assert.That(list, Does.Contain("Alice"));
Assert.That(name, Does.StartWith("A"));
Assert.That(() => account.Withdraw(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
```

- **Classic model (legacy):**
  - `Assert.AreEqual(expected, actual)`
  - `Assert.IsTrue(condition)` and `Assert.IsFalse(condition)`
  - `Assert.IsNull(obj)` and `Assert.IsNotNull(obj)`
  - `Assert.Throws<T>(action)`
  - `Assert.Contains(item, collection)`
- In NUnit 4 the classic asserts moved to `ClassicAssert` in the `NUnit.Framework.Legacy` namespace. `Assert.Throws<T>(...)` is still available.

---

## 13. Mapping to Assignment 2 requirements

The marking guide in [Assignment2_Specification.pdf](Assignment2_Specification.pdf) is the source for this checklist. **The code must compile in Visual Studio 2022 with .NET 9.0 or higher, or the submission gets zero marks.**

### Assignment objectives

- [ ] **GUI forms and controls:** [Section 10](#10-gui-with-windows-forms)
- [ ] **Communication between multiple interfaces (forms):** [Section 10, multiple forms](#multiple-forms-and-communication-between-them)
- [ ] **Collections, generics and delegates:** [Section 8](#8-generics-and-collections) and [Section 9](#9-delegates-lambdas-and-linq)
- [ ] **Enumerators, properties and extension methods:** [enums](#enums), [`foreach` and `IEnumerable`](#foreach), [properties](#properties-get-set-value) and [extension methods](#extension-methods)
- [ ] **File or database reading and writing, and Entity Framework:** [Section 7](#7-file-io) and [Section 11](#11-entity-framework-core)
- [ ] **Test cases with NUnit:** [Section 12](#12-unit-testing-with-nunit)

### Code requirements (6 marks)

- [ ] **High cohesion and low coupling:** keep each class focused on one job, such as forms for UI, services for logic, and a `DbContext` for data. Make classes depend on interfaces rather than concrete classes. See [OOP pillars](#the-pillars-of-oop) and [interfaces](#interfaces).
- [ ] **At least one useful example of polymorphism**, through inheritance or method/constructor overloading or overriding. See [overloading](#static-polymorphism-overloading), [overriding](#dynamic-polymorphism-overriding-and-hiding) and [abstract classes](#abstract-classes).
- [ ] **At least two interfaces:** [Section 4, interfaces](#interfaces). Write your own (for example `IRepository<T>` or `IExportable`), not just implementations of built-in ones.
- [ ] **At least one NUnit test:** [Section 12](#12-unit-testing-with-nunit)
- [ ] **At least one anonymous method used with LINQ through a lambda expression:** [method syntax](#method-syntax-lambdas-with-extension-methods), for example `expenses.Where(e => e.Amount > 50)`
- [ ] **At least one use of generics or a generic collection:** [Section 8](#8-generics-and-collections). `List<T>` or `Dictionary<TKey,TValue>` count, and a custom generic class such as `Repository<T>` is stronger.

### Interface design (8.5 marks)

- [ ] **At least four distinct, functional forms or screens** that are **resizable and responsive**. Welcome or splash screens don't count. See [responsive forms](#resizable-and-responsive-forms).
- [ ] **At least six different categories of UI element**, such as buttons, headings, drop-downs, images, lists, grids, context menus, modals, charts and sliders. See [common controls](#common-controls).

### Functionality (8.5 marks)

- [ ] **Core features work** as described in the project report.
- [ ] **Adequate error handling** with `try`/`catch` around file, database and parsing operations. See [Section 6](#6-error-handling).
- [ ] **Input validation**, for example `int.TryParse` or `decimal.TryParse`, required-field checks, range checks, property setters that reject bad values, and `MessageBox` feedback. See [properties](#properties-get-set-value) and [Section 6](#6-error-handling).
- [ ] **Appropriate data structures and algorithms:** choose the right collection. See [which collection to use](#which-collection-to-use).

### Code quality (3 marks)

- [ ] Proper indentation and white space, helpful comments, and meaningful names for classes, methods, properties and fields.

### Bonus (up to 4 marks)

- [ ] Use Blazor, ASP.NET, WPF or another UI library instead of Windows Forms. See [WPF and MAUI](#wpf-and-maui-bonus-marks).
- [ ] Use an external database with LINQ. See [LINQ to Entities](#crud-with-linq-to-entities).
- [ ] Use Entity Framework. See [Section 11](#11-entity-framework-core).
- [ ] Use external APIs or tools, including data analytics or machine learning.
- [ ] Use a machine-learning algorithm. Calling an LLM or VLM API does not count.

### Report and submission

- [ ] Register the project through the form linked in the specification.
- [ ] Write a 1500-2000 word PDF covering the idea, motivation, key features, usage instructions and each member's contribution, with references.
- [ ] Submit one zip containing the whole solution folder, the report and a ReadMe with run instructions. The team leader submits through Canvas by **Friday 16 October 2026, 11:59pm**.
- [ ] Demonstrate the app in the lab, and be ready to explain every component.

---

## 14. Corrections to the lecture slides

These slide examples are wrong as written. This file uses the corrected forms.

- **Week 6, Dictionary:** `new IDictionary<int, string>()` does not compile, because you cannot instantiate an interface. Write `IDictionary<int, string> d = new Dictionary<int, string>();`.
- **Week 2, console input:** `Console.Read()` returns an `int` (the character code), not a `string`, so `userInput = Console.Read();` with a `string` variable does not compile.
- **Weeks 3-4, File class:** `File.ReadAllText` returns a single `string`. It is `File.ReadAllLines` that returns a `string[]` of lines.
- **Week 5, abstract classes:** the abstract member can't be `private`, which is the default when no modifier is written. It must be `public` or `protected`. The derived class must use `public override string TransformName(...)`, because a plain `public` method hides the abstract one instead of implementing it, and that doesn't compile.
- **Week 9, test fixtures:** `[TextFixture]` should be `[TestFixture]`, and it goes on a **class**, not on a `public void` method.
- **Week 9, constraint assertions:** `Assert.That(expected, actual)` is the wrong shape. Write `Assert.That(actual, Is.EqualTo(expected))`, which puts the actual value first.
- **Weeks 3-4, operator overloading:** binary operators (`+ - * / %`) take **two** operands, not one.
- **Weeks 3-4, structs:** "it is an error to define a parameterless constructor or initialise a field in a struct" was true before C# 10. Modern C# (.NET 9) allows both.
- **Week 8, MySQL provider:** `MySql.Data.EntityFrameworkCore` is the old package name. The current official package is `MySql.EntityFrameworkCore`, and `Pomelo.EntityFrameworkCore.MySql` is a popular alternative.
