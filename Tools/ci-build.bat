rem all of this because msbuild command line does not work well with packages restore

set solution=MrAdvice.sln
.nuget\nuget.exe restore %solution%
msbuild %solution% /m /p:Configuration=Release /p:Platform="Any CPU"
