Remove-Item ..\QuickLook.Plugin.LottieFilesViewer.qlplugin -ErrorAction SilentlyContinue

$files = Get-ChildItem -Path ..\Build\Release\ -Exclude *.pdb,*.xml
Compress-Archive $files ..\QuickLook.Plugin.LottieFilesViewer.zip
Move-Item ..\QuickLook.Plugin.LottieFilesViewer.zip ..\QuickLook.Plugin.LottieFilesViewer.qlplugin