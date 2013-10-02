param($installPath, $toolsPath, $package, $project)

$file1 = $project.ProjectItems.Item("offreg.x64.dll")
$file2 = $project.ProjectItems.Item("offreg.x86.dll")

# set 'Copy To Output Directory' to 'Copy if newer'
$copyToOutput1 = $file1.Properties.Item("CopyToOutputDirectory")
$copyToOutput1.Value = 2

$copyToOutput2 = $file2.Properties.Item("CopyToOutputDirectory")
$copyToOutput2.Value = 2