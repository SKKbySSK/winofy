#!/bin/bash
IN="../Winofy.Device.csproj"
OUT="builds/"

dotnet publish $IN --self-contained -r linux-arm -c release -o $OUT/linux-arm/
dotnet publish $IN --self-contained -r win-x64 -c release -o $OUT/win-x64/
dotnet publish $IN --self-contained -r osx-x64 -c release -o $OUT/osx-x64/
dotnet publish $IN --self-contained -r linux-x64 -c release -o $OUT/linux-x64/

OUT="."
rm $OUT/linux-arm.zip
zip $OUT/linux-arm.zip -r $OUT/linux-arm/

rm $OUT/win-x64.zip
zip $OUT/win-x64.zip -r $OUT/win-x64/

rm $OUT/osx-x64.zip
zip $OUT/osx-x64.zip -r $OUT/osx-x64/

rm $OUT/linux-x64.zip
zip $OUT/linux-x64.zip -r $OUT/linux-x64/