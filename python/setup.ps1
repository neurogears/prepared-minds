if (!(Test-Path "./winpython")) {
    Invoke-WebRequest "https://github.com/winpython/winpython/releases/download/4.3.20210620/Winpython64-3.8.10.0dot.exe" -OutFile "temp.exe"
	$process = Start-Process "temp.exe" "-y" -PassThru
	$process.WaitForExit()
	Rename-Item -Path "WPy64-38100" -NewName "winpython"
    Remove-Item -Path "temp.exe"
}

cmd.exe /c '.\winpython\scripts\env.bat && pip install -r requirements.txt'