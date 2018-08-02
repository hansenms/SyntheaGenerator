Automated Synthea Generator and FHIR Uploader
---------------------------------------------

This repository builds a Docker image with a Synthea patient generator and a FHIR uploader that can be used with FHIR servers using Azure Active Directory as the OAuth2 provider. Specifically, when started, the Docker container will:

1. Generate the desired number of patients.
2. Authenticate with Azure Active Directory to obtain a token.
3. Upload patients to FHIR server.  

The Azure Active Directory authentication and FHIR server upload is handled by the [`FhirAADUploader`](FhirAADUploader) app, which is a .NET Core command line application.

