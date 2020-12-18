# WarThunderReplay

This application will separate and parse war thunder packets contained within replay files. 

# Usage

### Windows
    WarThunderReplay.exe ReplayFileName
This will parse the `ReplayFileName` replay

### Unix 
    dotnet WarThunderReplay.dll ReplayFileName
This will parse the `ReplayFileName` replay


# Requirements

DotNet 5.0 - DotNet Core 3.1 may work if compiled under 3.1
WT Tools by klensy - to decompress the replay files from wprl to wrplu files

