#!/bin/bash
IN="../Winofy.HomeConsole.csproj"
OUT="builds/"

dotnet publish $IN --self-contained -r linux-arm -c release -o $OUT/linux-arm/

OUT="."
rm $OUT/linux-arm.zip
zip $OUT/linux-arm.zip -r $OUT/linux-arm/
