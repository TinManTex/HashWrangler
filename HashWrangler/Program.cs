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
using Hashing;
using static Hashing.FoxEngine;
using static Hashing.HashFuncs;
//
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;


namespace HashWrangler
{
    class Program
    {
        //DEBUGNOW
        static void TestFNVHash() { 
            var testvectors = new[] { "", "a", "b", "c", "d", "e", "f", "fo", "foo", "foob", "fooba", "foobar", "\0", "a\0", "b\0", "c\0", "d\0", "e\0", "f\0", "fo\0", "foo\0", "foob\0", "fooba\0", "foobar\0", "ch", "cho", "chon", "chong", "chongo", "chongo ", "chongo w", "chongo wa", "chongo was", "chongo was ", "chongo was h", "chongo was he", "chongo was her", "chongo was here", "chongo was here!", "chongo was here!\n", "ch\0", "cho\0", "chon\0", "chong\0", "chongo\0", "chongo \0", "chongo w\0", "chongo wa\0", "chongo was\0", "chongo was \0", "chongo was h\0", "chongo was he\0", "chongo was her\0", "chongo was here\0", "chongo was here!\0", "chongo was here!\n\0", "cu", "cur", "curd", "curds", "curds ", "curds a", "curds an", "curds and", "curds and ", "curds and w", "curds and wh", "curds and whe", "curds and whey", "curds and whey\n", "cu\0", "cur\0", "curd\0", "curds\0", "curds \0", "curds a\0", "curds an\0", "curds and\0", "curds and \0", "curds and w\0", "curds and wh\0", "curds and whe\0", "curds and whey\0", "curds and whey\n\0", "hi", "hi\0", "hello", "hello\0", "\xff\x00\x00\x01", "\x01\x00\x00\xff", "\xff\x00\x00\x02", "\x02\x00\x00\xff", "\xff\x00\x00\x03", "\x03\x00\x00\xff", "\xff\x00\x00\x04", "\x04\x00\x00\xff", "\x40\x51\x4e\x44", "\x44\x4e\x51\x40", "\x40\x51\x4e\x4a", "\x4a\x4e\x51\x40", "\x40\x51\x4e\x54", "\x54\x4e\x51\x40", "127.0.0.1", "127.0.0.1\0", "127.0.0.2", "127.0.0.2\0", "127.0.0.3", "127.0.0.3\0", "64.81.78.68", "64.81.78.68\0", "64.81.78.74", "64.81.78.74\0", "64.81.78.84", "64.81.78.84\0", "feedface", "feedface\0", "feedfacedaffdeed", "feedfacedaffdeed\0", "feedfacedeadbeef", "feedfacedeadbeef\0", "line 1\nline 2\nline 3", "chongo <Landon Curt Noll> /\\../\\", "chongo <Landon Curt Noll> /\\../\\\0", "chongo (Landon Curt Noll) /\\../\\", "chongo (Landon Curt Noll) /\\../\\\0", "http://antwrp.gsfc.nasa.gov/apod/astropix.html", "http://en.wikipedia.org/wiki/Fowler_Noll_Vo_hash", "http://epod.usra.edu/", "http://exoplanet.eu/", "http://hvo.wr.usgs.gov/cam3/", "http://hvo.wr.usgs.gov/cams/HMcam/", "http://hvo.wr.usgs.gov/kilauea/update/deformation.html", "http://hvo.wr.usgs.gov/kilauea/update/images.html", "http://hvo.wr.usgs.gov/kilauea/update/maps.html", "http://hvo.wr.usgs.gov/volcanowatch/current_issue.html", "http://neo.jpl.nasa.gov/risk/", "http://norvig.com/21-days.html", "http://primes.utm.edu/curios/home.php", "http://slashdot.org/", "http://tux.wr.usgs.gov/Maps/155.25-19.5.html", "http://volcano.wr.usgs.gov/kilaueastatus.php", "http://www.avo.alaska.edu/activity/Redoubt.php", "http://www.dilbert.com/fast/", "http://www.fourmilab.ch/gravitation/orbits/", "http://www.fpoa.net/", "http://www.ioccc.org/index.html", "http://www.isthe.com/cgi-bin/number.cgi", "http://www.isthe.com/chongo/bio.html", "http://www.isthe.com/chongo/index.html", "http://www.isthe.com/chongo/src/calc/lucas-calc", "http://www.isthe.com/chongo/tech/astro/venus2004.html", "http://www.isthe.com/chongo/tech/astro/vita.html", "http://www.isthe.com/chongo/tech/comp/c/expert.html", "http://www.isthe.com/chongo/tech/comp/calc/index.html", "http://www.isthe.com/chongo/tech/comp/fnv/index.html", "http://www.isthe.com/chongo/tech/math/number/howhigh.html", "http://www.isthe.com/chongo/tech/math/number/number.html", "http://www.isthe.com/chongo/tech/math/prime/mersenne.html", "http://www.isthe.com/chongo/tech/math/prime/mersenne.html#largest", "http://www.lavarnd.org/cgi-bin/corpspeak.cgi", "http://www.lavarnd.org/cgi-bin/haiku.cgi", "http://www.lavarnd.org/cgi-bin/rand-none.cgi", "http://www.lavarnd.org/cgi-bin/randdist.cgi", "http://www.lavarnd.org/index.html", "http://www.lavarnd.org/what/nist-test.html", "http://www.macosxhints.com/", "http://www.mellis.com/", "http://www.nature.nps.gov/air/webcams/parks/havoso2alert/havoalert.cfm", "http://www.nature.nps.gov/air/webcams/parks/havoso2alert/timelines_24.cfm", "http://www.paulnoll.com/", "http://www.pepysdiary.com/", "http://www.sciencenews.org/index/home/activity/view", "http://www.skyandtelescope.com/", "http://www.sput.nl/~rob/sirius.html", "http://www.systemexperts.com/", "http://www.tq-international.com/phpBB3/index.php", "http://www.travelquesttours.com/index.htm", "http://www.wunderground.com/global/stations/89606.html" };

            var checkvalues = new[] {
                new ulong[] { 0x811c9dc5, 0x050c5d7e, 0x050c5d7d, 0x050c5d7c, 0x050c5d7b, 0x050c5d7a, 0x050c5d79, 0x6b772514, 0x408f5e13, 0xb4b1178b, 0xfdc80fb0, 0x31f0b262, 0x050c5d1f, 0x70772d5a, 0x6f772bc7, 0x6e772a34, 0x6d7728a1, 0x6c77270e, 0x6b77257b, 0x408f5e7c, 0xb4b117e9, 0xfdc80fd1, 0x31f0b210, 0xffe8d046, 0x6e772a5c, 0x4197aebb, 0xfcc8100f, 0xfdf147fa, 0xbcd44ee1, 0x23382c13, 0x846d619e, 0x1630abdb, 0xc99e89b2, 0x1692c316, 0x9f091bca, 0x2556be9b, 0x628e0e73, 0x98a0bf6c, 0xb10d5725, 0xdd002f35, 0x4197aed4, 0xfcc81061, 0xfdf1479d, 0xbcd44e8e, 0x23382c33, 0x846d61e9, 0x1630abba, 0xc99e89c1, 0x1692c336, 0x9f091ba2, 0x2556befe, 0x628e0e01, 0x98a0bf09, 0xb10d5704, 0xdd002f3f, 0x1c4a506f, 0x6e772a41, 0x26978421, 0xe184ff97, 0x9b5e5ac6, 0x5b88e592, 0xaa8164b7, 0x20b18c7b, 0xf28025c5, 0x84bb753f, 0x3219925a, 0x384163c6, 0x54f010d7, 0x8cea820c, 0xe12ab8ee, 0x26978453, 0xe184fff3, 0x9b5e5ab5, 0x5b88e5b2, 0xaa8164d6, 0x20b18c15, 0xf28025a1, 0x84bb751f, 0x3219922d, 0x384163ae, 0x54f010b2, 0x8cea8275, 0xe12ab8e4, 0x64411eaa, 0x6977223c, 0x428ae474, 0xb6fa7167, 0x73408525, 0xb78320a1, 0x0caf4135, 0xb78320a2, 0xcdc88e80, 0xb78320a3, 0x8ee1dbcb, 0xb78320a4, 0x4ffb2716, 0x860632aa, 0xcc2c5c64, 0x860632a4, 0x2a7ec4a6, 0x860632ba, 0xfefe8e14, 0x0a3cffd8, 0xf606c108, 0x0a3cffdb, 0xf906c5c1, 0x0a3cffda, 0xf806c42e, 0xc07167d7, 0xc9867775, 0xbf716668, 0xc78435b8, 0xc6717155, 0xb99568cf, 0x7662e0d6, 0x33a7f0e2, 0xc2732f95, 0xb053e78f, 0x3a19c02a, 0xa089821e, 0x31ae8f83, 0x995fa9c4, 0x35983f8c, 0x5036a251, 0x97018583, 0xb4448d60, 0x025dfe59, 0xc5eab3af, 0x7d21ba1e, 0x7704cddb, 0xd0071bfe, 0x0ff3774c, 0xb0fea0ea, 0x58177303, 0x4f599cda, 0x3e590a47, 0x965595f8, 0xc37f178d, 0x9711dd26, 0x23c99b7f, 0x6e568b17, 0x43f0245b, 0xbcb7a001, 0x12e6dffe, 0x0792f2d6, 0xb966936b, 0x46439ac5, 0x728d49af, 0xd33745c9, 0xbc382a57, 0x4bda1d31, 0xce35ccae, 0x3b6eed94, 0x445c9c58, 0x3db8bf9d, 0x2dee116d, 0xc18738da, 0x5b156176, 0x2aa7d593, 0xb2409658, 0xe1489528, 0xfe1ee07e, 0xe8842315, 0x3a6a63a2, 0x06d2c18c, 0xf8ef7225, 0x843d3300, 0xbb24f7ae, 0x878c0ec9, 0xb557810f, 0x57423246, 0x87f7505e, 0xbb809f20, 0x8932abb5, 0x0a9b3aa0, 0xb8682a24, 0xa7ac1c56, 0x11409252, },
                new ulong[] { 0x811c9dc5, 0xe40c292c, 0xe70c2de5, 0xe60c2c52, 0xe10c2473, 0xe00c22e0, 0xe30c2799, 0x6222e842, 0xa9f37ed7, 0x3f5076ef, 0x39aaa18a, 0xbf9cf968, 0x050c5d1f, 0x2b24d044, 0x9d2c3f7f, 0x7729c516, 0xb91d6109, 0x931ae6a0, 0x052255db, 0xbef39fe6, 0x6150ac75, 0x9aab3a3d, 0x519c4c3e, 0x0c1c9eb8, 0x5f299f4e, 0xef8580f3, 0xac297727, 0x4546b9c0, 0xbd564e7d, 0x6bdd5c67, 0xdd77ed30, 0xf4ca9683, 0x4aeb9bd0, 0xe0e67ad0, 0xc2d32fa8, 0x7f743fb7, 0x6900631f, 0xc59c990e, 0x448524fd, 0xd49930d5, 0x1c85c7ca, 0x0229fe89, 0x2c469265, 0xce566940, 0x8bdd8ec7, 0x34787625, 0xd3ca6290, 0xddeaf039, 0xc0e64870, 0xdad35570, 0x5a740578, 0x5b004d15, 0x6a9c09cd, 0x2384f10a, 0xda993a47, 0x8227df4f, 0x4c298165, 0xfc563735, 0x8cb91483, 0x775bf5d0, 0xd5c428d0, 0x34cc0ea3, 0xea3b4cb7, 0x8e59f029, 0x2094de2b, 0xa65a0ad4, 0x9bbee5f4, 0xbe836343, 0x22d5344e, 0x19a1470c, 0x4a56b1ff, 0x70b8e86f, 0x0a5b4a39, 0xb5c3f670, 0x53cc3f70, 0xc03b0a99, 0x7259c415, 0x4095108b, 0x7559bdb1, 0xb3bf0bbc, 0x2183ff1c, 0x2bd54279, 0x23a156ca, 0x64e2d7e4, 0x683af69a, 0xaed2346e, 0x4f9f2cab, 0x02935131, 0xc48fb86d, 0x2269f369, 0xc18fb3b4, 0x50ef1236, 0xc28fb547, 0x96c3bf47, 0xbf8fb08e, 0xf3e4d49c, 0x32179058, 0x280bfee6, 0x30178d32, 0x21addaf8, 0x4217a988, 0x772633d6, 0x08a3d11e, 0xb7e2323a, 0x07a3cf8b, 0x91dfb7d1, 0x06a3cdf8, 0x6bdd3d68, 0x1d5636a7, 0xd5b808e5, 0x1353e852, 0xbf16b916, 0xa55b89ed, 0x3c1a2017, 0x0588b13c, 0xf22f0174, 0xe83641e1, 0x6e69b533, 0xf1760448, 0x64c8bd58, 0x97b4ea23, 0x9a4e92e6, 0xcfb14012, 0xf01b2511, 0x0bbb59c3, 0xce524afa, 0xdd16ef45, 0x60648bb3, 0x7fa4bcfc, 0x5053ae17, 0xc9302890, 0x956ded32, 0x9136db84, 0xdf9d3323, 0x32bb6cd0, 0xc8f8385b, 0xeb08bfba, 0x62cc8e3d, 0xc3e20f5c, 0x39e97f17, 0x7837b203, 0x319e877b, 0xd3e63f89, 0x29b50b38, 0x5ed678b8, 0xb0d5b793, 0x52450be5, 0xfa72d767, 0x95066709, 0x7f52e123, 0x76966481, 0x063258b0, 0x2ded6e8a, 0xb07d7c52, 0xd0c71b71, 0xf684f1bd, 0x868ecfa8, 0xf794f684, 0xd19701c3, 0x346e171e, 0x91f8f676, 0x0bf58848, 0x6317b6d1, 0xafad4c54, 0x0f25681e, 0x91b18d49, 0x7d61c12e, 0x5147d25c, 0x9a8b6805, 0x4cd2a447, 0x1e549b14, 0x2fe1b574, 0xcf0cd31e, 0x6c471669, 0x0e5eef1e, 0x2bed3602, 0xb26249e0, 0x2c9b86a4 },
                new ulong[] { 0xcbf29ce484222325, 0xaf63bd4c8601b7be, 0xaf63bd4c8601b7bd, 0xaf63bd4c8601b7bc, 0xaf63bd4c8601b7bb, 0xaf63bd4c8601b7ba, 0xaf63bd4c8601b7b9, 0x08326207b4eb2f34, 0xd8cbc7186ba13533, 0x0378817ee2ed65cb, 0xd329d59b9963f790, 0x340d8765a4dda9c2, 0xaf63bd4c8601b7df, 0x08326707b4eb37da, 0x08326607b4eb3627, 0x08326507b4eb3474, 0x08326407b4eb32c1, 0x08326307b4eb310e, 0x08326207b4eb2f5b, 0xd8cbc7186ba1355c, 0x0378817ee2ed65a9, 0xd329d59b9963f7f1, 0x340d8765a4dda9b0, 0x50a6d3b724a774a6, 0x08326507b4eb341c, 0xd8d5c8186ba98bfb, 0x1ccefc7ef118dbef, 0x0c92fab3ad3db77a, 0x9b77794f5fdec421, 0x0ac742dfe7874433, 0xd7dad5766ad8e2de, 0xa1bb96378e897f5b, 0x5b3f9b6733a367d2, 0xb07ce25cbea969f6, 0x8d9e9997f9df0d6a, 0x838c673d9603cb7b, 0x8b5ee8a5e872c273, 0x4507c4e9fb00690c, 0x4c9ca59581b27f45, 0xe0aca20b624e4235, 0xd8d5c8186ba98b94, 0x1ccefc7ef118db81, 0x0c92fab3ad3db71d, 0x9b77794f5fdec44e, 0x0ac742dfe7874413, 0xd7dad5766ad8e2a9, 0xa1bb96378e897f3a, 0x5b3f9b6733a367a1, 0xb07ce25cbea969d6, 0x8d9e9997f9df0d02, 0x838c673d9603cb1e, 0x8b5ee8a5e872c201, 0x4507c4e9fb006969, 0x4c9ca59581b27f64, 0xe0aca20b624e423f, 0x13998e580afa800f, 0x08326507b4eb3401, 0xd8d5ad186ba95dc1, 0x1c72e17ef0ca4e97, 0x2183c1b327c38ae6, 0xb66d096c914504f2, 0x404bf57ad8476757, 0x887976bd815498bb, 0x3afd7f02c2bf85a5, 0xfc4476b0eb70177f, 0x186d2da00f77ecba, 0xf97140fa48c74066, 0xa2b1cf49aa926d37, 0x0690712cd6cf940c, 0xf7045b3102b8906e, 0xd8d5ad186ba95db3, 0x1c72e17ef0ca4ef3, 0x2183c1b327c38a95, 0xb66d096c914504d2, 0x404bf57ad8476736, 0x887976bd815498d5, 0x3afd7f02c2bf85c1, 0xfc4476b0eb70175f, 0x186d2da00f77eccd, 0xf97140fa48c7400e, 0xa2b1cf49aa926d52, 0x0690712cd6cf9475, 0xf7045b3102b89064, 0x74f762479f9d6aea, 0x08326007b4eb2b9c, 0xd8c4c9186b9b1a14, 0x7b495389bdbdd4c7, 0x3b6dba0d69908e25, 0xd6b2b17bf4b71261, 0x447bfb7f98e615b5, 0xd6b2b17bf4b71262, 0x3bd2807f93fe1660, 0xd6b2b17bf4b71263, 0x3329057f8f16170b, 0xd6b2b17bf4b71264, 0x2a7f8a7f8a2e19b6, 0x23d3767e64b2f98a, 0xff768d7e4f9d86a4, 0x23d3767e64b2f984, 0xccd1837e334e4aa6, 0x23d3767e64b2f99a, 0x7691fd7e028f6754, 0x34ad3b1041204318, 0xa29e749ea9d201c8, 0x34ad3b104120431b, 0xa29e779ea9d206e1, 0x34ad3b104120431a, 0xa29e769ea9d2052e, 0x02a17ebca4aa3497, 0x229ef18bcd375c95, 0x02a17dbca4aa32c8, 0x229b6f8bcd3449d8, 0x02a184bca4aa3ed5, 0x22b3618bcd48c3ef, 0x5c2c346706186f36, 0xb78c410f5b84f8c2, 0xed9478212b267395, 0xd9bbb55c5256662f, 0x8c54f0203249438a, 0xbd9790b5727dc37e, 0xa64e5f36c9e2b0e3, 0x8fd0680da3088a04, 0x67aad32c078284cc, 0xb37d55d81c57b331, 0x55ac0f3829057c43, 0xcb27f4b8e1b6cc20, 0x26caf88bcbef2d19, 0x8e6e063b97e61b8f, 0xb42750f7f3b7c37e, 0xf3c6ba64cf7ca99b, 0xebfb69b427ea80fe, 0x39b50c3ed970f46c, 0x5b9b177aa3eb3e8a, 0x6510063ecf4ec903, 0x2b3bbd2c00797c7a, 0xf1d6204ff5cb4aa7, 0x4836e27ccf099f38, 0x82efbb0dd073b44d, 0x4a80c282ffd7d4c6, 0x305d1a9c9ee43bdf, 0x15c366948ffc6997, 0x80153ae218916e7b, 0xfa23e2bdf9e2a9e1, 0xd47e8d8a2333c6de, 0x7e128095f688b056, 0x2f5356890efcedab, 0x95c2b383014f55c5, 0x4727a5339ce6070f, 0xb0555ecd575108e9, 0x48d785770bb4af37, 0x09d4701c12af02b1, 0x79f031e78f3cf62e, 0x52a1ee85db1b5a94, 0x6bd95b2eb37fa6b8, 0x74971b7077aef85d, 0xb4e4fae2ffcc1aad, 0x2bd48bd898b8f63a, 0xe9966ac1556257f6, 0x92a3d1cd078ba293, 0xf81175a482e20ab8, 0x5bbb3de722e73048, 0x6b4f363492b9f2be, 0xc2d559df73d59875, 0xf75f62284bc7a8c2, 0xda8dd8e116a9f1cc, 0xbdc1e6ab76057885, 0xfec6a4238a1224a0, 0xc03f40f3223e290e, 0x1ed21673466ffda9, 0xdf70f906bb0dd2af, 0xf3dcda369f2af666, 0x9ebb11573cdcebde, 0x81c72d9077fedca0, 0x0ec074a31be5fb15, 0x2a8b3280b6c48f20, 0xfd31777513309344, 0x194534a86ad006b6, 0x3be6fdf46e0cfe12, 0x017cc137a07eb057, 0x9428fc6e7d26b54d, 0x9aaa2e3603ef8ad7, 0x82c6d3f3a0ccdf7d, 0xc86eeea00cf09b65, 0x705f8189dbb58299, 0x415a7f554391ca69, 0xcfe3d49fa2bdc555, 0xf0f9c56039b25191, 0x7075cb6abd1d32d9, 0x43c94e2c8b277509, 0x3cbfd4e4ea670359, 0xc05887810f4d019d, 0x14468ff93ac22dc5, 0xebed699589d99c05, 0x6d99f6df321ca5d5, 0x0cd410d08c36d625, 0xef1b2a2c86831d35, 0x3b349c4d69ee5f05, 0x55248ce88f45f035, 0xaa69ca6a18a4c885, 0x1fe3fce62bd816b5, 0x0289a488a8df69d9, 0x15e96e1613df98b5, 0xe6be57375ad89b99 },
                new ulong[] { 0xcbf29ce484222325, 0xaf63dc4c8601ec8c, 0xaf63df4c8601f1a5, 0xaf63de4c8601eff2, 0xaf63d94c8601e773, 0xaf63d84c8601e5c0, 0xaf63db4c8601ead9, 0x08985907b541d342, 0xdcb27518fed9d577, 0xdd120e790c2512af, 0xcac165afa2fef40a, 0x85944171f73967e8, 0xaf63bd4c8601b7df, 0x089be207b544f1e4, 0x08a61407b54d9b5f, 0x08a2ae07b54ab836, 0x0891b007b53c4869, 0x088e4a07b5396540, 0x08987c07b5420ebb, 0xdcb28a18fed9f926, 0xdd1270790c25b935, 0xcac146afa2febf5d, 0x8593d371f738acfe, 0x34531ca7168b8f38, 0x08a25607b54a22ae, 0xf5faf0190cf90df3, 0xf27397910b3221c7, 0x2c8c2b76062f22e0, 0xe150688c8217b8fd, 0xf35a83c10e4f1f87, 0xd1edd10b507344d0, 0x2a5ee739b3ddb8c3, 0xdcfb970ca1c0d310, 0x4054da76daa6da90, 0xf70a2ff589861368, 0x4c628b38aed25f17, 0x9dd1f6510f78189f, 0xa3de85bd491270ce, 0x858e2fa32a55e61d, 0x46810940eff5f915, 0xf5fadd190cf8edaa, 0xf273ed910b32b3e9, 0x2c8c5276062f6525, 0xe150b98c821842a0, 0xf35aa3c10e4f55e7, 0xd1ed680b50729265, 0x2a5f0639b3dded70, 0xdcfbaa0ca1c0f359, 0x4054ba76daa6a430, 0xf709c7f5898562b0, 0x4c62e638aed2f9b8, 0x9dd1a8510f779415, 0xa3de2abd4911d62d, 0x858e0ea32a55ae0a, 0x46810f40eff60347, 0xc33bce57bef63eaf, 0x08a24307b54a0265, 0xf5b9fd190cc18d15, 0x4c968290ace35703, 0x07174bd5c64d9350, 0x5a294c3ff5d18750, 0x05b3c1aeb308b843, 0xb92a48da37d0f477, 0x73cdddccd80ebc49, 0xd58c4c13210a266b, 0xe78b6081243ec194, 0xb096f77096a39f34, 0xb425c54ff807b6a3, 0x23e520e2751bb46e, 0x1a0b44ccfe1385ec, 0xf5ba4b190cc2119f, 0x4c962690ace2baaf, 0x0716ded5c64cda19, 0x5a292c3ff5d150f0, 0x05b3e0aeb308ecf0, 0xb92a5eda37d119d9, 0x73ce41ccd80f6635, 0xd58c2c132109f00b, 0xe78baf81243f47d1, 0xb0968f7096a2ee7c, 0xb425a84ff807855c, 0x23e4e9e2751b56f9, 0x1a0b4eccfe1396ea, 0x54abd453bb2c9004, 0x08ba5f07b55ec3da, 0x337354193006cb6e, 0xa430d84680aabd0b, 0xa9bc8acca21f39b1, 0x6961196491cc682d, 0xad2bb1774799dfe9, 0x6961166491cc6314, 0x8d1bb3904a3b1236, 0x6961176491cc64c7, 0xed205d87f40434c7, 0x6961146491cc5fae, 0xcd3baf5e44f8ad9c, 0xe3b36596127cd6d8, 0xf77f1072c8e8a646, 0xe3b36396127cd372, 0x6067dce9932ad458, 0xe3b37596127cf208, 0x4b7b10fa9fe83936, 0xaabafe7104d914be, 0xf4d3180b3cde3eda, 0xaabafd7104d9130b, 0xf4cfb20b3cdb5bb1, 0xaabafc7104d91158, 0xf4cc4c0b3cd87888, 0xe729bac5d2a8d3a7, 0x74bc0524f4dfa4c5, 0xe72630c5d2a5b352, 0x6b983224ef8fb456, 0xe73042c5d2ae266d, 0x8527e324fdeb4b37, 0x0a83c86fee952abc, 0x7318523267779d74, 0x3e66d3d56b8caca1, 0x956694a5c0095593, 0xcac54572bb1a6fc8, 0xa7a4c9f3edebf0d8, 0x7829851fac17b143, 0x2c8f4c9af81bcf06, 0xd34e31539740c732, 0x3605a2ac253d2db1, 0x08c11b8346f4a3c3, 0x6be396289ce8a6da, 0xd9b957fb7fe794c5, 0x05be33da04560a93, 0x0957f1577ba9747c, 0xda2cc3acc24fba57, 0x74136f185b29e7f0, 0xb2f2b4590edb93b2, 0xb3608fce8b86ae04, 0x4a3a865079359063, 0x5b3a7ef496880a50, 0x48fae3163854c23b, 0x07aaa640476e0b9a, 0x2f653656383a687d, 0xa1031f8e7599d79c, 0xa31908178ff92477, 0x097edf3c14c3fb83, 0xb51ca83feaa0971b, 0xdd3c0d96d784f2e9, 0x86cd26a9ea767d78, 0xe6b215ff54a30c18, 0xec5b06a1c5531093, 0x45665a929f9ec5e5, 0x8c7609b4a9f10907, 0x89aac3a491f0d729, 0x32ce6b26e0f4a403, 0x614ab44e02b53e01, 0xfa6472eb6eef3290, 0x9e5d75eb1948eb6a, 0xb6d12ad4a8671852, 0x88826f56eba07af1, 0x44535bf2645bc0fd, 0x169388ffc21e3728, 0xf68aac9e396d8224, 0x8e87d7e7472b3883, 0x295c26caa8b423de, 0x322c814292e72176, 0x8a06550eb8af7268, 0xef86d60e661bcf71, 0x9e5426c87f30ee54, 0xf1ea8aa826fd047e, 0x0babaf9a642cb769, 0x4b3341d4068d012e, 0xd15605cbc30a335c, 0x5b21060aed8412e5, 0x45e2cda1ce6f4227, 0x50ae3745033ad7d4, 0xaa4588ced46bf414, 0xc1b0056c4a95467e, 0x56576a71de8b4089, 0xbf20965fa6dc927e, 0x569f8383c2040882, 0xe1e772fba08feca0, 0x4ced94af97138ac4, 0xc4112ffb337a82fb, 0xd64a4fd41de38b7d, 0x4cfc32329edebcbb, 0x0803564445050395, 0xaa1574ecf4642ffd, 0x694bc4e54cc315f9, 0xa3d7cb273b011721, 0x577c2f8b6115bfa5, 0xb7ec8c1a769fb4c1, 0x5d5cfce63359ab19, 0x33b96c3cd65b5f71, 0xd845097780602bb9, 0x84d47645d02da3d5, 0x83544f33b58773a5, 0x9175cbb2160836c5, 0xc71b3bc175e72bc5, 0x636806ac222ec985, 0xb6ef0e6950f52ed5, 0xead3d8a0f3dfdaa5, 0x922908fe9a861ba5, 0x6d4821de275fd5c5, 0x1fe3fce62bd816b5, 0xc23e9fccd6f70591, 0xc1af12bdfe16b5b5, 0x39e9f18f2f85e221 }
            };

            var hashes = new HashAlgorithm[] { new FNV1Hash32(), new FNV1aHash32(), new FNV1Hash64(), new FNV1aHash64() };

            // Iterate all hashes
            for (int h = 0; h<hashes.Length; h++)
            {
                var hash = hashes[h];

                // Iterate all testvectors
                for (int i = 0; i<testvectors.Length; i++)
                {
                    // We don't use Encoding.Ascii.GetBytes (or any other Encoding) because the original testvectors treat the text as raw bytes
                    using (var s = new MemoryStream(testvectors[i].ToCharArray().Select(c => (byte) c).ToArray()))
                    {
                        // Compute hash & convert to ulong
                        var value = hash.ComputeHash(s);
                        var convertedvalue = hash.HashSize == 32 ? BitConverter.ToUInt32(value, 0) : BitConverter.ToUInt64(value, 0);

                        // Compare computed hash against known check value
                        if (convertedvalue != checkvalues[h][i])
                        {
                            Console.WriteLine($"TEST FAILED for {hash.GetType().Name}, vector {i}: [{testvectors[i].Replace("\0", "\\0")}], expected: {checkvalues[h][i]}, actual: {value}");
                        }
                    }
                }
            }
        }//TestFNVHash


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
            public string validateRoot = "";//tex points to a fileType folder in repo. TODO: (actually it currently points to the hash_types.json) split into validateRoot, validateHashTypes (pointing to hash types .json)

            public bool matchedStringsNameIsDictionary = false;//tex Should only be used when input strings are a folder, else it will overwrite the input strings file.
            //By default matched strings will be written to <inputStringsPath>Strings_matchedStrings.txt (if input strings are a folder), 
            //when set to true matched strings will be written to <inputStringsPath>.txt , which saves hasle of having to rename stuff if your workflow is for updating the mgsv-strings github repo

            public bool buildToolDictionaries = false;//tex combine the mgsv-lookup-strings repo Dictionaries\{gameId}\{hashName} dictionaries to too dictionary name {fileType}_{hashName}_dictionary.txt
            //should be used in conjunction with validateMode
            //public bool buildHashTypeSets = false;//tex TODO DEBUGNOW build condensed lists of hashes by hashType
        }//RunSettings

        static void Main(string[] args)
        {
            TestFNVHash();//DEBUGNOW

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
                ValidateMode(runSettings);
                //tex suppose I'll gate it in validate mode
                if (runSettings.buildToolDictionaries)
                {
                    BuildToolDictionaries(runSettings);
                }
            } else {
                HashWrangle(runSettings);
            }

            Console.WriteLine("All done");
        }//Main

        private static void HashWrangle(RunSettings runSettings)
        {
            HashFunction HashFunc;
            try
            {
                HashFunc = hashFuncs[runSettings.funcType.ToLower()];
            }
            catch (KeyNotFoundException)
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
            BuildOutput(runSettings, hashMatches, unmatchedStrings, runSettings.funcType);
        }//HashWrangle

        private static void ValidateMode(RunSettings runSettings)
        {
            //tex clear for run, BuildOutput will append for each call
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
                    BuildOutput(runSettings, hashMatches, unmatchedStrings, hashType);
                }// for hashtypes
            }// for hashesGamePaths
        }//ValidateMode

        //ASSUMPTION: mgsv-lookup-strings repo layout of dictionaries at {fileType}\Dictionaries\{gameId}...\{hashName}.txt
        //only want to run this after validate has updated the dictionaries
        //TODO: review the current state of tools dictionary names/usage (some combine multiple hashName dicts)
        private static void BuildToolDictionaries(RunSettings runSettings)
        {
            //REF validateRoot=D:\GitHub\mgsv-lookup-strings\lba\lba_hash_types.json
            string rootPath = Path.GetDirectoryName(runSettings.validateRoot);
            string fileType = new DirectoryInfo(rootPath).Name;
            string dictionariesPath = rootPath + "\\" + "Dictionaries";

            string jsonString = File.ReadAllText(runSettings.validateRoot, Encoding.UTF8);
            var hashTypes = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);

            Dictionary<string, HashSet<string>> dictionaries = new Dictionary<string, HashSet<string>>();//k=hashType,v=combined dictionary

            foreach (var entry in hashTypes)
            {
                string hashName = entry.Key;
                dictionaries.Add(hashName, new HashSet<string>());
            }//foreach hashTypes

            string[] dictionariesGamePaths = Directory.GetDirectories(dictionariesPath);
            foreach (string gameIdPath in dictionariesGamePaths)
            {
                //REF gameIdPath=D:\GitHub\mgsv-lookup-strings\lba\Dictionaries\TPP
                string gameId = new DirectoryInfo(gameIdPath).Name;
                foreach (var entry in hashTypes)
                {
                    string hashName = entry.Key;
                    string dictPath = Path.Combine(gameIdPath, $"{hashName}.txt");
                    var lines = File.ReadLines(dictPath);
                    foreach (var line in lines)
                    {
                        dictionaries[hashName].Add(line);
                    }//foreach line
                }//foreach hashTypes
            }//for dictionariesGamePaths

            foreach(var entry in dictionaries)
            {
                string hashName = entry.Key;
                var dictionary = entry.Value.ToList();
                dictionary.Sort();

                string fileName = $"{fileType}_{hashName}_dictionary.txt".ToLower();
                string dictionaryPath = Path.Combine(dictionariesPath, fileName);
                File.WriteAllLines(dictionaryPath, dictionary);
            }//foreach dictionaries
        }//BuildToolDictionaries

        private static void BuildOutput(RunSettings runSettings, Dictionary<string, ConcurrentDictionary<string, bool>> hashMatches, ConcurrentQueue<string> unmatchedStringsQueue, string hashType)
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
                    sw.WriteLine(hashType);
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

                        foreach (var testExtension in FoxEngine.FileExtensions)
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
        }//FixupPath

        public static bool IsHex(String s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                // Check if the character is invalid
                if ((ch < '0' || ch > '9') &&
                    (ch < 'A' || ch > 'F') &&
                    (ch < 'a' || ch > 'f'))
                {
                    return false;
                }
            }// for char in string
            return true;
        }//IsHex
    }//Program
}
