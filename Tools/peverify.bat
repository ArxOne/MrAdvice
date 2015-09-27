rem this is a bit dirty here, but I found no other way to locate peverify

rem if you have any other path, duplicate the lines below and add your own path
set peverify="C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\PEVerify.exe"
if exist %peverify% goto verify

set peverify="C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\NETFX 4.0 Tools\PEVerify.exe"
if exist %peverify% goto verify

set peverify="C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\PEVerify.exe"
if exist %peverify% goto verify

echo peverify not found, skipping this step

goto noverify

:verify

rem muted errors as follow (not sure muting them is good, but they are no valid errors)
rem 0x801318BF because an advised ctor can call the base class ctor
rem 0x80131859 warning of 'this' being unitialized in ctor
rem 0x8013184F because .ctor inner (wrapped original) method calls base..ctor()
if exist "%1" %peverify% /nologo /hresult /ignore=0x801318BF,0x80131859,0x8013184F %1

:noverify
