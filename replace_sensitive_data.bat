@echo off
setlocal enabledelayedexpansion

for /f "tokens=*" %%i in (%1) do (
    set "line=%%i"
    set "line=!line:"Server"="""!
    set "line=!line:"Database"="""!
    set "line=!line:"UserId"="""!
    set "line=!line:"Password"=""""!
    set "line=!line:"Port"=""""!
    set "line=!line:"CLIENT_ORIGIN_URL"=""""!
    set "line=!line:"AUTH0_AUDIENCE"=""""!
    set "line=!line:"AUTH0_DOMAIN"=""""!
    echo !line!
)

endlocal