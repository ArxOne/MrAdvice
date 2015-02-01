# Weavisor

## Summary

Weavisor is an open source (and free or charge) alternative to PostSharp (which is still far more advanced, see https://www.postsharp.net/)
Weavisor can weave assemblies for
* .NET framework (4 and above)
* Mono
* Silverlight (4 & 5)
* Windows Phone (8 and above)

## How it works

It will come as a NuGet package using Fody weaver, but currently, the job is not done.

## Philosophy

Currently, Weavisor won't bring you any aspect out-of-the-box.
This means you'll have to write your own.

## How to implement your own aspects

Here is the minimal sample:
```csharp
public class ChangeParameter : Attribute, IMethodAdvice
{
	public void Advise(Call<MethodCallContext> call)
	{
	    // do things you want here
        call.Proceed(); // this calls the original method
		// do other things here
    }
}
```
