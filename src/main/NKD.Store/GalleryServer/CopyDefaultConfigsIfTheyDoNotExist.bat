:: %~1 returns the input variable without the first and last character (i.e. without the quotes).
:: Assuming the input variable is coming in with quotation marks.
set destinationDirectory=%~1
set configDirectory=%destinationDirectory%Config\

set appSettingsFileName=AppSettings.config
set appSettingsDefaultFileName=%appSettingsFileName%.default
set appSettingsFullPath="%destinationDirectory%%appSettingsFileName%"
set appSettingsDefaultFullPath="%configDirectory%%appSettingsDefaultFileName%"

set connectionStringsFileName=ConnectionStrings.config
set connectionStringsDefaultFileName=%connectionStringsFileName%.default
set connectionStringsFullPath="%destinationDirectory%%connectionStringsFileName%"
set connectionStringsDefaultFullPath="%configDirectory%%connectionStringsDefaultFileName%"

if not exist %appSettingsFullPath% copy %appSettingsDefaultFullPath% %appSettingsFullPath%
if not exist %connectionStringsFullPath% copy %connectionStringsDefaultFullPath% %connectionStringsFullPath%