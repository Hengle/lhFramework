@echo off
set str="protocal"
set aa=
set bb=
:STR_DEV
for /f "tokens=1,*" %%a in (%str%) do (
rem echo a= %%a
set str="%%b"
set "aa=  -i:%%a.proto %aa% "
set "bb=%%a.cs %bb% "
goto STR_DEV
)
rem echo b=%aa%
rem echo c=%bb%

cd protogen/protos
"../protogen.exe" %aa% -o:"..\protodll\protos.cs" -d
dir

cd ..
dllcompile.exe ..\protogen\protodll

"../protoprecompile/precompile.exe" "..\protogen\protodll\protos.dll"  -o:"..\protogen\protodll\ProtobufSerializer.dll" -t:ProtobufSerializer

Xcopy "..\protogen\protodll\*.dll" "..\..\..\Program\Assets\Plugins\Protocol" /s /e /y
pause