# MrAdvice

## Summary

MrAdvice is an open source (and free of charge) alternative to PostSharp (which is still far more advanced, see https://www.postsharp.net).  
It intends to inject aspects at build-time. Advices are written in the form of attributes, and marking methods with them makes the pointcut, resulting in a nice and easy aspect injection.  
More information about what is an aspect at [Wikipedia](http://en.wikipedia.org/wiki/Aspect-oriented_programming).  

MrAdvice can weave assemblies for:

* .NET framework (4 and above)
* Mono
* Silverlight (4 & 5)
* Windows Phone (8 and above)

## How it works

It is available as a NuGet package (https://www.nuget.org/packages/MrAdvice.Fody). There is also an automatic build with tests at appveyor. The current status is [![Build status](https://ci.appveyor.com/api/projects/status/96i8xbxf954x79vw?svg=true)](https://ci.appveyor.com/project/picrap/mradvice)


## Philosophy

Currently, MrAdvice won't bring you any aspect out-of-the-box.
This means you'll have to write your own aspects (however you can see below other packages using Mr. Advice).  
So it brings us to the next chapter, which is...

## How to implement your own aspects

### In a nutshell

Here is the minimal sample:
```csharp
public class MyProudAdvice : Attribute, IMethodAdvice
{
    public void Advise(MethodAdviceContext context)
    {
        // do things you want here
        context.Proceed(); // this calls the original method
        // do other things here
    }
}
```
You then just need to mark the method(s) with the attribute and that's it, your aspect is injected!

```csharp
[MyProudAdvice]
public void MyProudMethod()
{
}
```

### More details

Your aspects can be injected at assembly, type or method level, simply by setting the attribute:

* When an aspect is injected at asembly level, all methods of all types are weaved.
* When the aspect is injected at type level, all of its methods are weaved.
* And of course, if the aspect is injected on a method, only the method is weaved.

## Other projects using Mr. Advice

### NuGet packages

 * [MrAdvice.MVVM](https://github.com/ArxOne/MrAdvice.MVVM) and its [NuGet package](https://www.nuget.org/packages/MrAdvice.MVVM/), an MVVM implementation using aspects.

### Miscellaneous projects

 * [The Blue Dwarf](https://github.com/picrap/BlueDwarf), a tunneling anti-censorship local proxy.

## Some disclaimer 
(almost hidden at the almost end of this almost interesting readme)

MrAdvice exposes an API (such as `IAdvice`, `IMethodAdvice`, etc.), which is subject to change until we reach the version 1.0.  
Of course, there shouldn't be any structural change, but methods might be renamed, or parameters changed if we need it to improve the product.

## Contact and links

Project owner is [picrap](https://github.com/picrap), feel free to drop a mail :email:.  
Project company is [Arx One](http://arxone.com), a french company editor of backup software solutions.  
