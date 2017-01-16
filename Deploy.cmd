SET OLDDIR=%CD%
SET CURRDIR=%~dp0

CD /d %CURRDIR%

ServiceFabricAppPackageUtil.exe /source:DummyStatelessService\bin\x64\Debug /target:PipServices.Dummy.Application\pkg\Debug /appname:DummyStatelessServicePkg /exe:DummyStatelessService.exe /ma:app /AppType:PipServices.Dummy.ApplicationType

CD PipServices.Dummy.Application\pkg\Debug\DummyStatelessServicePkg

rename C Code
rename config Config
CD Config
rename settings.xml Settings.xml
CD ..

copy ..\..\..\..\Settings.xml Config\Settings.xml
copy ..\..\..\..\DummyStatelessService\PackageRoot\ServiceManifest.xml ServiceManifest.xml
CD ..
copy ..\..\..\PipServices.Dummy.Application\ApplicationPackageRoot\ApplicationManifest.xml ApplicationManifest.xml
copy ..\..\..\PipServices.Dummy.Application\ApplicationPackageRoot\ApplicationManifest.xml ApplicationManifest.xml

CD /d %OLDDIR%

CD PipServices.Dummy.Application

powershell.exe Scripts\update-cluster-dummy.ps1

CD /d %OLDDIR%
