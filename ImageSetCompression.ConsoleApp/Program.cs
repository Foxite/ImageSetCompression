using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ImageSetCompression.ConsoleApp {
	public sealed class Program {
		private static void Main(string[] args) {
			Console.WriteLine("Compress or decompress files?");
			string input = Console.ReadLine();

			bool compress = input.StartsWith("c", StringComparison.InvariantCultureIgnoreCase);

			IList<string> paths;
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

			Algorithm algorithm = Algorithm.BuiltInAlgorithms[ConsoleChoiceMenu("Enter the number of the algorithm you want to use:", Algorithm.BuiltInAlgorithms.ListSelect(algo => algo.Name))];
			
			int top = Console.WindowHeight - 1;
			int width = Console.WindowWidth - "[...%] [".Length - "]".Length;

			var progress = new Progress<float>(p => {
				Console.SetCursorPosition(0, top);
				Console.Write($"[{p,4:P0}] [{new string('#', (int) (p * width))}]");
			});

			if (compress) {
				ImageSetCompressor.CompressSet(algorithm, new ReadOnlyCollection<string>(paths), resultFolder, progress);
			} else {
				ImageSetCompressor.DecompressSet(algorithm, new ReadOnlyCollection<string>(paths), resultFolder, progress);
			}
		}

		public static int ConsoleChoiceMenu(string question, params string[] options) => ConsoleChoiceMenu(question, options);
		public static int ConsoleChoiceMenu(string question, IReadOnlyList<string> options) {
			Console.WriteLine(question);
			for (int i = 0; i < options.Count; i++) {
				Console.WriteLine($"[{i + 1}] {options[i]}");
			}

			int choice;
			while (!(int.TryParse(Console.ReadLine(), out choice) && choice >= 1 && choice <= options.Count)) {
				Console.WriteLine("Enter the number of the option you want.");
			}

			return choice - 1;
		}
	}
}
