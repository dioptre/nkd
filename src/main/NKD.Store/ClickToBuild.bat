@echo off
cls


:: Build the Plugins Project
cd /d Plugins
call build.bat IntegrationTests
set buildErrorLevel=%errorlevel%
cd /d ..
if %buildErrorLevel% neq 0 (goto end)


:: Copy Plugin Files to Gallery Server Project
echo Copying Plugin DLLs to Gallery Server
set destinationDirectory="GalleryServer\src\Gallery.Server\bin\"
if not exist %destinationDirectory% md %destinationDirectory%
copy "Plugins\src\NuPackPackageFactory\bin\Debug\NuGet.Core.dll" %destinationDirectory%
copy "Plugins\src\NuPackPackageFactory\bin\Debug\NuPackPackageFactory.dll" %destinationDirectory%


:: Remove config files if flag given and they exist.
if "%1" neq "--delete-configs" (goto buildGalleryServer)
echo Deleting config files.
set appSettingsFile="GalleryServer\src\Gallery.Server\AppSettings.config"
set connectionStringsFile="GalleryServer\src\Gallery.Server\ConnectionStrings.config"
if exist %appSettingsFile% (del %appSettingsFile%)
if exist %connectionStringsFile% (del %connectionStringsFile%)


:: Build the Gallery Server Project
:buildGalleryServer
cd /d GalleryServer
call build.bat IntegrationTests
set buildErrorLevel=%errorlevel%
cd /d ..
if %buildErrorLevel% neq 0 (goto end)


:: Successful build
set buildErrorLevel=0


:end
pause
exit %buildErrorLevel%
