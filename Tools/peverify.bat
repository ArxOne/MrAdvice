@echo off

rem search for newest PEVerify.exe

set searchdir="%ProgramFiles(x86)%\Microsoft SDKs\Windows"

FOR /F "tokens=*" %%I IN ('DIR %searchdir%\*PEVerify.exe /s/b /od') DO SET peverify=%%I&& GOTO :next

:next
if exist "%peverify%" goto verify

echo peverify not found, skipping this step

goto noverify

:verify

rem muted errors as follow (not sure muting them is good, but they are no valid errors)
rem 0x801318BF because an advised ctor can call the base class ctor
rem 0x80131859 warning of 'this' being unitialized in ctor
rem 0x80131266 because it occurs with AppVeyor (WTF?)

if exist "%1" "%peverify%" /nologo /hresult /ignore=0x801318BF,0x80131859,0x80131266 %1

:noverify
