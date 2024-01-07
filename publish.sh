cd NetCurses

# increment version
version=$(grep -oPm1 "(?<=<VersionPrefix>)[^<]+" NetCurses.csproj)

version=$(echo $version | awk -F. '{$NF = $NF + 1;} 1' | sed 's/ /./g')
sed -i "s/<VersionPrefix>.*<\/VersionPrefix>/<VersionPrefix>$version<\/VersionPrefix>/" NetCurses.csproj

git add NetCurses.csproj
git commit -m "Bump version to $version"
git push

# build and publish
dotnet pack -c Release -o ./nuget
dotnet nuget push ./nuget/NetCurses.$version.nupkg -k $NUGET_KEY -s https://api.nuget.org/v3/index.json
