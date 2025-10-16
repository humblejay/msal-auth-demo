@echo off
echo Building MSAL Auth App with WebView2 Extension...

echo.
echo Building WebView2 Extension DLL...
"%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" WebView2Extension\WebView2Extension.csproj -p:Configuration=Debug
if errorlevel 1 (
    echo ERROR: Failed to build WebView2Extension
    pause
    exit /b 1
)

echo.
echo Copying WebView2 Extension files to main app...
copy "WebView2Extension\bin\Debug\WebView2Extension.dll" "bin\Debug\"
copy "WebView2Extension\bin\Debug\Microsoft.Web.WebView2.*.dll" "bin\Debug\"

echo.
echo Ensuring WebView2Loader.dll is available...
copy "packages\Microsoft.Web.WebView2.1.0.2210.55\runtimes\win-x64\native\WebView2Loader.dll" "bin\Debug\"

echo.
echo Building main application...
"%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" MSALAuthApp.csproj -p:Configuration=Debug
if errorlevel 1 (
    echo ERROR: Failed to build main application
    pause
    exit /b 1
)

echo.
echo ✅ Build completed successfully!
echo You can now run: bin\Debug\MSALAuthApp.exe
echo.
pause