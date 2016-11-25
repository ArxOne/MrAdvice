# Mr. Advice

## Summary

Mr. Advice is an open source (and free of charge) alternative to PostSharp (which is still far more advanced, see https://www.postsharp.net).  
It intends to inject aspects at build-time. Advices are written in the form of attributes, and marking methods with them makes the pointcut, resulting in a nice and easy aspect injection.  
More information about what is an aspect at [Wikipedia](http://en.wikipedia.org/wiki/Aspect-oriented_programming).  

Mr. Advice can weave assemblies for:

* .NET framework (4 and above) / Mono
* Silverlight (4 & 5)
* Universal Windows Platform 
* .NET Standard 1.6

Mr. Advice allows you to:
* Advise methods or parameters, at assembly, type, method or parameter level
* Advice types (at assembly startup)
* Introduce fields
* Advise Mr. Advice (and this is **BIG**) at weaving-time (during build step), so you can rename methods as they are advised, add properties, etc.

## How it works

It is available as a NuGet package (https://www.nuget.org/packages/MrAdvice). There is also an automatic build with tests at appveyor. The current status is [![Build status](https://ci.appveyor.com/api/projects/status/96i8xbxf954x79vw?svg=true)](https://ci.appveyor.com/project/picrap/mradvice)


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

## Contributing

Currently, Mr. Advice still exists in two flavors:
* The brand new version using dnlib, in [master branch](https://github.com/ArxOne/MrAdvice). This is the branch you can contribute to, by forking and submitting pull requests.
* A legacy version using Fody and Cecil still exists in [fody branch](https://github.com/ArxOne/MrAdvice/tree/fody). This version still exists in NuGet, and is available under ID [MrAdvice.Fody](https://www.nuget.org/packages/MrAdvice.Fody/). However, it is not supported anymore and may disappear in the future.

## Other projects using Mr. Advice

NuGet packages:
 * [MrAdvice.MVVM](https://github.com/ArxOne/MrAdvice.MVVM) and its [NuGet package](https://www.nuget.org/packages/MrAdvice.MVVM/), an MVVM implementation using aspects.

Miscellaneous projects:

 * [The Blue Dwarf](https://github.com/picrap/BlueDwarf), a tunneling anti-censorship local proxy.

## Contact and links

Project owner is [picrap](https://github.com/picrap), feel free to drop a mail :email:.  
Project company is [Arx One](http://arx.one), a french company editor of backup software solutions.  
