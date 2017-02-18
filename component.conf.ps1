$VersionControl = 'git'

$Package = 'nuget'
$PackageSource = ''

$Build = 'visualstudio'
$Document = 'none'
$Test = 'mstest'

$Deploy = 'servicefabric'
$DeployServer = 'local'
$DeployComponent = ''
$DeployConfigs = @(
    @{
        Server = "local";
        Uri = "localhost:19000";
        Profile = "Local.1Node.xml";
    }
)

$Run = 'none'