using Newtonsoft.Json;
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
            public string inputHashesPath = "";
            public string inputStringsPath = "";
            public string funcType = "StrCode32";

            public bool tryVariations = false;//WIP DEBUGNOW trouble is this fires off alot more collisions for StrCode32 depending on input
            public bool tryExtensions = false;
            public bool hashesToHex = false;

            //tex for validating using mgsv-lookup-strings repo layout
            public bool validateMode = false;
            public string validateRoot = "";//TODO: split into validateRoot, validateHashTypes (pointing to hash types .json)

            public bool matchedStringsNameIsDictionary = false;//tex Should only be used when input strings are a folder, else it will overwrite the input strings file.
            //By default matched strings will be written to <inputStringsPath>Strings_matchedStrings.txt (if input strings are a folder), 
            //when set to true matched strings will be written to <inputStringsPath>.txt , which saves hasle of having to rename stuff if your workflow is for updating the mgsv-strings github repo
        }//RunSettings

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowUsageInfo();
                Console.WriteLine();

                RunSettings defaultConfig = new RunSettings();
                JsonSerializerSettings serializeSettings = new JsonSerializerSettings();
                serializeSettings.Formatting = Formatting.Indented;
                string jsonStringOut = JsonConvert.SerializeObject(defaultConfig, serializeSettings);
                string jsonOutPath = Directory.GetCurrentDirectory() + "/default-config.json";
                jsonOutPath = Regex.Replace(jsonOutPath, @"\\", "/");
                File.WriteAllText(jsonOutPath, jsonStringOut, Encoding.UTF8);
                Console.WriteLine($"Writing default run config to {jsonOutPath}");
                return;
            }//if args==0

            RunSettings runSettings = new RunSettings();

            string configPath = GetPath(args[0]);
            if (configPath == null)
            {
                Console.WriteLine("ERROR: invalid path " + args[0]);
                return;
            }
            if (configPath.Contains(".json"))
            {
                Console.WriteLine("Using run settings " + configPath);
                string jsonString = File.ReadAllText(configPath, Encoding.UTF8);
                runSettings = JsonConvert.DeserializeObject<RunSettings>(jsonString);
            } else {
                runSettings.inputHashesPath = args[0];
                runSettings.inputStringsPath = "";

                if (args.Count() > 1)
                {
                    runSettings.inputStringsPath = args[1];
                }

                if (args.Count() > 3)
                {
                    if (args[2].ToLower() == "-hashfunction" || args[2].ToLower() == "-h")
                    {
                        runSettings.funcType = args[3];
                    }
                }
            }//read args

            if (!runSettings.validateMode)
            {
                //tex hashwrangler <strings path> - BuildHashesForStrings
                if (runSettings.inputStringsPath == "")
                {
                    runSettings.inputStringsPath = runSettings.inputHashesPath;

                    Console.WriteLine("Building hashes for strings:");
                    List<string> stringsFilesPaths = GetFileList(runSettings.inputStringsPath);
                    if (stringsFilesPaths.Count == 0)
                    {
                        Console.WriteLine($"ERROR: Could not find any input strings in {runSettings.inputStringsPath}");
                        return;
                    }

                    foreach (string funcName in hashFuncs.Keys)
                    {
                        BuildHashesForStrings(stringsFilesPaths, funcName);
                    }
                    return;
                }

                if (!Directory.Exists(runSettings.inputHashesPath) && File.Exists(runSettings.inputHashesPath) == false)
                {
                    Console.WriteLine("ERROR: Could not find " + runSettings.inputHashesPath);
                    return;
                }

                if (!Directory.Exists(runSettings.inputStringsPath) && File.Exists(runSettings.inputStringsPath) == false)
                {
                    Console.WriteLine("ERROR: Could not find " + runSettings.inputStringsPath);
                    return;
                }
            } else {
                if (!Directory.Exists(Path.GetDirectoryName(runSettings.validateRoot)))
                {
                    Console.WriteLine("ERROR: Could not find " + runSettings.validateRoot);
                    return;
                }
            }//validatemode?

            FixupPath(ref runSettings.inputHashesPath);
            FixupPath(ref runSettings.inputStringsPath);
            FixupPath(ref runSettings.validateRoot);

            if (runSettings.validateMode)
            {
                string statsPath = Path.GetDirectoryName(runSettings.validateRoot) + "\\ValidateStats.txt";
                if (File.Exists(statsPath))
                {
                    File.Delete(statsPath);
                }


                string jsonString = File.ReadAllText(runSettings.validateRoot, Encoding.UTF8);
                var hashTypes = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);

                string rootPath = Path.GetDirectoryName(runSettings.validateRoot);
                string hashesPathRoot = rootPath + "\\" + "Hashes";
                string directoriesPathRoot = rootPath + "\\" + "Dictionaries";

                string[] hashesGamePaths = Directory.GetDirectories(hashesPathRoot);
                foreach (string gamePath in hashesGamePaths)
                {
                    string gameId = new DirectoryInfo(gamePath).Name;

                    foreach (var entry in hashTypes)
                    {
                        string hashName = entry.Key;
                        string hashType = entry.Value;


                        string hashNamePath = gamePath + "\\" + hashName;
                        if (!Directory.Exists(hashNamePath))
                        {
                            Console.WriteLine("WARNING: Could not find path " + hashNamePath);
                            continue;
                        }

                        string hashNamesDictionaryPath = $"{directoriesPathRoot}\\{gameId}\\{hashName}.txt";
                        if (!File.Exists(hashNamesDictionaryPath))
                        {
                            Console.WriteLine("ERROR: Could not find dictionary " + hashNamesDictionaryPath);
                            return;
                        }


                        runSettings.funcType = hashType;
                        runSettings.inputHashesPath = hashNamePath;
                        runSettings.inputStringsPath = hashNamesDictionaryPath;



                        HashFunction HashFunc;
                        try
                        {
                            HashFunc = hashFuncs[runSettings.funcType.ToLower()];
                        } catch (KeyNotFoundException)
                        {
                            Console.WriteLine("ERROR: Could not find hash function " + runSettings.funcType);
                            return;
                        }

                        Console.WriteLine("Reading input hashes:");
                        List<string> inputHashesList = GetInputHashes(runSettings.inputHashesPath, runSettings.hashesToHex);
                        if (inputHashesList == null)
                        {
                            Console.WriteLine($"ERROR: Could not find any input hashes in {runSettings.inputHashesPath}");
                            return;
                        }

                        Console.WriteLine("Building strings file list");
                        List<string> inputStringsFiles = GetFileList(runSettings.inputStringsPath);
                        if (inputStringsFiles.Count == 0)
                        {
                            Console.WriteLine($"ERROR: Could not find any input strings in {runSettings.inputStringsPath}");
                            return;
                        }

                        Console.WriteLine($"Testing strings using HashFunction {runSettings.funcType}:");
                        var unmatchedStrings = new ConcurrentQueue<string>();

                        var testStringsStopWatch = new System.Diagnostics.Stopwatch();
                        testStringsStopWatch.Start();
                        var hashMatches = TestStrings(runSettings, HashFunc, inputHashesList, inputStringsFiles, unmatchedStrings);
                        testStringsStopWatch.Stop();
                        var timeSpan = testStringsStopWatch.Elapsed;
                        Console.WriteLine($"TestStrings completed in {timeSpan.Hours}:{timeSpan.Minutes}:{timeSpan.Seconds}:{timeSpan.Milliseconds}");

                        Console.WriteLine("Building output");
                        BuildOutput(runSettings, hashMatches, unmatchedStrings);
                    }// for hashtypes
                }// for hashesGamePaths

                return;
            }//do validatemode

            {//classic hash wrangle
                HashFunction HashFunc;
                try
                {
                    HashFunc = hashFuncs[runSettings.funcType.ToLower()];
                } catch (KeyNotFoundException)
                {
                    Console.WriteLine("ERROR: Could not find hash function " + runSettings.funcType);
                    return;
                }

                Console.WriteLine("Reading input hashes:");
                List<string> inputHashesList = GetInputHashes(runSettings.inputHashesPath, runSettings.hashesToHex);
                if (inputHashesList == null)
                {
                    Console.WriteLine($"ERROR: Could not find any input hashes in {runSettings.inputHashesPath}");
                    return;
                }

                Console.WriteLine("Building strings file list");
                List<string> inputStringsFiles = GetFileList(runSettings.inputStringsPath);
                if (inputStringsFiles.Count == 0)
                {
                    Console.WriteLine($"ERROR: Could not find any input strings in {runSettings.inputStringsPath}");
                    return;
                }

                Console.WriteLine($"Testing strings using HashFunction {runSettings.funcType}:");
                var unmatchedStrings = new ConcurrentQueue<string>();
                var testStringsStopWatch = new System.Diagnostics.Stopwatch();
                testStringsStopWatch.Start();
                var hashMatches = TestStrings(runSettings, HashFunc, inputHashesList, inputStringsFiles, unmatchedStrings);
                testStringsStopWatch.Stop();
                var timeSpan = testStringsStopWatch.Elapsed;
                Console.WriteLine($"TestStrings completed in {timeSpan.Hours}:{timeSpan.Minutes}:{timeSpan.Seconds}:{timeSpan.Milliseconds}");

                Console.WriteLine("Building output");
                BuildOutput(runSettings, hashMatches, unmatchedStrings);
            }//classic hash wrangle

            Console.WriteLine("All done");
        }//Main

        private static void BuildOutput(RunSettings runSettings, Dictionary<string, ConcurrentDictionary<string, bool>> hashMatches, ConcurrentQueue<string> unmatchedStringsQueue)
        {
            var matchedHashes = new List<string>();
            var unmatchedHashes = new List<string>();
            var collisionHashes = new List<string>();

            var matchedStrings = new List<string>();
            var unmatchedStrings = unmatchedStringsQueue.ToList<string>();
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

                //Collision > 1 matches
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
            unmatchedStrings.Sort();
            collisionStrings.Sort();

            hashStringMatches.Sort();
            hashStringCollisions.Sort();



            Console.WriteLine(runSettings.inputStringsPath);
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

            string unmatchedStats = $"unmatchedHashes    [{unmatchedHashes.Count.ToString().PadLeft(numHashPlaces)}/{numInputHashes}] - {unmatchedHashesPercent}%";
            string   matchedStats = $"matchedHashes      [{matchedHashes.Count.ToString().PadLeft(numHashPlaces)}/{numInputHashes}] - {matchedHashesPercent}%";
            string collisionStats = $"collsionHashes     [{collisionHashes.Count.ToString().PadLeft(numHashPlaces)}/{numInputHashes}] - {collisionHashesPercent}%";


            Console.WriteLine(unmatchedStats);
            Console.WriteLine(matchedStats);
            Console.WriteLine(collisionStats);
            //Console.WriteLine($"unmatchedStrings   [{unmatchedStringsNew.Count.ToString().PadLeft(numStringPlaces)}/{numInputStrings}] - {unmatchedStringsPercent}%");
            //Console.WriteLine($"matchedStrings     [{matchedStringsNew.Count.ToString().PadLeft(numStringPlaces)}/{numInputStrings}] - {matchedStringsPercent}%");
            Console.WriteLine($"unmatchedStrings   [{unmatchedStrings.Count.ToString().PadLeft(numStringPlaces)}]");

            if (runSettings.validateMode)
            {
                string statsPath = Path.GetDirectoryName(runSettings.validateRoot) + "\\ValidateStats.txt";
                using (StreamWriter sw = File.AppendText(statsPath))
                {
                    sw.WriteLine(runSettings.inputStringsPath);
                    sw.WriteLine(unmatchedStats);
                    sw.WriteLine(matchedStats);
                    if (collisionHashes.Count > 0)
                    {
                        sw.WriteLine(collisionStats);
                    }
                    sw.WriteLine();
                }
            }//if validateMode


            Console.WriteLine("Writing out files");

            //tex output files will be 
            //for single file input:
            //<input path>\<filename>_<somesuffix>.txt
            //for directory input:
            //<parent of input path>\<foldername><Hashes | Strings>_<somesuffix>.txt
            string hashesPath = "";
            if (File.Exists(runSettings.inputHashesPath))
            {
                hashesPath = Path.GetDirectoryName(runSettings.inputHashesPath) + "\\" + Path.GetFileNameWithoutExtension(runSettings.inputHashesPath);
            } else
            {
                string parent = Directory.GetParent(runSettings.inputHashesPath).FullName;
                hashesPath = Path.Combine(runSettings.inputHashesPath, "..") + "\\" + new DirectoryInfo(runSettings.inputHashesPath).Name;// + "Hashes";
            }
            string stringsPath = "";
            if (File.Exists(runSettings.inputStringsPath))
            {
                stringsPath = Path.GetDirectoryName(runSettings.inputStringsPath) + "\\" + Path.GetFileNameWithoutExtension(runSettings.inputStringsPath);
            } else
            {
                stringsPath = Path.Combine(runSettings.inputStringsPath, "..") + "\\" + new DirectoryInfo(runSettings.inputStringsPath).Name;// + "Strings";
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

            if (unmatchedStrings.Count != 0)
            {
                File.WriteAllLines(stringsPath + "_unmatchedStrings.txt", unmatchedStrings);
            }
            if (runSettings.matchedStringsNameIsDictionary)
            {
                File.WriteAllLines(stringsPath + ".txt", matchedStrings);
            } else
            {
                File.WriteAllLines(stringsPath + "_matchedStrings.txt", matchedStrings);
            }
            if (collisionStrings.Count > 0)
            {
                File.WriteAllLines(stringsPath + "_collisionStrings.txt", collisionStrings);
            }
        }//BuildOutput

        /// <summary>
        /// Tests all strings in all files in inputStringsFiles using HashFunc
        /// If a match is found it's added to hashMatches which is a dictionary of hash, list<matches> (since multiple strings can match a hash aka a collision)
        /// </summary>
        /// <returns>
        /// hashMatches
        /// </returns>
        private static Dictionary<string, ConcurrentDictionary<string, bool>> TestStrings(RunSettings runSettings, HashFunction HashFunc, List<string> inputHashesList, List<string> inputStringsFiles, ConcurrentQueue<string> unmatchedStrings)
        {
            bool isPathCode = false;
            string funcType = runSettings.funcType.ToLower();
            //GOTCHA: not to be confused with pathfilenamecode which retains it's extension //the issue this is trying to workaround is pathcode hashing function is stripping . anway, so you'll get multiple matches
            //however GZ is weird instead of hashing its extension it uses an extension id see TypeExtensions in GzsTool 0.2 Hashing.cs, or if no id is found (or rather id 0 extension=="") it just expects the entire string to be hashed, extension included
            if (funcType.Contains("pathcode") && !funcType.Contains("gz"))
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
                var lines = File.ReadLines(filePath);//DEBUGNOW encoding?
                //Parallel.ForEach(lines, lineX => {
                foreach (var lineX in lines)
                {
                    var line = lineX;//DEBUGNOW

                    if (isPathCode)
                    {
                        line = FixPathCodePath(line);
                    }

                    AddMatch(hashMatches, HashFunc, line, unmatchedStrings);

                    if (runSettings.tryVariations)
                    {
                        AddMatch(hashMatches, HashFunc, line.ToLower(), unmatchedStrings);
                        AddMatch(hashMatches, HashFunc, line.ToUpper(), unmatchedStrings);
                        if (line.Length > 1)
                        {
                            string capFirst = char.ToUpper(line[0]) + line.Substring(1);
                            AddMatch(hashMatches, HashFunc, capFirst, unmatchedStrings);
                        }
                    }
                    if (runSettings.tryExtensions)
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
                            AddMatch(hashMatches, HashFunc, extLine, unmatchedStrings);
                        }

                        foreach (var testExtension in Hashing.FileExtensions)
                        {
                            string extLine = $"{line}.{testExtension}";
                            AddMatch(hashMatches, HashFunc, extLine, unmatchedStrings);
                        }
                    }
                }
                //);
            }

            return hashMatches;
        }

        /// <summary>
        /// Adds to hashMatches
        /// </summary>
        private static void AddMatch(Dictionary<string, ConcurrentDictionary<string, bool>> hashMatches, HashFunction HashFunc, string testString, ConcurrentQueue<string> unmatchedStrings)
        {
            ConcurrentDictionary<string, bool> matches;
            string hash = HashFunc(testString);
            if (hashMatches.TryGetValue(hash, out matches))
            {
                matches[testString] = true;
            } else
            {
                //no match for testString
                unmatchedStrings.Enqueue(testString);
            }
        }//AddMatch

        /// <summary>
        /// Outputs hashes for input strings for each HashFunc
        /// </summary>
        private static void BuildHashesForStrings(List<string> inputStringsPaths, string funcName)
        {

                HashFunction HashFunc = hashFuncs[funcName];
                foreach (var filePath in inputStringsPaths)
                {
                    Console.WriteLine(funcName + " hashing " + filePath);

                    string outputPath = Path.GetDirectoryName(filePath) + "\\" + Path.GetFileNameWithoutExtension(filePath) + "_" + funcName + "HashStringList.txt";
                    using (StreamWriter sw = new StreamWriter(outputPath, false, Encoding.UTF8))
                    {
                        foreach (string line in File.ReadLines(filePath))//DEBUGNOW encoding?
                        {
                            sw.WriteLine(line + "," + HashFunc(line));
                        }
                    }

                    Console.WriteLine("Finished " + funcName + " hashing " + filePath);
                }
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
                lines = File.ReadAllLines(path);//DEBUGNOW encoding?
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
                lines = File.ReadAllLines(path);//DEBUGNOW encoding?
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

        private static string GetPath(string path)
        {
            if (Directory.Exists(path) || File.Exists(path))
            {
                if (!Path.IsPathRooted(path))
                {
                    path = Path.GetFullPath(path);
                }
            } else
            {
                path = null;
            }

            return path;
        }//GetPath

        static void FixupPath(ref string path)
        {
            if (path == null)
            {
                return;
            }

            if (path == "")
            {
                return;
            }

            if (!Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(path);
            }
            path = Regex.Replace(path, @"\\", "/");
        }

    }//Program
}
