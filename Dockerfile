FROM microsoft/dotnet:2.1-sdk

WORKDIR /app

#Installing JAVA and Synthea
RUN apt-get update && apt-get install -y software-properties-common python3-software-properties
RUN echo debconf shared/accepted-oracle-license-v1-1 select true | debconf-set-selections
RUN echo debconf shared/accepted-oracle-license-v1-1 seen true | debconf-set-selections
RUN echo "deb http://ppa.launchpad.net/webupd8team/java/ubuntu xenial main" | tee /etc/apt/sources.list.d/webupd8team-java.list
RUN echo "deb-src http://ppa.launchpad.net/webupd8team/java/ubuntu xenial main" | tee -a /etc/apt/sources.list.d/webupd8team-java.list
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys EEA14886
RUN apt-get update
RUN apt-get install -yq openjdk-8-jdk

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
