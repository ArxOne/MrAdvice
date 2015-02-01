# Weavisor

## Summary

Weavisor is an open source (and free of charge) alternative to PostSharp (which is still far more advanced, see https://www.postsharp.net/)
Weavisor can weave assemblies for:

* .NET framework (4 and above)
* Mono
* Silverlight (4 & 5)
* Windows Phone (8 and above)

## How it works

It will come as a NuGet package using Fody weaver, but currently, the job is not done (stay tuned, it will be available before the end of February 2015).

## Philosophy

Currently, Weavisor won't bring you any aspect out-of-the-box.
This means you'll have to write your own aspects.

## How to implement your own aspects

### In a nutshell

Here is the minimal sample:
```csharp
public class MyProudAdvice : Attribute, IMethodAdvice
{
	public void Advise(Call<MethodCallContext> call)
	{
	    // do things you want here
        call.Proceed(); // this calls the original method
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
