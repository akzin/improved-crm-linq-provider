@echo off
rem copy /y ..\Akzin.Crm.Linq\bin\Release\ILMerge\Akzin.Crm.Linq.dll Folder\lib\net45
cd folder
del *.nupkg
..\nuget pack
copy /y *.nupkg ..
cd ..