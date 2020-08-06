using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageSetCompression.ConsoleApp {
	public class Program {
		private static void Main(string[] args) {
			Console.WriteLine("Compress or decompress files?");
			string input = Console.ReadLine();

			bool compress = input.StartsWith("c", StringComparison.InvariantCultureIgnoreCase);

			ICollection<string> paths;
			if (args.Length == 0) {
				Console.WriteLine("Enter:");
				Console.WriteLine($"- The path to directory containing all files to be {(compress ? "" : "de")}compressed, OR");
				Console.WriteLine($"- The path to each file to be {(compress ? "" : "de")}compressed, and press enter twice to finish.");
				paths = new List<string>();
				string nextPath;
				while (!string.IsNullOrWhiteSpace(nextPath = Console.ReadLine())) {
					if (Directory.Exists(nextPath)) {
						paths = Directory.GetFiles(nextPath);
						break;
					} else {
						paths.Add(nextPath);
					}
				}
			} else {
				paths = args;
			}
			Console.WriteLine("Enter the path of the result folder:");
			string resultFolder = Console.ReadLine();

			int top = Console.WindowHeight - 1;
			int width = Console.WindowWidth - "[...%] ".Length;

			var progress = new Progress<float>(p => {
				Console.SetCursorPosition(0, top);
				Console.Write($"[{p,4:P0}] {new string('#', (int) (p * width))}");
			});

			var setImages = new List<string>(paths.Skip(1));
			if (compress) {
				ImageSetCompressor.CompressSet(paths.First(), setImages, resultFolder, progress);
			} else {
				ImageSetCompressor.DecompressSet(paths.First(), setImages, resultFolder, progress);
			}
		}
	}
}
