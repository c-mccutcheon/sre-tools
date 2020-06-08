###
#
# This script is used to create organisation & project level groups for new project creation and performs most of 
# the standard assignments in line with the IAM documentation in confluence here:
# https://confluence.devops.lloydsbanking.com/pages/viewpage.action?pageId=367101137
#
### Params to pass in
param(
    [string] 
    $org = "lbg-cloudfirst",
    [string]
    $project = "Build Release Spike",
    [string]
    $prjAbrv = "BRS"
)
###

# Deines the list of Organisation level Groups to be created
$orgGroups = [ordered]@{
    "ADO_CNEB_PRJ_CyberEng" = "PROJ - Cyber security engineers on the project";
    "ADO_CNEB_PRJ_DevLeads" = "PROJ - Dev leads are generally the Engineering Leads within a Lab and share some of the SRE tasks together with an elevated level sign off";
    "ADO_CNEB_PRJ_Devs" = "PROJ - Developers on the project";
    "ADO_CNEB_PRJ_Guest" = "PROJ - Guest user with reader permissions to the Project";
    "ADO_CNEB_PRJ_LabSRE" = "PROJ - Guest user with reader permissions to the Project";
    "ADO_CNEB_PRJ_ProjectMgmt" = "PROJ - Group for the Project Management function, purpose is to act as the release gate where process requires it";
    "ADO_CNEB_PRJ_SnrDevs" = "PROJ - Senior Devs are the next level down from Dev Leads and are generally responsible for a more senior level of approval in the PR process";
    "ADO_CNEB_PRJ_SrvAcc" = "PROJ - Project level service account"
}

#Defines the list of Project level groups to be created
$prjGroups = [ordered]@{
    "SignOff_Cyber" = "Sign off groups for changes requiring Cyber Security approval";
    "SignOff_NetSec" = "Sign off group for Network Security related changes";
    "SignOff_PolicyOwner" = "Sign off group for Policy Owners";
    "SignOff_ReleaseMgmt" = "Sign off group for where a release needs approval";
    "SignOff_SeniorEng" = "Sign off group for senior engineers";
    "SignOff_SRE" = "Sign off group for SRE";
    "SignOff_WAGF" = "Sign off group for WAFG changes";
    "Service Accounts" = "Team for non-human service accounts that perform automated tasks"
}

$grpDescriptors = @{}

# Tests the output from the az command for errors
Function Test-Output($output) {
    if (!($output | ConvertFrom-Json)) {
        Write-Error "Error querying security group list"
        return
    }
}

# Queries the Groups that exist in the AzDO org and creates a structured list of this together with descriptors
Function Create-GroupsHash {
    Param(
        $grps = @{}
    )    
    $descriptor = ""
    $json = az devops security group list --scope organization
    Test-Output $output
    $json | ForEach-Object {
        $line = $_
        $found = $line -match '"descriptor": "(.*)"'
        if ($found) {
            $descriptor = $Matches[1]
        }

        $found2 = $line -match '"principalName": "\[(.*)\]\\\\(.*)"'
        if ($found2) {
            $scope = $Matches[1]
            $grpName = $Matches[2]

            if (!$grps.contains($scope)) {
                $grps.add($scope, @{});
            }
            $grps[$scope].add($grpName,$descriptor)
        }
    }
    return $grps
}

# Adds passed in child groups as members of the parent security group in AzDO
Function Add-Groups() {
    Param(
        # Structured object of security groups present in AzDO
        [Parameter(Mandatory=$true)]
        $groups,
        # The parent group name (i.e. the group to add members to)
        [Parameter(Mandatory=$true)]
        [String]
        $parent,
        # Array of child group names (i.e. the groups to add as members)
        [String[]]
        $children
    )

    $pDesc = ""
    $found = $parent -match '(.*)\/(.*)'
    if ($found) {
        $pDesc = $groups[$Matches[1]][$Matches[2]]
    }

    $children | ForEach-Object {
        "parent: $parent | child: $_"

        $cDesc = ""
        $found = $_ -match '(.*)\/(.*)'
        if ($found) {
            $cDesc = $groups[$Matches[1]][$Matches[2]]
        }
        $output = az devops security group membership add --group-id $pDesc --member-id $cDesc
        Test-Output $output
    }
}

# Set AzDO defaults for CLI
az devops configure --defaults organization=https://dev.azure.com/lbg-cloudfirst project=$project

# Cycle through and create all the org level groups
$orgGroups.GetEnumerator() | ForEach-Object {
    $grp = $_.key -replace "_PRJ_", "_$($prjAbrv)_"
    $msg = $_.value -replace "PROJ - ", "$($project) - "
    "creating - $grp"
    $output = az devops security group create --name $grp --description $msg --scope organization
    Test-Output $output
}

# Cycle through and create all the project level groups
$prjGroups.GetEnumerator() | ForEach-Object {
    "creating - $($_.key)"
    $output = az devops security group create --name $_.key --description $_.value --scope project --project "$project"
    Test-Output $output
}

$grpHash = Create-GroupsHash

# now add the org (& project) level groups as member od the right project groups
Add-Groups -groups $grpHash -parent "$project/SignOff_Cyber"                    -children @("$org/ADO_CNEB_$($prjAbrv)_CyberEng")
Add-Groups -groups $grpHash -parent "$project/SignOff_NetSec"                   -children @("$org/ADO_CNEB_GLO_NetSec")
Add-Groups -groups $grpHash -parent "$project/SignOff_PolicyOwner"              -children @("$org/ADO_CNEB_GLO_PolicyOwner")
Add-Groups -groups $grpHash -parent "$project/SignOff_ReleaseMgmt"              -children @("$org/ADO_CNEB_$($prjAbrv)_ProjectMgmt")
Add-Groups -groups $grpHash -parent "$project/SignOff_SeniorEng"                -children @("$org/ADO_CNEB_$($prjAbrv)_DevLeads",
                                                                                            "$org/ADO_CNEB_$($prjAbrv)_SnrDevs")
Add-Groups -groups $grpHash -parent "$project/SignOff_SRE"                      -children @("$org/ADO_CNEB_$($prjAbrv)_LabSRE")
Add-Groups -groups $grpHash -parent "$project/SignOff_WAGF"                     -children @("$org/ADO_CNEB_GLO_WAGF")

Add-Groups -groups $grpHash -parent "$project/Build Administrators"             -children @("$project/Service Accounts",
                                                                                            "$org/ADO_CNEB_$($prjAbrv)_DevLeads",
                                                                                            "$org/ADO_CNEB_$($prjAbrv)_LabSRE")
Add-Groups -groups $grpHash -parent "$project/Contributors"                     -children @("$org/ADO_CNEB_$($prjAbrv)_CyberEng",
                                                                                            "$org/ADO_CNEB_$($prjAbrv)_DevLeads",
                                                                                            "$org/ADO_CNEB_$($prjAbrv)_Devs",
                                                                                            "$org/ADO_CNEB_$($prjAbrv)_LabSRE",
                                                                                            "$org/ADO_CNEB_$($prjAbrv)_ProjectMgmt",
                                                                                            "$org/ADO_CNEB_$($prjAbrv)_SnrDevs")
Add-Groups -groups $grpHash -parent "$project/Deployment Group Administrators"  -children @("$org/ADO_CNEB_$($prjAbrv)_LabSRE")
Add-Groups -groups $grpHash -parent "$project/Endpoint Administrators"          -children @("$project/Service Accounts",
                                                                                            "$org/ADO_CNEB_$($prjAbrv)_LabSRE")
Add-Groups -groups $grpHash -parent "$project/Project Administrators"           -children @("$org/ADO_CNEB_GLO_PlatformSRE")
Add-Groups -groups $grpHash -parent "$project/Readers"                          -children @("$project/Contributors",
                                                                                            "$project/Service Accounts",
                                                                                            "$org/ADO_CNEB_$($prjAbrv)_Guest",
                                                                                            "$project/SignOff_Cyber",
                                                                                            "$project/SignOff_NetSec",
                                                                                            "$project/SignOff_PolicyOwner",
                                                                                            "$project/SignOff_ReleaseMgmt",
                                                                                            "$project/SignOff_SeniorEng",
                                                                                            "$project/SignOff_SRE",
                                                                                            "$project/SignOff_WAGF")
Add-Groups -groups $grpHash -parent "$project/Release Administrators"           -children @("$org/ADO_CNEB_$($prjAbrv)_DevLeads",
                                                                                            "$org/ADO_CNEB_$($prjAbrv)_LabSRE")
Add-Groups -groups $grpHash -parent "$project/Service Accounts"                 -children @("$org/ADO_CNEB_$($prjAbrv)_SrvAcc",
                                                                                            "$org/ADO_CNEB_GLO_SrvAcc")
