rem this is a bit dirty here, but I found no other way to locate peverify

rem if you have any other path, duplicate the lines below and add your own path
set peverify="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\PEVerify.exe"
if exist %peverify% goto verify

goto noverify

:verify

rem 0x801318AA is because of Action(method)
rem 0x801318BF because an advised ctor can call the base class ctor
rem 0x80131859 warning of 'this' being unitialized in ctor
%peverify% /nologo /hresult /ignore=0x801318AA,0x801318BF,0x80131859 %1

:noverify
