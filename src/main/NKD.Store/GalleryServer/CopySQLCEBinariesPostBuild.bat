:: Assumes incoming variables are surrounded by quotation marks.
set sourcePath=%~1
set destinationPath=%~2
xcopy /S /Y "%sourcePath%packages\SqlServerCompact.4.0.8482.1\NativeBinaries\*" "%destinationPath%"
xcopy /S /Y "%sourcePath%..\lib\Microsoft.VC90.CRT\x86\*" "%destinationPath%x86\Microsoft.VC90.CRT\"
xcopy /S /Y "%sourcePath%..\lib\Microsoft.VC90.CRT\amd64\*" "%destinationPath%amd64\Microsoft.VC90.CRT\"
set/A errlev="%ERRORLEVEL% & 24"
