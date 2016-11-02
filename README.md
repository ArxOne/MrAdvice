# Mr. Advice

## News

**2016-10-02**: Mr. Advice using dnlib is now released, under another package name, at https://www.nuget.org/packages/MrAdvice. If it behaves well with everyone, we are going to deprecate the version using Fody (because we can fly with our own wings :sunglasses:).  
**2016-08-16**: No, Mr. Advice is not dead, but we're making undergoing changes.  
The master branch is globally inactive, because the project is currently progressing in two other branches:
 * [fody](https://github.com/ArxOne/MrAdvice/tree/fody) where it keeps using Fody and Cecil, as it was written at first place.
 * [dnlib](https://github.com/ArxOne/MrAdvice/tree/dnlib), where it flies solo, using the excellent [dnlib](https://github.com/0xd4d/dnlib) and some other projects of my (main contributor [picrap](https://github.com/picrap)) own ([StitcherBoy](https://github.com/picrap/StitcherBoy) and [Blobber](https://github.com/picrap/Blobber)).

Both branches are available as separate NuGet packages ([fody](https://www.nuget.org/packages/MrAdvice.Fody/) and [dnlib](https://www.nuget.org/packages/MrAdvice/), this latter being currenly unreleased); if dnlib version works, we'll probably drop the fody branch.

## Summary

Mr. Advice is an open source (and free of charge) alternative to PostSharp (which is still far more advanced, see https://www.postsharp.net).  
It intends to inject aspects at build-time. Advices are written in the form of attributes, and marking methods with them makes the pointcut, resulting in a nice and easy aspect injection.  
More information about what is an aspect at [Wikipedia](http://en.wikipedia.org/wiki/Aspect-oriented_programming).  

Mr. Advice can weave assemblies for:

* .NET framework (4 and above)
* Mono
* Silverlight (4 & 5)
* Windows Phone (8 and above)
* Universal Windows Platform 

Mr. Advice allows you to:
* Advise methods or parameters, at assembly, type, method or parameter level
* Advice types (at assembly startup)
* Introduce fields
* Advise Mr. Advice (and this is **BIG**) at weaving-time (during build step), so you can rename methods as they are advised, add properties, etc.

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

NuGet packages:
 * [MrAdvice.MVVM](https://github.com/ArxOne/MrAdvice.MVVM) and its [NuGet package](https://www.nuget.org/packages/MrAdvice.MVVM/), an MVVM implementation using aspects.

Miscellaneous projects:

 * [The Blue Dwarf](https://github.com/picrap/BlueDwarf), a tunneling anti-censorship local proxy.

## Contact and links

Project owner is [picrap](https://github.com/picrap), feel free to drop a mail :email:.  
Project company is [Arx One](http://arx.one), a french company editor of backup software solutions.  
