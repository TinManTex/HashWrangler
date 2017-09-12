using HashWrangler.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashWrangler {
    class Program {
        delegate string HashFunction(string str);

        //SYNC with whatever you output
        static string[] outputSuffixes = {
            "_HashStringsCollisions",
            "_HashStringMatches",
            "_unmatchedHashes", 
            "_matchedHashes",
            "_unmatchedStrings",
            "_matchedStrings"
        };

        static void Main(string[] args) {
            if (args.Length == 0) {
                ShowUsageInfo();
                return;
            }

            string inputHashesPath = args[0];
            string inputStringsPath = null;
            if (args.Count() > 1)
            {
                inputStringsPath = args[1];
            }

            //tex hashwrangler <strings path>
            bool buildHashesForDict = false;
            if (inputStringsPath==null)
            {
                buildHashesForDict = true;
                inputStringsPath = inputHashesPath;
            }

            string funcType = "StrCode32";

            if (args.Count() > 3) {
                if (args[2].ToLower() == "-hashfunction" || args[2].ToLower() == "-h") {
                    funcType = args[3];
                }
            }

            if (!Directory.Exists(inputHashesPath) && File.Exists(inputHashesPath) == false) {
                Console.WriteLine("Could not find " + inputHashesPath);
                return;
            }

            if (!Directory.Exists(inputStringsPath) && File.Exists(inputStringsPath) == false) {
                Console.WriteLine("Could not find " + inputStringsPath);
                return;
            }

            if (!Path.IsPathRooted(inputHashesPath)) {
                inputHashesPath = Path.GetFullPath(inputHashesPath);
            }
            if (!Path.IsPathRooted(inputStringsPath)) {
                inputStringsPath = Path.GetFullPath(inputStringsPath);
            }

            Dictionary<string, HashFunction> hashFuncs = new Dictionary<string, HashFunction>();
            hashFuncs["strcode32"] = StrCode32Str;
            hashFuncs["strcode64"] = StrCode64Str;
            hashFuncs["pathfilenamecode32"] = PathFileNameCode32Str;
            hashFuncs["pathfilenamecode64"] = PathFileNameCode64Str;//tex for want of a better name
            hashFuncs["pathcode64"] = PathCode64Str;
            hashFuncs["pathcode64gz"] = PathCode64GzStr;

            HashFunction HashFunc;
            try {
                HashFunc = hashFuncs[funcType.ToLower()];
            } catch (KeyNotFoundException) {
                HashFunc = StrCode32Str;
                Console.WriteLine("ERROR: Could not find hash function " + funcType);
                return;
            }

            Console.WriteLine("Using HashFunction " + funcType);

            //<hash as string, list of strings for hash> // tex due to collisions need to keep track of multiple strings for hash
            Dictionary<string, HashSet<string>> dictionary = BuildDictionary(inputStringsPath,HashFunc);
            if (dictionary==null) {
                return;
            }

            List<string> inputStrings = dictionary.SelectMany(d => d.Value).ToList(); //tex could save off initial file ReadAllLines in BuildDictionary but would still have to uniquify it.
            inputStrings.Sort();
            //
            if (buildHashesForDict)
            {
                Console.WriteLine("buildHashesForDict");
                foreach (string funcName in hashFuncs.Keys)
                {
                    HashFunc = hashFuncs[funcName];
                    var hashesForInputStrings = new List<string>();
                    foreach (string str in inputStrings)
                    {
                        var hash = HashFunc(str).ToString();
                        hashesForInputStrings.Add(str + " " + hash);
                    }
                    string hashesForInputStringsPath = "";
                    if (File.Exists(inputStringsPath))
                    {
                        hashesForInputStringsPath = Path.GetDirectoryName(inputStringsPath) + "\\" + Path.GetFileNameWithoutExtension(inputStringsPath);
                    } else
                    {
                        hashesForInputStringsPath = Path.Combine(inputStringsPath, "..") + "\\" + new DirectoryInfo(inputStringsPath).Name + "Strings";
                    }

                    File.WriteAllLines(hashesForInputStringsPath + "_" + funcName + "HashStringMatches.txt", hashesForInputStrings.ToArray());
                }
                return;
            }


            List<string> inputHashes = GetInputHashes(inputHashesPath);
            if (inputHashes == null) {
                return;
            }

            if (dictionary==null || inputHashes==null) {
                return;
            }

            var matchedHashes = new HashSet<string>();
            var unmatchedHashes = new HashSet<string>();

            var collisionHashes = new Dictionary<string, HashSet<string>>();

            var matchedStrings = new HashSet<string>();
            var unmatchedStrings = new HashSet<string>();
            var collisionStrings = new HashSet<string>();

            Console.WriteLine("Finding strings for hashes");
            int doneCount = 0;
            int numInputHashes = inputHashes.Count;
            int numInputStrings = inputStrings.Count;
            foreach (string hash in inputHashes) {
                doneCount++;
                //Console.WriteLine("[" + doneCount + "/" + numInputHashes + "]");//DEBUG

                HashSet<string> stringsForHash;
                if (dictionary.TryGetValue(hash, out stringsForHash)) {
                    //Console.WriteLine("Found string for hash " + hash);//DEBUG
                    //tex collisions have already been gathered by BuildDictionary by this point, 
                    //but we only want to output collisions for the input hashes.
                    if (stringsForHash.Count > 1) {
                        collisionHashes[hash]=stringsForHash;
                        foreach (string str in stringsForHash) {
                            collisionStrings.Add(str);
                        }
                    } else {
                        bool isNew = matchedHashes.Add(hash);
                        if (!isNew) {
                            //Console.WriteLine("Hash " + hash + " was already in matchedHashes");
                        }
                        foreach (string str in stringsForHash) {
                            matchedStrings.Add(str);
                        }
                    }
                } else {
                    //Console.WriteLine("No string found for hash " + hash);//DEBUG
                    bool isNew = unmatchedHashes.Add(hash);
                    if (!isNew) {
                        //Console.WriteLine("Hash " + hash + " was already in unmatchedHashes");//DEBUGNOW
                    }
                }
            }

            
             foreach (string str in inputStrings) {
                if (!matchedStrings.Contains(str) && !collisionStrings.Contains(str)) {
                    unmatchedStrings.Add(str);
                }
            }

            Console.WriteLine("Stats:");
            int numHashPlaces = numInputHashes.ToString().Length;
            int numStringPlaces = numInputHashes.ToString().Length;

            float hashPerMult = numInputHashes;
            float stringPerMult = 0;

            float unmatchedHashesPercent = 0;
            float matchedHashesPercent = 0;
            float collisionHashesPercent = 0;
            float unmatchedStringsPercent = 0;
            float matchedStringsPercent = 0;

            if (numInputHashes > 0) {
                hashPerMult = 100.0f / (float)numInputHashes;
                unmatchedHashesPercent = unmatchedHashes.Count * hashPerMult;
                matchedHashesPercent = matchedHashes.Count * hashPerMult;
                collisionHashesPercent = collisionHashes.Count * hashPerMult;
            }
            if (numInputStrings > 0) {
                stringPerMult = 100.0f / (float)numInputStrings;
                unmatchedStringsPercent = unmatchedStrings.Count * stringPerMult;
                matchedStringsPercent = matchedStrings.Count * stringPerMult;
            }

            Console.WriteLine("unmatchedHashes    [" + unmatchedHashes.Count.ToString().PadLeft(numHashPlaces) + "/" + numInputHashes + "] - " + unmatchedHashesPercent + "%");
            Console.WriteLine("matchedHashes      [" + matchedHashes.Count.ToString().PadLeft(numHashPlaces) + "/" + numInputHashes + "] - " + matchedHashesPercent + "%");
            Console.WriteLine("collsionHashes     [" + collisionHashes.Count.ToString().PadLeft(numHashPlaces) + "/" + numInputHashes + "] - " + collisionHashesPercent + "%");
            Console.WriteLine("unmatchedStrings   [" + unmatchedStrings.Count.ToString().PadLeft(numStringPlaces) + "/" + numInputStrings + "] - " + unmatchedStringsPercent + "%");
            Console.WriteLine("matchedStrings     [" + matchedStrings.Count.ToString().PadLeft(numStringPlaces) + "/" + numInputStrings + "] - " + matchedStringsPercent + "%");

            Console.WriteLine("Writing out files");

            var unmatchedHashesList = unmatchedHashes.OrderBy(s => s, StringComparer.InvariantCultureIgnoreCase).ToArray<string>();
            var unmatchedStringsList = unmatchedStrings.OrderBy(s => s, StringComparer.InvariantCultureIgnoreCase).ToArray<string>();
            var matchedHashesList = matchedHashes.OrderBy(s => s, StringComparer.InvariantCultureIgnoreCase).ToArray<string>();
            var matchedStringsList = matchedStrings.OrderBy(s => s, StringComparer.InvariantCultureIgnoreCase).ToArray<string>();

            var hashStringsCollisions = new List<string>();
            foreach (var item in collisionHashes) {
                string line = item.Key + " " + String.Join("||", item.Value.ToArray());
                hashStringsCollisions.Add(line);
            }

            //tex using matchedStrings so its sorted
            var hashStringMatches = new List<string>();
            foreach (var str in matchedStringsList) {
                //tex killing some performance here, but don't have a string>hash lookup set up.
                var hash = HashFunc(str).ToString();
                string line = hash.PadLeft(13) + " " + str;
                hashStringMatches.Add(line);
            }

            //tex output files will be 
            //for single file input:
            //<input path>\<filename>_<somesuffix>.txt
            //for directory input:
            //<parent of input path>\<foldername><Hashes | Strings>_<somesuffix>.txt
            string hashesPath = "";
            if (File.Exists(inputHashesPath)) {
                hashesPath = Path.GetDirectoryName(inputHashesPath) + "\\" + Path.GetFileNameWithoutExtension(inputHashesPath);
            } else {
                string parent = Directory.GetParent(inputHashesPath).FullName;
                hashesPath = Path.Combine(inputHashesPath,"..") + "\\" + new DirectoryInfo(inputHashesPath).Name + "Hashes";
            }
            string stringsPath = "";
            if (File.Exists(inputStringsPath)) {
                stringsPath = Path.GetDirectoryName(inputStringsPath) + "\\" + Path.GetFileNameWithoutExtension(inputStringsPath);
            } else {
                stringsPath = Path.Combine(inputStringsPath, "..") + "\\" + new DirectoryInfo(inputStringsPath).Name + "Strings";
            }

            //tex delete since we might not be overwriting and old one could cause confusion
            if (File.Exists(stringsPath + "_HashStringsCollisions.txt")) {
                File.Delete(stringsPath + "_HashStringsCollisions.txt");
            }
            if (hashStringsCollisions.Count > 0) {
                File.WriteAllLines(stringsPath + "_HashStringsCollisions.txt", hashStringsCollisions.ToArray());
            }
            File.WriteAllLines(stringsPath + "_HashStringMatches.txt", hashStringMatches.ToArray());

            File.WriteAllLines(hashesPath + "_unmatchedHashes.txt", unmatchedHashesList);
            File.WriteAllLines(hashesPath + "_matchedHashes.txt", matchedHashesList);

            File.WriteAllLines(stringsPath + "_unmatchedStrings.txt", unmatchedStringsList);
            File.WriteAllLines(stringsPath + "_matchedStrings.txt", matchedStringsList);


            Console.WriteLine("All done");
        }

        static void ShowUsageInfo() {
            Console.WriteLine("HashWrangler by tinmantex\n" +
                              "  For comparing lists of hashes against lists of strings and outputting found and unfound lists.\n" +
                              "Usage:\n" +
                              "  HashWrangler <hashes file path> <strings file path> [-HashFunction <hash function name>]\n" +
                              "  HashFunction defaults to StrCode32, others are StrCode64, PathCode64, PathCode64Gz, PathFileNameCode64, PathFileNameCode32.\n" +
                              "  Options are case insensitive\n" +
                              "  or\n" +
                              " HashWrangler <dictionary file path>\n" +
                              "   outputs <dictionary>_<hash func name>HashMatches.txt for each hash function on the input dictionary."
                              );
        }

        private static Dictionary<string, HashSet<string>> BuildDictionary(string path, HashFunction HashFunc) {
            var dictionary = new Dictionary<string, HashSet<string>>();

            string[] files = null;
            if (File.Exists(path)) {
                files = new string[] { path };
            }
            if (Directory.Exists(path)) {
                files = Directory.GetFiles(path, "*.txt");
            }

            if (files!=null) {
                foreach (string filePath in files) {
                    ReadDictionary(filePath, HashFunc, ref dictionary);
                }
            }

            return dictionary;
        }

        private static void ReadDictionary(string path, HashFunction HashFunc, ref Dictionary<string, HashSet<string>> dictionary) {
            Console.WriteLine("ReadDictionary " + path);
            if (!File.Exists(path)) {
                Console.WriteLine("Could not find " + path);
                return;
            }

            int dups = 0;

            string[] lines;
            try {
                lines = File.ReadAllLines(path);
            } catch (Exception e) {
                Console.WriteLine("Unable to read the dictionary " + path + " " + e.Message);
                return;
            }

            foreach (string line in lines) {
                var hash = HashFunc(line);

                HashSet<string> stringsForHash;
                if (dictionary.TryGetValue(hash, out stringsForHash)) {
                    bool isNew = stringsForHash.Add(line);
                    if (!isNew) {
                        //Console.WriteLine("string " + line + " already exists for hash " + hash);//DEBUGNOW
                        dups++;
                    } else {
                        string strings = String.Join(" | ", stringsForHash.ToArray());
                        //Console.WriteLine("hash collision for " + hash + ": " + strings);//DEBUGNOW
                    }
                } else {
                    stringsForHash = new HashSet<string>();
                    stringsForHash.Add(line);
                    dictionary.Add(hash, stringsForHash);
                }
            }
            if (dups > 0) {
                Console.WriteLine(dups + " strings from " + Path.GetFileName(path) + " were already in dictionary");//DEBUGNOW
            }
        }

        private static List<string> GetInputHashes(string path) {
            var inputHashes = new HashSet<string>();

            string[] files = null;
            if (File.Exists(path)) {
                files = new string[] { path };
            }
            if (Directory.Exists(path)) {
                files = Directory.GetFiles(path, "*.txt");
            }

            if (files != null) {
                foreach (string filePath in files) {
                    ReadInputHashes(filePath, ref inputHashes);
                }
            }

            return inputHashes.ToList();
        }

        private static void ReadInputHashes(string path, ref HashSet<string> inputHashes) {
            Console.WriteLine("ReadInputHashes " + path);
            if (!File.Exists(path)) {
                Console.WriteLine("Could not find " + path);
                return;
            }

            int duplicates = 0;

            string[] lines;
            try {
                lines = File.ReadAllLines(path);
            } catch (Exception e) {
                Console.WriteLine("Unable to read inputHashes " + path + " " + e);
                return;
            }

            foreach (var line in lines) {
                var str32 = line;
                bool isNew = inputHashes.Add(str32);
                if (!isNew) {
                    duplicates++;
                }
            }

            if (duplicates > 0) {
                Console.WriteLine("There is " + duplicates + " duplicates in " + path);
            }
        }

        public static string StrCode32Str(string text) {
            var hash = (uint)Hashing.HashFileNameLegacy(text);
            return hash.ToString();
        }
        public static string StrCode64Str(string text) {
            var hash = Hashing.HashFileNameLegacy(text);
            return hash.ToString();
        }
        //TODO: verify output matches lua PathFileNameCode32 (it was missing in some cases? see mockfox pathfilename note?)
        public static string PathFileNameCode32Str(string text) {
            var hash = (uint)Hashing.HashFileNameWithExtension(text);
            return hash.ToString();
        }
        public static string PathFileNameCode64Str(string text) {
            ulong hash = Hashing.HashFileNameWithExtension(text);
            return hash.ToString();
        }
        //tex DEBUGNOW TODO name, this is more specific to gzstool dictionary implementation than a general Fox implementation?
        public static string PathCode64Str(string text) {
            ulong hash = Hashing.HashFileName(text) & 0x3FFFFFFFFFFFF;
            return hash.ToString("x");
        }

        public static string PathCode64GzStr(string text) {
            ulong hash = Hashing.HashFileNameLegacy(text);
            return hash.ToString("x");
        }
    }
}
