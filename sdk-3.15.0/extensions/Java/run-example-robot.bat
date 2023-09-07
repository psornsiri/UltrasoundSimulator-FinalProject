@echo off

REM  Force Dimension Haptic SDK 3.15.0$SUFFIX
REM  Copyright (C) 2001-2022 Force Dimension
REM  All Rights Reserved.
REM
REM  contact: support@forcedimension.com


if not exist "%JAVA_HOME%\bin\java.exe" goto ERROR
echo using JDK located at %JAVA_HOME%

echo launching 'robot' example...
java -cp classes;classes\com\forcedimension\examples robot
goto END

:ERROR
echo JRE not found, exiting...

:END
