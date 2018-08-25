set ProjectPath=%1
set TargetDir=%2
set ConfigurationName=%3

nuget pack "%ProjectPath%" -Properties Configuration=%ConfigurationName%
if defined NUGET_REPOSITORY (echo NUGET_REPOSITORY is defined: %NUGET_REPOSITORY%) else (echo NUGET_REPOSITORY is NOT defined; exit 1)
move /Y "%TargetDir%*.nupkg" "%NUGET_REPOSITORY%"
