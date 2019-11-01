# Saekano-Bin-Depacker
C# Depacker for Saekano VN Vita Game

## Info
This program was primarily based on the BMS script found for the game and in terms of depacking should perform very similarly to that script, although the end directory trees are slightly different because of how some cascading files are handled. This means this file structure can't be used for reinsertion through BMS as the final archive might be structured a little differently.

Note: This program was made to make sure that I had an understanding of the GPDA .bin file structures in preparation for desiging a repacker. It was not meant to replace the BMS unpacking script

## Usage
Drag and drop the .bin file with the GPDA header onto the exe and it will begin depacking and decompressing the files. Alternatively you can run the exe through the command line and specify the file as the first arguement (all others are ignored)

Example:
`"Saekano Depack.exe" <filename>`