@echo off
set CONF=%1
if "%CONF%"=="" set CONF=Release
pushd Maestro
dotnet publish -r win-x64 -c %CONF% --self-contained true .\Maestro.csproj -o "..\out\publish\%CONF%"
popd
REM copy addins
REM ExtendedObjectModels
if not exist "%CD%\out\publish\%CONF%\AddIns\ExtendedObjectModels" mkdir "%CD%\out\publish\%CONF%\AddIns\ExtendedObjectModels"
copy /Y "%CD%\Maestro.AddIn.ExtendedObjectModels\bin\%CONF%\Maestro.AddIn.ExtendedObjectModels.dll" "%CD%\out\publish\%CONF%\AddIns\ExtendedObjectModels"
copy /Y "%CD%\Maestro.AddIn.ExtendedObjectModels\bin\%CONF%\Manifest.addin" "%CD%\out\publish\%CONF%\AddIns\ExtendedObjectModels"
REM Rest
if not exist "%CD%\out\publish\%CONF%\AddIns\Rest" mkdir "%CD%\out\publish\%CONF%\AddIns\Rest"
copy /Y "%CD%\Maestro.AddIn.Rest\bin\%CONF%\Maestro.AddIn.Rest.dll" "%CD%\out\publish\%CONF%\AddIns\Rest"
copy /Y "%CD%\Maestro.AddIn.Rest\bin\%CONF%\Manifest.addin" "%CD%\out\publish\%CONF%\AddIns\Rest"
REM Local
if not exist "%CD%\out\publish\%CONF%\AddIns\Local" mkdir "%CD%\out\publish\%CONF%\AddIns\Local"
copy /Y "%CD%\Maestro.AddIn.Local\bin\%CONF%\Maestro.AddIn.Local.dll" "%CD%\out\publish\%CONF%\AddIns\Local"
copy /Y "%CD%\Maestro.AddIn.Local\bin\%CONF%\OSGeo.MapGuide.Desktop.*" "%CD%\out\publish\%CONF%\AddIns\Local"
copy /Y "%CD%\Maestro.AddIn.Local\bin\%CONF%\OSGeo.MapGuide.Foundation.*" "%CD%\out\publish\%CONF%\AddIns\Local"
copy /Y "%CD%\Maestro.AddIn.Local\bin\%CONF%\OSGeo.MapGuide.PlatformBase.*" "%CD%\out\publish\%CONF%\AddIns\Local"
copy /Y "%CD%\Maestro.AddIn.Local\bin\%CONF%\OSGeo.MapGuide.Geometry.*" "%CD%\out\publish\%CONF%\AddIns\Local"
copy /Y "%CD%\Maestro.AddIn.Local\bin\%CONF%\OSGeo.MapGuide.Viewer.*" "%CD%\out\publish\%CONF%\AddIns\Local"
copy /Y "%CD%\Maestro.AddIn.Local\bin\%CONF%\OSGeo.MapGuide.MaestroAPI.Local.*" "%CD%\out\publish\%CONF%\AddIns\Local"
copy /Y "%CD%\Maestro.AddIn.Local\bin\%CONF%\Manifest.addin" "%CD%\out\publish\%CONF%\AddIns\Local"
xcopy /s /y /i "%CD%\Maestro.AddIn.Local\Dictionaries" "%CD%\out\publish\%CONF%\AddIns\Local\Dictionaries"
xcopy /s /y /i "%CD%\Packages\mapguide-api-base-x64\3.1.2.9484\mapguide-api-base\*.*" "%CD%\out\publish\%CONF%\AddIns\Local"
xcopy /s /y /i "%CD%\Packages\mg-desktop-x64\3.1.2.9484\mg-desktop\*.*" "%CD%\out\publish\%CONF%\AddIns\Local"
REM Scripting
if not exist "%CD%\out\publish\%CONF%\AddIns\Scripting" mkdir "%CD%\out\publish\%CONF%\AddIns\Scripting"
copy /Y "%CD%\Maestro.AddIn.Scripting\bin\%CONF%\Maestro.AddIn.Scripting.dll" "%CD%\out\publish\%CONF%\AddIns\Scripting"
copy /Y "%CD%\Maestro.AddIn.Scripting\bin\%CONF%\Manifest.addin" "%CD%\out\publish\%CONF%\AddIns\Scripting"