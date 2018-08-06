Automated Synthea Generator and FHIR Uploader
---------------------------------------------

This repository builds a Docker image with a Synthea patient generator and a FHIR uploader that can be used with FHIR servers using Azure Active Directory as the OAuth2 provider. Specifically, when started, the Docker container will:

1. Generate the desired number of patients.
2. Authenticate with Azure Active Directory to obtain a token.
3. Upload patients to FHIR server.  

The Azure Active Directory authentication and FHIR server upload is handled by the [`FhirAADUploader`](FhirAADUploader) app, which is a .NET Core command line application.

To generate 100 patients and upload them to a FHIR server with URL `https://my-fhir-server.com`:

```
docker run --name synthea --rm -t -e AzureAD_ClientSecret='AAD-CLIENT-SECRET' -e AzureAD_ClientId='AAD-CLIENT-ID' -e AzureAD_Authority='https
://login.microsoftonline.com/TENANT-ID/' -e AzureAD_Audience='AAD-FHIR-API-APP-ID' -e NUMBER_OF_PATIENTS='100' -e FHIR_SERVER_URL='https://my-fhir-server/com/' hansenms/s
ynthegenerator
```

To build your own version of the Docker image:

```
docker build -t yourrepo/yourtagname .
```

You can use an Azure Container Instance to generate and upload the patients:

<a href="https://transmogrify.azurewebsites.net/azuredeploy.json" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a>
