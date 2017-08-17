HashWrangler
A tool for testing Fox Engine lists of hashes against lists of strings and outputing matches.

Usage:
HashWrangler <hashes file path> <strings file path> -HashFunction <hash function type>

Options:
-HashFunction <hash function type>
or
-h <hash function type>
Function types - defaults to StrCode32
StrCode32 or str32
PathFileNameCode64
PathFileNameCode32
PathCode64
PathCode64Gz
Options are case insensitive

Output:
<hashes>_matchedHashes.txt - hashes that matched a string
<hashes>_unmatchedHashes.txt - hashes that didn't match any strings

<strings>_matchedStrings.txt - strings that matched a hash, can be considered the validated dictionary for the given input hashes.
<strings>_matchedStrings.txt - strings that didn't match any hash

_matchedHashes and _matchedStrings are output in paired order, so for a given line in one file it will match in the other.

<strings>_HashStringMatches.txt - hash string pairs, useful as a manual human lookup.

<strings>_HashStringsCollisions.txt - hash strings pairs, input strings that resolved to the same input hash.

Files with output suffixes in their names will not be read as input.

See Wrangling Hashes.txt in https://github.com/TinManTex/mgsv-lookup-strings or http://metalgearmodding.wikia.com/wiki/Hash_Wrangling for more info.