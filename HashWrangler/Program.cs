using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utility;
using static Utility.Hashing;

namespace HashWrangler
{
    class Program
    {
        class RunSettings
        {
            public string inputHashesPath = null;
            public string inputStringsPath = null;
            public string funcType = "StrCode32";

            public bool tryVariations = false;//WIP DEBUGNOW trouble is this fires off alot more collisions for StrCode32 depending on input
            public bool tryExtensions = false;
            public bool hashesToHex = false;
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowUsageInfo();
                return;
            }

            RunSettings run = new RunSettings();

            run.inputHashesPath = args[0];
            run.inputStringsPath = null;
            if (args.Count() > 1)
            {
                run.inputStringsPath = args[1];
            }

            if (args.Count() > 3)
            {
                if (args[2].ToLower() == "-hashfunction" || args[2].ToLower() == "-h")
                {
                    run.funcType = args[3];
                }
            }


            //tex hashwrangler <strings path>
            if (run.inputStringsPath == null)
            {
                run.inputStringsPath = run.inputHashesPath;

                Console.WriteLine("Building hashes for strings:");
                List<string> stringsFilesPaths = GetFileList(run.inputStringsPath);
                if (stringsFilesPaths.Count == 0)
                {
                    Console.WriteLine($"Could not find any input strings in {run.inputStringsPath}");
                    return;
                }
                BuildHashesForDict(stringsFilesPaths);
                return;
            }


            if (!Directory.Exists(run.inputHashesPath) && File.Exists(run.inputHashesPath) == false)
            {
                Console.WriteLine("Could not find " + run.inputHashesPath);
                return;
            }

            if (!Directory.Exists(run.inputStringsPath) && File.Exists(run.inputStringsPath) == false)
            {
                Console.WriteLine("Could not find " + run.inputStringsPath);
                return;
            }

            if (!Path.IsPathRooted(run.inputHashesPath))
            {
                run.inputHashesPath = Path.GetFullPath(run.inputHashesPath);
            }
            run.inputHashesPath = Regex.Replace(run.inputHashesPath, @"\\", "/");


            if (!Path.IsPathRooted(run.inputStringsPath))
            {
                run.inputStringsPath = Path.GetFullPath(run.inputStringsPath);
            }
            run.inputStringsPath = Regex.Replace(run.inputStringsPath, @"\\", "/");

            HashFunction HashFunc;
            try
            {
                HashFunc = hashFuncs[run.funcType.ToLower()];
            } catch (KeyNotFoundException)
            {
                HashFunc = StrCode32Str;
                Console.WriteLine("ERROR: Could not find hash function " + run.funcType);
                return;
            }

            Console.WriteLine("Reading input hashes:");
            List<string> inputHashesList = GetInputHashes(run.inputHashesPath, run.hashesToHex);
            if (inputHashesList == null)
            {
                Console.WriteLine($"Could not find any input hashes in {run.inputHashesPath}");
                return;
            }

            Console.WriteLine("Building strings file list");
            List<string> inputStringsFiles = GetFileList(run.inputStringsPath);
            if (inputStringsFiles.Count == 0)
            {
                Console.WriteLine($"Could not find any input strings in {run.inputStringsPath}");
                return;
            }

            Console.WriteLine($"Testing strings using HashFunction {run.funcType}:");
            var hashMatches = TestStrings(run, HashFunc, inputHashesList, inputStringsFiles);

            Console.WriteLine("Building output");
            BuildOutput(run, hashMatches);

            Console.WriteLine("All done");
        }

        private static void BuildOutput(RunSettings run, Dictionary<string, ConcurrentDictionary<string, bool>> hashMatches)
        {
            var matchedHashes = new List<string>();
            var unmatchedHashes = new List<string>();
            var collisionHashes = new List<string>();

            var matchedStrings = new List<string>();
            var collisionStrings = new List<string>();

            var hashStringMatches = new List<string>();
            var hashStringCollisions = new List<string>();

            foreach (var item in hashMatches)
            {
                var hash = item.Key;
                var matches = item.Value;

                if (matches.Count == 0)
                {
                    unmatchedHashes.Add(item.Key);
                    continue;
                }

                if (matches.Count == 1)
                {
                    matchedHashes.Add(hash);
                    string match = matches.First().Key;
                    matchedStrings.Add(match);
                    hashStringMatches.Add($"{hash} {match}");
                    continue;
                }

                //Collision
                collisionHashes.Add(hash);
                StringBuilder line = new StringBuilder(hash.PadLeft(13));
                line.Append(" ");
                foreach (var match in matches.Keys)
                {
                    collisionStrings.Add(match);

                    line.Append(match);
                    line.Append("||");
                }
                hashStringCollisions.Add(line.ToString());
            }//foreach hashMatches

            matchedHashes.Sort();
            unmatchedHashes.Sort();
            collisionHashes.Sort();

            matchedStrings.Sort();
            collisionStrings.Sort();

            hashStringMatches.Sort();
            hashStringCollisions.Sort();


            Console.WriteLine("Stats:");
            int numInputHashes = hashMatches.Count;

            int numHashPlaces = numInputHashes.ToString().Length;
            int numStringPlaces = numInputHashes.ToString().Length;

            float hashPerMult = numInputHashes;

            float unmatchedHashesPercent = 0;
            float matchedHashesPercent = 0;
            float collisionHashesPercent = 0;
            float unmatchedStringsPercent = 0;
            float matchedStringsPercent = 0;

            if (numInputHashes > 0)
            {
                hashPerMult = 100.0f / (float)numInputHashes;
                unmatchedHashesPercent = unmatchedHashes.Count * hashPerMult;
                matchedHashesPercent = matchedHashes.Count * hashPerMult;
                collisionHashesPercent = collisionHashes.Count * hashPerMult;
            }
            /*
            if (numInputStrings > 0) {
                float stringPerMult = 100.0f / (float)numInputStrings;
                unmatchedStringsPercent = unmatchedStringsNew.Count * stringPerMult;
                matchedStringsPercent = matchedStringsNew.Count * stringPerMult;
            }
            */

            Console.WriteLine("unmatchedHashes    [" + unmatchedHashes.Count.ToString().PadLeft(numHashPlaces) + "/" + numInputHashes + "] - " + unmatchedHashesPercent + "%");
            Console.WriteLine("matchedHashes      [" + matchedHashes.Count.ToString().PadLeft(numHashPlaces) + "/" + numInputHashes + "] - " + matchedHashesPercent + "%");
            Console.WriteLine("collsionHashes     [" + collisionHashes.Count.ToString().PadLeft(numHashPlaces) + "/" + numInputHashes + "] - " + collisionHashesPercent + "%");
            //Console.WriteLine("unmatchedStrings   [" + unmatchedStringsNew.Count.ToString().PadLeft(numStringPlaces) + "/" + numInputStrings + "] - " + unmatchedStringsPercent + "%");
            //Console.WriteLine("matchedStrings     [" + matchedStringsNew.Count.ToString().PadLeft(numStringPlaces) + "/" + numInputStrings + "] - " + matchedStringsPercent + "%");


            Console.WriteLine("Writing out files");

            //tex output files will be 
            //for single file input:
            //<input path>\<filename>_<somesuffix>.txt
            //for directory input:
            //<parent of input path>\<foldername><Hashes | Strings>_<somesuffix>.txt
            string hashesPath = "";
            if (File.Exists(run.inputHashesPath))
            {
                hashesPath = Path.GetDirectoryName(run.inputHashesPath) + "\\" + Path.GetFileNameWithoutExtension(run.inputHashesPath);
            } else
            {
                string parent = Directory.GetParent(run.inputHashesPath).FullName;
                hashesPath = Path.Combine(run.inputHashesPath, "..") + "\\" + new DirectoryInfo(run.inputHashesPath).Name + "Hashes";
            }
            string stringsPath = "";
            if (File.Exists(run.inputStringsPath))
            {
                stringsPath = Path.GetDirectoryName(run.inputStringsPath) + "\\" + Path.GetFileNameWithoutExtension(run.inputStringsPath);
            } else
            {
                stringsPath = Path.Combine(run.inputStringsPath, "..") + "\\" + new DirectoryInfo(run.inputStringsPath).Name + "Strings";
            }

            hashesPath = Regex.Replace(hashesPath, @"\\", "/");
            stringsPath = Regex.Replace(stringsPath, @"\\", "/");

            //tex delete since we might not be overwriting and old one could cause confusion
            if (File.Exists(stringsPath + "_HashStringsCollisions.txt"))
            {
                File.Delete(stringsPath + "_HashStringsCollisions.txt");
            }
            if (File.Exists(stringsPath + "_collisionStrings.txt"))
            {
                File.Delete(stringsPath + "_collisionStrings.txt");
            }
            if (hashStringCollisions.Count > 0)
            {
                File.WriteAllLines(stringsPath + "_HashStringsCollisions.txt", hashStringCollisions);
            }
            File.WriteAllLines(stringsPath + "_HashStringMatches.txt", hashStringMatches);

            File.WriteAllLines(hashesPath + "_unmatchedHashes.txt", unmatchedHashes);
            File.WriteAllLines(hashesPath + "_matchedHashes.txt", matchedHashes);

            //File.WriteAllLines(stringsPath + "_unmatchedStrings.txt", unmatchedStringsList);
            File.WriteAllLines(stringsPath + "_matchedStrings.txt", matchedStrings);
            if (collisionStrings.Count > 0)
            {
                File.WriteAllLines(stringsPath + "_collisionStrings.txt", collisionStrings);
            }
        }

        /// <summary>
        /// Tests all strings in all files in inputStringsFiles using HashFunc
        /// If a match is found it's added to hashMatches which is a dictionary of hash, list<matches> (since multiple strings can match a hash aka a collision)
        /// </summary>
        /// <returns>
        /// hashMatches
        /// </returns>
        private static Dictionary<string, ConcurrentDictionary<string, bool>> TestStrings(RunSettings run, HashFunction HashFunc, List<string> inputHashesList, List<string> inputStringsFiles)
        {
            bool isPathCode = false;
            if (run.funcType.ToLower().Contains("pathcode"))//GOTCHA: not to be confused with pathfilenamecode which retains it's extension
            {
                isPathCode = true;
            }

            // Prep hashMatches with input hashes
            // using ConcurrentDictionary<string, byte> as a concurrent hashset
            var hashMatches = new Dictionary<string, ConcurrentDictionary<string, bool>>();
            foreach (var hash in inputHashesList)
            {
                hashMatches.Add(hash, new ConcurrentDictionary<string, bool>());
            }

            foreach (var filePath in inputStringsFiles)
            {
                Console.WriteLine(filePath);
                var lines = File.ReadLines(filePath);
                //Parallel.ForEach(File.ReadLines(filePath), (Action<string>)(line => {
                foreach (var lineX in lines)
                {
                    var line = lineX;//DEBUGNOW

                    if (isPathCode)
                    {
                        line = FixPathCodePath(line);
                    }

                    AddMatch(hashMatches, HashFunc, line);

                    if (run.tryVariations)
                    {
                        AddMatch(hashMatches, HashFunc, line.ToLower());
                        AddMatch(hashMatches, HashFunc, line.ToUpper());
                        if (line.Length > 1)
                        {
                            string capFirst = char.ToUpper(line[0]) + line.Substring(1);
                            AddMatch(hashMatches, HashFunc, capFirst);
                        }
                    }
                    if (run.tryExtensions)
                    {
                        //DEBUGNOW
                        // have extra .s in  Hashing.FileExtensions
                        var testExtensions = new List<string> {
                           "evf",
                           "ftexs",
                           "lng",
                        };
                        foreach (var testExtension in testExtensions)
                        {
                            string extLine = $"{line}.{testExtension}";
                            AddMatch(hashMatches, HashFunc, extLine);
                        }

                        foreach (var testExtension in Hashing.FileExtensions)
                        {
                            string extLine = $"{line}.{testExtension}";
                            AddMatch(hashMatches, HashFunc, extLine);
                        }
                    }
                }//));
            }

            return hashMatches;
        }

        /// <summary>
        /// Adds to hashMatches
        /// </summary>
        private static void AddMatch(Dictionary<string, ConcurrentDictionary<string, bool>> hashMatches, HashFunction HashFunc, string testString)
        {
            ConcurrentDictionary<string, bool> matches;
            string hash = HashFunc(testString);
            if (hashMatches.TryGetValue(hash, out matches))
            {
                matches[testString] = true;
            } else
            {
                //no match for testString
            }
        }

        /// <summary>
        /// Outputs hashes for input strings for each HashFunc
        /// </summary>
        private static void BuildHashesForDict(List<string> inputStringsPaths)
        {
            //Parallel.ForEach(hashFuncs.Keys, funcName => {
            foreach (string funcName in hashFuncs.Keys)
            {
                HashFunction HashFunc = hashFuncs[funcName];
                foreach (var filePath in inputStringsPaths)
                {
                    Console.WriteLine(funcName + " hashing " + filePath);

                    string outputPath = Path.GetDirectoryName(filePath) + "\\" + Path.GetFileNameWithoutExtension(filePath) + "_" + funcName + "HashStringList.txt";
                    using (StreamWriter sw = new StreamWriter(outputPath))
                    {
                        foreach (string line in File.ReadLines(filePath))
                        {
                            sw.WriteLine(line + " " + HashFunc(line));
                        }
                    }

                    Console.WriteLine("Finished " + funcName + " hashing " + filePath);
                }
            }
            //});
        }

        private static string FixPathCodePath(string line)
        {
            if (line.Contains("/") || line.Contains("\\"))
            {
                if (line.Contains('.'))
                {
                    if (!FilePathHasInvalidChars(line))
                    {
                        line = line.Substring(0, line.IndexOf('.'));
                    }
                }
            }
            line = line.Replace('\\', '/');
            return line;
        }

        private static List<string> GetFileList(string inputStringsPath)
        {
            if (!Path.IsPathRooted(inputStringsPath))
            {
                inputStringsPath = Path.GetFullPath(inputStringsPath);
            }

            List<string> fileList = new List<string>();
            if (File.Exists(inputStringsPath))
            {
                fileList.Add(inputStringsPath);
            }
            if (Directory.Exists(inputStringsPath))
            {
                fileList = Directory.GetFiles(inputStringsPath, "*.txt", SearchOption.AllDirectories).ToList<string>();
            }

            return fileList;
        }

        static void ShowUsageInfo()
        {
            Console.WriteLine("HashWrangler by tinmantex\n" +
                              "  For comparing lists of hashes against lists of strings and outputting found and unfound lists.\n" +
                              "Usage:\n" +
                              "  HashWrangler <hashes file path> <strings file path> [-HashFunction <hash function name>]\n" +
                              "  HashFunction defaults to StrCode32, others are StrCode64, PathCode64, PathCode64Gz, PathFileNameCode64, PathFileNameCode32.\n" +
                              "  Options are case insensitive\n" +
                              "  or\n" +
                              " HashWrangler <dictionary file path>\n" +
                              "   outputs <dictionary>_<hash func name>HashStringList.txt for each hash function on the input dictionary."
                              );
        }

        private static Dictionary<string, HashSet<string>> BuildDictionary(string path, HashFunction HashFunc)
        {
            var dictionary = new Dictionary<string, HashSet<string>>();

            string[] files = null;
            if (File.Exists(path))
            {
                files = new string[] { path };
            }
            if (Directory.Exists(path))
            {
                files = Directory.GetFiles(path, "*.txt");
            }

            if (files != null)
            {
                foreach (string filePath in files)
                {
                    ReadDictionary(filePath, HashFunc, ref dictionary);
                }
            }

            return dictionary;
        }

        private static void ReadDictionary(string path, HashFunction HashFunc, ref Dictionary<string, HashSet<string>> dictionary)
        {
            Console.WriteLine("ReadDictionary " + path);
            if (!File.Exists(path))
            {
                Console.WriteLine("Could not find " + path);
                return;
            }

            int dups = 0;

            string[] lines;
            try
            {
                lines = File.ReadAllLines(path);
            } catch (Exception e)
            {
                Console.WriteLine("Unable to read the dictionary " + path + " " + e.Message);
                return;
            }

            foreach (string line in lines)
            {
                var hash = HashFunc(line);

                HashSet<string> stringsForHash;
                if (dictionary.TryGetValue(hash, out stringsForHash))
                {
                    bool isNew = stringsForHash.Add(line);
                    if (!isNew)
                    {
                        //Console.WriteLine("string " + line + " already exists for hash " + hash);//DEBUGNOW
                        dups++;
                    } else
                    {
                        string strings = String.Join(" | ", stringsForHash.ToArray());
                        //Console.WriteLine("hash collision for " + hash + ": " + strings);//DEBUGNOW
                    }
                } else
                {
                    stringsForHash = new HashSet<string>();
                    stringsForHash.Add(line);
                    dictionary.Add(hash, stringsForHash);
                }
            }
            if (dups > 0)
            {
                Console.WriteLine(dups + " strings from " + Path.GetFileName(path) + " were already in dictionary");//DEBUGNOW
            }
        }

        private static List<string> GetInputHashes(string path, bool hashesToHex = false)
        {
            var inputHashes = new HashSet<string>();

            string[] files = null;
            if (File.Exists(path))
            {
                files = new string[] { path };
            }
            if (Directory.Exists(path))
            {
                files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
            }

            if (files != null)
            {
                foreach (string filePath in files)
                {
                    ReadInputHashes(filePath, ref inputHashes, hashesToHex);
                }
            }

            return inputHashes.ToList();
        }

        private static void ReadInputHashes(string path, ref HashSet<string> inputHashes, bool hashesToHex = false)
        {
            Console.WriteLine("ReadInputHashes " + path);
            if (!File.Exists(path))
            {
                Console.WriteLine("Could not find " + path);
                return;
            }

            int duplicates = 0;

            string[] lines;
            try
            {
                lines = File.ReadAllLines(path);
            } catch (Exception e)
            {
                Console.WriteLine("Unable to read inputHashes " + path + " " + e);
                return;
            }

            foreach (var line in lines)
            {
                if (line == "")
                {
                    Console.WriteLine("empty line in " + path);
                    continue;
                }
                var hash = line;
                if (hashesToHex)
                {
                    var uhash = ulong.Parse(hash);
                    uhash = uhash & 0x3FFFFFFFFFFFF;
                    hash = uhash.ToString("x");
                }
                bool isNew = inputHashes.Add(hash);
                if (!isNew)
                {
                    duplicates++;
                }
            }

            if (duplicates > 0)
            {
                Console.WriteLine("There is " + duplicates + " duplicates in " + path);
            }
        }

        public static bool FilePathHasInvalidChars(string path)
        {
            bool ret = false;
            if (!string.IsNullOrEmpty(path))
            {
                if (path.Length >= 260)
                {
                    return true;
                }

                try
                {
                    // Careful!
                    //    Path.GetDirectoryName("C:\Directory\SubDirectory")
                    //    returns "C:\Directory", which may not be what you want in
                    //    this case. You may need to explicitly add a trailing \
                    //    if path is a directory and not a file path. As written, 
                    //    this function just assumes path is a file path.
                    string fileName = System.IO.Path.GetFileName(path);
                    string fileDirectory = System.IO.Path.GetDirectoryName(path);

                    // we don't need to do anything else,
                    // if we got here without throwing an 
                    // exception, then the path does not
                    // contain invalid characters
                } catch (ArgumentException)
                {
                    // Path functions will throw this 
                    // if path contains invalid chars
                    ret = true;
                }
            }
            return ret;
        }

    }
}
