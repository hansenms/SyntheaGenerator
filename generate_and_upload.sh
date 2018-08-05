#!/bin/bash

cd /app/synthea
./run_synthea -p $NUMBER_OF_PATIENTS

cd /app
dotnet run /app/synthea/output/fhir $FHIR_SERVER_URL

