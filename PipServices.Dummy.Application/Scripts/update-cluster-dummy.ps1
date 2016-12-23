#
# update_cluster_dummy.ps1
#
Connect-ServiceFabricCluster localhost:19000

Write-Host 'Handling dummy service'

Write-Host 'Copying application package...'
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath '.\pkg\debug' -ImageStoreConnectionString 'file:C:\SfDevCluster\Data\ImageStoreShare' -ApplicationPackagePathInImageStore 'Store\PipServices.Dummy.ApplicationType'

Write-Host 'Registering application type...'
Register-ServiceFabricApplicationType -ApplicationPathInImageStore 'Store\PipServices.Dummy.ApplicationType' -TimeoutSec 600

Write-Host 'Creating new application...'
New-ServiceFabricApplication -ApplicationName 'fabric:/PipServices.Dummy.Application' -ApplicationTypeName 'PipServices.Dummy.ApplicationType' -ApplicationTypeVersion 1.0.0 -TimeoutSec 600
