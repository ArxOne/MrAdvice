rem all of this because msbuild command line does not work well with packages restore

cd ..

set solution=MrAdvice.sln
.nuget\nuget.exe restore %solution%
msbuild %solution% /m /p:Configuration=Debug /p:Platform="Any CPU"
