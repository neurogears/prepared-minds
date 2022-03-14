if (!(Test-Path "./winpython")) {
    Invoke-WebRequest "https://github.com/winpython/winpython/releases/download/4.1.20210417/Winpython64-3.8.9.0dot.exe" -OutFile "temp.exe"
	$process = Start-Process "temp.exe" "-y" -PassThru
	$process.WaitForExit()
	Rename-Item -Path "WPy64-3890" -NewName "winpython"
    Remove-Item -Path "temp.exe"
}

cmd.exe /c '.\winpython\scripts\env.bat && pip install -r requirements.txt'