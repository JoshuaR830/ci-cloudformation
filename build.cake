// Based on the content-api build.cake file - trying to work out what is going on

#addin nuget:?package=AWSSDK.IdentityManagement&version=3.3.106.34&loaddependencies=true
#addin nuget:?package=AWSSDK.CloudFormation&version=3.3.106.21&loaddependencies=true
#addin nuget:?package=AWSSDK.SecurityToken&version=3.3.105.43&loaddependencies=true

using Amazon;
using Amazon.CloudFormation;
using Amazon.Runtime.CredentialManagement;

// The aim - make a file that can bootstrap my aws stack

// So I want to ensure credentials
AWSCredentials remoteAwsCredentials = null;

var profileName = Argument("profile", "joshua");

Task("Get-AWS")
    .Description("Get AWS Credentials")
    .Does(async () =>
    {
        GetIdentityManagementClient();
    });

IAmazonIdentityManagementService GetIdentityManagementClient()
{
    EnsureCredentials();
    return new IAmazonIdentityManagementServiceClient(remoteAwsCredentials, "eu-west-2");
}

void EnsureCredentials()
{
    // If they are set already - don't need to get them
    if (remoteAwsCredentials != null) return;

    var credentialStore = new CredentialProfileStoreChain();
    if (!credentialStore.TryGetProfile(profileName, out var profile))
        throw new Exception($"Could not find AWS profile: {profileName}");

    remoteAwsCredentials = AWSCredentialsFactory.GetAWSCredentials(profile, credentialStore);

    var assumeRoleCredentials = remoteAwsCredentials as AssumeRoleAWSCredentials;

    if (assumeRoleCredentials != null)
    {
        assumeRoleCredentials.Options.MfaTokenCallback = () => {
            Console.WriteLine($"Enter MFA code for {profileName}");
        };
    }
}