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

        static void Main(string[] args) {
            if (args.Length < 2) {
                ShowUsageInfo();
                return;
            }

            string inputHashesPath = args[0];
            string inputStringsPath = args[1];

            string funcType = "StrCode32";

            if (args.Count() > 3) {
                if (args[2].ToLower() == "-hashfunction") {
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

            Dictionary<string, HashFunction> hashFuncs = new Dictionary<string, HashFunction>();
            hashFuncs["StrCode32"] = StrCode32Str;
            hashFuncs["PathFileNameCode32"] = PathFileNameCode32Str;
            hashFuncs["PathFileNameCode64"] = PathFileNameCode64Str;//tex for want of a better name

            HashFunction HashFunc;
            try {
                HashFunc = hashFuncs[funcType];
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

            List<string> inputStrings = dictionary.SelectMany(d => d.Value).ToList(); //tex could save off initial file RealAllLines in BuildDictionary but would still have to uniquify it.
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
                            Console.WriteLine("Hash " + hash + " was already in matchedHashes");//DEBUGNOW
                        }
                        foreach (string str in stringsForHash) {
                            matchedStrings.Add(str);
                        }
                    }
                } else {
                    //Console.WriteLine("No string found for hash " + hash);//DEBUG
                    bool isNew = unmatchedHashes.Add(hash);
                    if (!isNew) {
                        Console.WriteLine("Hash " + hash + " was already in unmatchedHashes");//DEBUGNOW
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

            Console.WriteLine("unmatchedHashes    [" + unmatchedHashes.Count.ToString().PadLeft(numHashPlaces) + "/" + numInputHashes + "]");
            Console.WriteLine("matchedHashes      [" + matchedHashes.Count.ToString().PadLeft(numHashPlaces) + "/" + numInputHashes + "]");
            Console.WriteLine("collsionHashes     [" + collisionHashes.Count.ToString().PadLeft(numHashPlaces) + "/" + numInputHashes + "]");
            Console.WriteLine("unmatchedStrings   [" + unmatchedStrings.Count.ToString().PadLeft(numStringPlaces) + "/" + numInputStrings + "]");
            Console.WriteLine("matchedStrings     [" + matchedStrings.Count.ToString().PadLeft(numStringPlaces) + "/" + numInputStrings + "]");

            Console.WriteLine("Writing out files");

            var unmatchedHashesList = unmatchedHashes.ToList<string>();
            unmatchedHashesList.Sort();
            var unmatchedStringsList = unmatchedStrings.ToList<string>();
            unmatchedStringsList.Sort();

            var matchedHashesList = matchedHashes.ToList<string>();
            matchedHashesList.Sort();
            var matchedStringsList = matchedStrings.ToList<string>();
            matchedStringsList.Sort();

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

            //tex TODO: what's the best name
            File.WriteAllLines(stringsPath + "_HashStringsCollisions.txt", hashStringsCollisions.ToArray());
            File.WriteAllLines(stringsPath + "_HashStringMatches.txt", hashStringMatches.ToArray());

            File.WriteAllLines(hashesPath + "_unmatchedHashes.txt", unmatchedHashesList.ToArray());
            File.WriteAllLines(hashesPath + "_matchedHashes.txt", matchedHashesList.ToArray());

            File.WriteAllLines(stringsPath + "_unmatchedStrings.txt", unmatchedStringsList.ToArray());
            File.WriteAllLines(stringsPath + "_matchedStrings.txt", matchedStringsList.ToArray());


            Console.WriteLine("All done");//DEBUGNOW
            //Console.ReadKey();//DEBUGNOW
        }

        static void ShowUsageInfo() {
            Console.WriteLine("HashWrangler by tinmantex\n" +
                              "  For comparing lists of hashes against lists of strings and outputting found and unfound lists.\n" +
                              "Usage:\n" +
                              "  HashWrangler <hashes file path> <strings file path> [-HashFunction <hash function name>]\n" +
                              "  HashFunction defaults to StrCode32, others are PathFileCode32, PathFileNameCode64 - path file name no extension hash seen in GzsTool dictionary\n"
                              );//DEBUGNOW
        }

        private static Dictionary<string, HashSet<string>> BuildDictionary(string path, HashFunction HashFunc) {
            var dictionary = new Dictionary<string, HashSet<string>>();

            if (Directory.Exists(path)) {
                string[] files = Directory.GetFiles(path,"*.txt");
                foreach (string filePath in files) {
                    ReadDictionary(filePath, HashFunc, ref dictionary);
                }
            } else {
                ReadDictionary(path, HashFunc, ref dictionary);
            }

            return dictionary;
        }

        private static void ReadDictionary(string path, HashFunction HashFunc, ref Dictionary<string, HashSet<string>> dictionary) {
            Console.WriteLine("ReadDictionary " + path);
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
                        Console.WriteLine("string " + line + " already exists for hash " + hash);//DEBUGNOW
                    } else {
                        string strings = String.Join(" | ", stringsForHash.ToArray());
                        Console.WriteLine("hash collision for " + hash + ": " + strings);//DEBUGNOW
                    }
                } else {
                    stringsForHash = new HashSet<string>();
                    stringsForHash.Add(line);
                    dictionary.Add(hash, stringsForHash);
                }
            }
        }

        private static List<string> GetInputHashes(string path) {
            var inputHashes = new HashSet<string>();
            if (Directory.Exists(path)) {
                string[] files = Directory.GetFiles(path, "*.txt");
                foreach (string filePath in files) {
                    ReadInputHashes(filePath, ref inputHashes);
                }
            } else {
                ReadInputHashes(path, ref inputHashes);
            }
            return inputHashes.ToList();
        }

        private static void ReadInputHashes(string path, ref HashSet<string> inputHashes) {
            Console.WriteLine("ReadInputHashes " + path);
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
                Console.WriteLine("There is " + duplicates + " duplicates in " + path);//DEBUGNOW
            }
        }

        public static string StrCode32Str(string text) {
            var hash = (uint)Hashing.HashFileNameLegacy(text);
            return hash.ToString();
        }
        //TODO: verify output matches lua PathFileNameCode32
        public static string PathFileNameCode32Str(string text) {
            var hash = (uint)Hashing.HashFileNameWithExtension(text);
            return hash.ToString();
        }
        //tex DEBUGNOW TODO name, this is more specific to gzstool dictionary implementation than a general Fox implementation?
        public static string PathFileNameCode64Str(string text) {
            ulong hash = Hashing.HashFileName(text) & 0x3FFFFFFFFFFFF;
            return hash.ToString("x");
        }
    }
}
