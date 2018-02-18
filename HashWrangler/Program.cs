using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Utility.Hashing;

namespace HashWrangler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
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
            if (inputStringsPath == null)
            {
                buildHashesForDict = true;
                inputStringsPath = inputHashesPath;
            }

            string funcType = "StrCode32";

            if (args.Count() > 3)
            {
                if (args[2].ToLower() == "-hashfunction" || args[2].ToLower() == "-h")
                {
                    funcType = args[3];
                }
            }

            bool tryVariations = false;//WIP

            if (!Directory.Exists(inputHashesPath) && File.Exists(inputHashesPath) == false)
            {
                Console.WriteLine("Could not find " + inputHashesPath);
                return;
            }

            if (!Directory.Exists(inputStringsPath) && File.Exists(inputStringsPath) == false)
            {
                Console.WriteLine("Could not find " + inputStringsPath);
                return;
            }

            if (!Path.IsPathRooted(inputHashesPath))
            {
                inputHashesPath = Path.GetFullPath(inputHashesPath);
            }
            if (!Path.IsPathRooted(inputStringsPath))
            {
                inputStringsPath = Path.GetFullPath(inputStringsPath);
            }

            inputHashesPath = Regex.Replace(inputHashesPath, @"\\", "/");
            inputStringsPath = Regex.Replace(inputStringsPath, @"\\", "/");




            HashFunction HashFunc;
            try
            {
                HashFunc = hashFuncs[funcType.ToLower()];
            } catch (KeyNotFoundException)
            {
                HashFunc = StrCode32Str;
                Console.WriteLine("ERROR: Could not find hash function " + funcType);
                return;
            }

            if (buildHashesForDict)
            {
                Console.WriteLine("buildHashesForDict");

                List<string> dictFiles = GetFileList(inputStringsPath);

                if (dictFiles.Count == 0)
                {
                    return;
                }

                Parallel.ForEach(hashFuncs.Keys, funcName => {
                    //foreach (string funcName in hashFuncs.Keys)
                    {
                        HashFunc = hashFuncs[funcName];
                        foreach (var filePath in dictFiles)
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
                });
                return;
            }


            Console.WriteLine("Read input hashes:");
            List<string> inputHashesList = GetInputHashes(inputHashesPath);
            if (inputHashesList == null)
            {
                return;
            }

            //TODO: have GetInputHashes build inputHashesNew directly
            var hashMatches = new Dictionary<string, ConcurrentBag<string>>();
            foreach (var hash in inputHashesList)
            {
                hashMatches.Add(hash, new ConcurrentBag<string>());
            }


            Console.WriteLine("Read input strings:");
            List<string> files = GetFileList(inputStringsPath);

            if (files.Count == 0)
            {
                return;
            }

            Console.WriteLine("Testing strings using HashFunction " + funcType);

            foreach (var filePath in files)
            {
                Console.WriteLine(filePath);
                Parallel.ForEach(File.ReadLines(filePath), (Action<string>)(line => {
                    var hash = HashFunc(line);
                    ConcurrentBag<string> matches;
                    if (hashMatches.TryGetValue(hash, out matches))
                    {
                        matches.Add(line);
                    } else
                    {
                        //no match 
                    }

                    //WIP
                    if (tryVariations)
                    {
                        hash = HashFunc(line.ToLower());
                        if (hashMatches.TryGetValue(hash, out matches))
                        {
                            matches.Add(line.ToLower());
                        } else
                        {
                            //no match 
                        }

                        hash = HashFunc(line.ToUpper());
                        if (hashMatches.TryGetValue(hash, out matches))
                        {
                            matches.Add(line.ToUpper());
                        } else
                        {
                            //no match 
                        }

                        if (line.Length > 1)
                        {
                            string capFirst = char.ToUpper(line[0]) + line.Substring(1);
                            hash = HashFunc(capFirst);
                            if (hashMatches.TryGetValue(hash, out matches))
                            {
                                matches.Add(capFirst);
                            } else
                            {
                                //no match 
                            }
                        }
                    }
                }));
            }

            Console.WriteLine("Building output");
            var matchedHashes = new List<string>();
            var unmatchedHashes = new List<string>();
            var collisionHashes = new List<string>();

            var matchedStrings = new List<string>();
            var collisionStrings = new List<string>();

            var hashStringMatches = new List<string>();
            var hashStringCollisions = new List<string>();

            foreach (var item in hashMatches)
            {
                ConcurrentBag<string> matches = item.Value;
                if (matches.Count == 0)
                {
                    unmatchedHashes.Add(item.Key);
                } else
                {
                    if (matches.Count == 1)
                    {
                        matchedHashes.Add(item.Key);
                        string match;
                        matches.TryTake(out match);
                        matchedStrings.Add(match);
                        hashStringMatches.Add(string.Format("{0} {1}", item.Key, match));
                    } else
                    { //Collision
                        //tex crush it down to uniques since there's input strings havent been checked for duplpicates
                        HashSet<string> matchesUnique = new HashSet<string>();
                        foreach (var match in matches)
                        {
                            matchesUnique.Add(match);
                        }

                        if (matchesUnique.Count == 1)
                        {
                            matchedHashes.Add(item.Key);
                            string match = matchesUnique.First();
                            matchedStrings.Add(match);
                            hashStringMatches.Add(string.Format("{0} {1}", item.Key, match));
                        } else
                        {
                            collisionHashes.Add(item.Key);
                            StringBuilder line = new StringBuilder(item.Key.PadLeft(13));
                            line.Append(" ");
                            foreach (var match in matchesUnique)
                            {
                                line.Append(match);
                                line.Append("||");

                                collisionStrings.Add(match);
                            }
                            hashStringCollisions.Add(line.ToString());
                        }
                    }
                }
            }

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
            if (File.Exists(inputHashesPath))
            {
                hashesPath = Path.GetDirectoryName(inputHashesPath) + "\\" + Path.GetFileNameWithoutExtension(inputHashesPath);
            } else
            {
                string parent = Directory.GetParent(inputHashesPath).FullName;
                hashesPath = Path.Combine(inputHashesPath, "..") + "\\" + new DirectoryInfo(inputHashesPath).Name + "Hashes";
            }
            string stringsPath = "";
            if (File.Exists(inputStringsPath))
            {
                stringsPath = Path.GetDirectoryName(inputStringsPath) + "\\" + Path.GetFileNameWithoutExtension(inputStringsPath);
            } else
            {
                stringsPath = Path.Combine(inputStringsPath, "..") + "\\" + new DirectoryInfo(inputStringsPath).Name + "Strings";
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

            Console.WriteLine("All done");
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

        private static List<string> GetInputHashes(string path)
        {
            var inputHashes = new HashSet<string>();

            string[] files = null;
            if (File.Exists(path))
            {
                files = new string[] { path };
            }
            if (Directory.Exists(path))
            {
                files = Directory.GetFiles(path, "*.txt",SearchOption.AllDirectories);
            }

            if (files != null)
            {
                foreach (string filePath in files)
                {
                    ReadInputHashes(filePath, ref inputHashes);
                }
            }

            return inputHashes.ToList();
        }

        private static void ReadInputHashes(string path, ref HashSet<string> inputHashes)
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
                var str32 = line;
                bool isNew = inputHashes.Add(str32);
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



    }
}
