Start-Sleep (60*10)
while ($true)
{
    Start-Sleep (60*10)
    $energy = Get-Process EnergyComparer -ErrorAction SilentlyContinue
    if ($energy -eq $null) 
    {
        shutdown.exe /r /t (0)
    }
}

