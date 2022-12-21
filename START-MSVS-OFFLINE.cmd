@ECHO OFF
SET WORKDIR=%~dp0
PUSHD %WORKDIR% && (CALL scons.bat prepare-dotnet && CALL scons.bat msvs MSVSVERSION=10.0) || PAUSE
POPD