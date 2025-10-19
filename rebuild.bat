@echo off
setlocal
echo Rebuild MSALAuthApp (prefers Visual Studio BuildTools msbuild)

:: Solution directory (this script must live in repo root)
set "SOLUTION_DIR=%~dp0"
set "MSBUILD_PATH="

:: Prefer Visual Studio BuildTools MSBuild, then Community, then framework msbuild, then PATH msbuild
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
  set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" (
  set "MSBUILD_PATH=C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" (
  set "MSBUILD_PATH=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
) else (
  set "MSBUILD_PATH=msbuild"
)

echo Using MSBuild: %MSBUILD_PATH%
"%MSBUILD_PATH%" "%SOLUTION_DIR%MSALAuthApp.sln" /p:Configuration=Debug /m:1 /v:minimal
if errorlevel 1 (
  echo ERROR: Build failed.
  pause
  exit /b 1
)
echo Build succeeded.

:: Copy WebView2 extension DLL and runtime files into main bin\Debug (if present)
if exist "%SOLUTION_DIR%WebView2Extension\bin\Debug\WebView2Extension.dll" (
  echo Copying WebView2 extension into main app output...
  copy /Y "%SOLUTION_DIR%WebView2Extension\bin\Debug\WebView2Extension.dll" "%SOLUTION_DIR%bin\Debug\" >nul
  copy /Y "%SOLUTION_DIR%WebView2Extension\bin\Debug\Microsoft.Web.WebView2.*.dll" "%SOLUTION_DIR%bin\Debug\" >nul 2>&1
  if exist "%SOLUTION_DIR%packages\Microsoft.Web.WebView2.1.0.2210.55\runtimes\win-x64\native\WebView2Loader.dll" (
    copy /Y "%SOLUTION_DIR%packages\Microsoft.Web.WebView2.1.0.2210.55\runtimes\win-x64\native\WebView2Loader.dll" "%SOLUTION_DIR%bin\Debug\" >nul
  )
  echo Copied extension and runtime artifacts to bin\Debug
) else (
  echo No WebView2Extension.dll found at WebView2Extension\bin\Debug
)

echo Done.
pause
endlocal
