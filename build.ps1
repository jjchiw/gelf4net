param(
    [Parameter(Position=0,Mandatory=0)]
    [string[]]$taskList = @(),
    [Parameter(Position=1, Mandatory=0)]
    [System.Collections.Hashtable]$properties = @{}
  )

$root = Split-Path -parent $MyInvocation.MyCommand.Definition

Import-Module $root\tools\psake\psake.psm1 -ErrorAction SilentlyContinue

Invoke-psake $root\default.ps1 -properties $properties -taskList $taskList

Remove-Module psake -ErrorAction SilentlyContinue