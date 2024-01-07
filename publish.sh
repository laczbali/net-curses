cd NetCurses

# increment version
version=$(grep -oPm1 "(?<=<VersionPrefix>)[^<]+" NetCurses.csproj)

version=$(echo $version | awk -F. '{$NF = $NF + 1;} 1' | sed 's/ /./g')
sed -i "s/<VersionPrefix>.*<\/VersionPrefix>/<VersionPrefix>$version<\/VersionPrefix>/" NetCurses.csproj

# build and publish
dotnet pack -c Release -o ./nuget -p:PackageID=Blaczko.NetCurses
dotnet nuget push ./nuget/Blaczko.NetCurses.$version.nupkg -k $NUGET_KEY -s https://api.nuget.org/v3/index.json
