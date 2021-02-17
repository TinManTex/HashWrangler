HashWrangler
A tool for testing Fox Engine lists of hashes against lists of strings and outputing matches.

Usage:
HashWrangler <hashes file path> <strings file path> -HashFunction <hash function type>

Options:
-HashFunction <hash function type>
or
-h <hash function type>
Function types - defaults to StrCode32
StrCode32
StrCode64
PathFileNameCode64
PathFileNameCode32
PathCode64
PathCode64Gz
ExtensionCode64
Md5HashText
Options are case insensitive

Usage json config:
Run HashWrangler without any command line args for it to write default-config.json

HashWrangler <config file path>.json

Config options:
"inputHashesPath": "", -- hashes to test
"inputStringsPath": "", -- strings to test
"funcType": "StrCode32", -- function type, as above
"tryVariations": false, -- adds upper case,lower case and capatalized versions of input strings. Will generally cause a lot of collisions for depending on input strings and hash function type.
"tryExtensions": false,-- adds file extensions that appear in qar files to input strings
"hashesToHex": false, -- converts input hashes to hex representation
"validateMode": false, -- for validating using mgsv-lookup-strings repo layout
"validateRoot": "", -- path to hash types.json for format, should be at root
"matchedStringsNameIsDictionary": false -- Should only be used when input strings are a folder, else it will overwrite the input strings file.
By default matched strings will be written to <inputStringsPath>Strings_matchedStrings.txt (if input strings are a folder), 
when set to true matched strings will be written to <inputStringsPath>.txt , which saves hasle of having to rename stuff if your workflow is for updating the mgsv-strings github repo

validate hash types .json example
{
"DataSet": "PathFileNameCode32",
"LocatorName": "StrCode32"
>
}
Entries are:
<dictionary name to create> : <func type>

Output:
<hashes>_matchedHashes.txt - hashes that matched a string
<hashes>_unmatchedHashes.txt - hashes that didn't match any strings

<strings>_matchedStrings.txt - strings that matched a hash, can be considered the validated dictionary for the given input hashes.

<strings>_HashStringMatches.txt - hash string pairs, useful as a manual human lookup.

<strings>_HashStringsCollisions.txt - hash strings pairs, input strings that resolved to the same input hash.

ValidateStats.txt - only with validatemode

Files with output suffixes in their names will not be read as input.

Hashes are mostly in Uint32 or Uint64 depending on the hash function. PathCode64/Gz is big-endian hex. 

See Wrangling Hashes.txt in https://github.com/TinManTex/mgsv-lookup-strings or http://metalgearmodding.wikia.com/wiki/Hash_Wrangling for more info.

Alternate usage:
HashWrangler <dictionary file path>

outputs <dictionary>_<hash func name>HashStringList.txt for each hash function on the input dictionary.