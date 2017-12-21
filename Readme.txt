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
Options are case insensitive

Output:
<hashes>_matchedHashes.txt - hashes that matched a string
<hashes>_unmatchedHashes.txt - hashes that didn't match any strings

<strings>_matchedStrings.txt - strings that matched a hash, can be considered the validated dictionary for the given input hashes.

<strings>_HashStringMatches.txt - hash string pairs, useful as a manual human lookup.

<strings>_HashStringsCollisions.txt - hash strings pairs, input strings that resolved to the same input hash.

Files with output suffixes in their names will not be read as input.

See Wrangling Hashes.txt in https://github.com/TinManTex/mgsv-lookup-strings or http://metalgearmodding.wikia.com/wiki/Hash_Wrangling for more info.

Alternate usage:
HashWrangler <dictionary file path>

outputs <dictionary>_<hash func name>HashStringList.txt for each hash function on the input dictionary.