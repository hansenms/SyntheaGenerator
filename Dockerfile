FROM microsoft/dotnet:2.1-sdk

WORKDIR /app

#Installing JAVA and Synthea
RUN apt-get update && apt-get install -y software-properties-common python3-software-properties
RUN echo debconf shared/accepted-oracle-license-v1-1 select true | debconf-set-selections
RUN echo debconf shared/accepted-oracle-license-v1-1 seen true | debconf-set-selections
RUN add-apt-repository -y ppa:webupd8team/java
RUN apt-get update
RUN apt-get install -yq --allow-unauthenticated oracle-java8-installer git

RUN git clone https://github.com/synthetichealth/synthea.git && \
    cd synthea && \
    ./gradlew build check test

# copy csproj and restore as distinct layers
COPY FhirAADUploader/*.csproj .
RUN dotnet restore

# copy everything else and build app
COPY FhirAADUploader ./
RUN dotnet publish -c release

COPY generate_and_upload.sh ./
RUN chmod +x generate_and_upload.sh
ENTRYPOINT ["bash", "-c", "./generate_and_upload.sh"]