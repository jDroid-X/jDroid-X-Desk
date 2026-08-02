node_fake -v  
IF %0% NEQ 0 (  
ECHO Downloading...  
powershell -Command \" Invoke-WebRequest -Uri https://nodejs.org/dist/v20.17.0/node-v20.17.0-x64.msi -OutFile %C:\Users\jiten\AppData\Local\Temp%\node_installer.msi "\  
ECHO Launching MSI  
)  
